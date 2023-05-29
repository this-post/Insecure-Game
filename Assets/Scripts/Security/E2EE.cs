using System;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;

using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Digests;

using UnityEngine;

using Util;
using Constant;
using HttpManager;

using Newtonsoft.Json;

namespace Security
{
    public class E2eePayload
    {
        public static String Encryption(String plain)
        {
            #if DEBUG
            Debug.Log(plain);
            #endif
            byte[] plainTextByte = Encoding.ASCII.GetBytes(plain);
            byte[] sharedSecret = KeyAgreement.GetSharedSecret();
            byte[] sessionKey = KeyAgreement.GetKdfKey(sharedSecret);
            SecureRandom secureRandom = new SecureRandom();
            byte[] nonce = new byte[CryptoConstant.NonceSize];
            byte[] aad = new byte[CryptoConstant.AadSize];
            secureRandom.NextBytes(nonce);
            secureRandom.NextBytes(aad);
            GcmBlockCipher cipherSpec = new GcmBlockCipher(new AesEngine());
            AeadParameters cipherParams = new AeadParameters(new KeyParameter(sessionKey), CryptoConstant.AadSize * 8, nonce, aad);
            cipherSpec.Init(true, cipherParams);
            byte[] cipherText = new byte[cipherSpec.GetOutputSize(plainTextByte.Length)];
            int offset = cipherSpec.ProcessBytes(plainTextByte, 0, plainTextByte.Length, cipherText, 0);
            cipherSpec.DoFinal(cipherText, offset);
            return Conversion.HexToString(nonce.Concat(cipherText).Concat(aad).ToArray());
        }

        public static String Decryption(String cipher)
        {
            byte[] sharedSecret = KeyAgreement.GetSharedSecret();
            byte[] nonce = Conversion.StringToByteArray(cipher.Substring(0, CryptoConstant.NonceSize * 2));
            byte[] aad = Conversion.StringToByteArray(cipher.Substring(cipher.Length - (CryptoConstant.AadSize * 2)));
            int offsetBetweenNonceAndAad = cipher.Length - (CryptoConstant.NonceSize * 2) - (CryptoConstant.AadSize * 2);
            byte[] cipherText = Conversion.StringToByteArray(cipher.Substring(CryptoConstant.NonceSize * 2, offsetBetweenNonceAndAad));
            byte[] sessionKey = KeyAgreement.GetKdfKey(sharedSecret);
            #if DEBUG
            Debug.Log("Cipher: " + cipher);
            Debug.Log("Session key: " + Conversion.HexToString(sessionKey));
            Debug.Log("Shared secret: " + Conversion.HexToString(sharedSecret));
            Debug.Log("Aad: " + Conversion.HexToString(aad));
            Debug.Log("Nonce: " + Conversion.HexToString(nonce));
            Debug.Log("Cipher: " + Conversion.HexToString(cipherText));
            #endif
            GcmBlockCipher cipherSpec = new GcmBlockCipher(new AesEngine());
            AeadParameters cipherParams = new AeadParameters(new KeyParameter(sessionKey), CryptoConstant.AadSize * 8, nonce, aad);
            cipherSpec.Init(false, cipherParams);
            byte[] plainText = new byte[cipherText.Length];
            int offset = cipherSpec.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
            cipherSpec.DoFinal(plainText, offset);
            return Encoding.UTF8.GetString(plainText);
        }
    }

    public class KeyAgreement
    {
        public static void CreateKeyPair()
        {
            ECKeyPairGenerator keyPairGenerator = (ECKeyPairGenerator)GeneratorUtilities.GetKeyPairGenerator("ECDH");
            keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 521));
            AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();
            ECPublicKeyParameters clientPubKey = (ECPublicKeyParameters)keyPair.Public;
            ECPrivateKeyParameters clientPrivKey = (ECPrivateKeyParameters)keyPair.Private;
            StringWriter sw1 = new StringWriter();
            PemWriter pemWriter1 = new PemWriter(sw1);
            pemWriter1.WriteObject(clientPubKey);
            pemWriter1.Writer.Flush();
            String clientPubKeyPem = sw1.ToString();
            PlayerPrefs.SetString("pubKey", clientPubKeyPem);
            pemWriter1.Writer.Close();
            StringWriter sw2 = new StringWriter();
            PemWriter pemWriter2 = new PemWriter(sw2);
            pemWriter2.WriteObject(clientPrivKey);
            pemWriter2.Writer.Flush();
            String clientPrivKeyPem = sw2.ToString();
            PlayerPrefs.SetString("privKey", clientPrivKeyPem);
            pemWriter2.Writer.Close();
        }

        public static void KeyExchange()
        {
            PublicKeyDto publicKeyDto = new PublicKeyDto();
            publicKeyDto.PublicKey = GetPublicKey();
            String serializedHttpBody = JsonConvert.SerializeObject(publicKeyDto);
            #if TEST
            var (headers, jsonReponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURLs["KeyExchange"]), null, false, serializedHttpBody);
            #else
            var (headers, jsonReponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURLs["KeyExchange"]), null, false, serializedHttpBody);
            #endif
            KeyExchangeDto kexDto = new KeyExchangeDto();
            try
            {
                kexDto = JsonConvert.DeserializeObject<KeyExchangeDto>(jsonReponseText);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
            String keyId = kexDto.KeyId;
            String salt = kexDto.Salt;
            String serverPublicKey = kexDto.ServerPublicKey;
            PlayerPrefs.SetString("keyId", keyId);
            PlayerPrefs.SetString("salt", salt);
            PlayerPrefs.SetString("servPubKey", serverPublicKey);
        }

        public static String GetKeyId()
        {
            String keyId = PlayerPrefs.GetString("keyId");
            if(String.IsNullOrEmpty(keyId))
            {
                throw new PlayerPrefsItemNotFoundException("keyId");
            }
            return keyId;
        }

        internal static byte[] GetSharedSecret()
        {
            String clientPrivateKeyPem = PlayerPrefs.GetString("privKey");
            String serverPublicKeyDer = PlayerPrefs.GetString("servPubKey");
            if(String.IsNullOrEmpty(clientPrivateKeyPem))
            {
                throw new PlayerPrefsItemNotFoundException("privKey");
            }
            if(String.IsNullOrEmpty(serverPublicKeyDer))
            {
                throw new PlayerPrefsItemNotFoundException("servPubKey");
            }
            byte[] serverPublicKeyDerByte = Conversion.StringToByteArray(serverPublicKeyDer);
            PemReader pemReader = new PemReader(new StringReader(clientPrivateKeyPem));
            AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            ECPrivateKeyParameters ecPrivKey = (ECPrivateKeyParameters)keyPair.Private;
            ECPublicKeyParameters ecPubKey = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(serverPublicKeyDerByte);
            IBasicAgreement aKeyAgree = AgreementUtilities.GetBasicAgreement("ECDH");
            aKeyAgree.Init(ecPrivKey);
            // https://github.com/cose-wg/cose-issues/issues/3
            BigInteger sharedSecret = aKeyAgree.CalculateAgreement(ecPubKey); // BoucyCastle's BigInteger will convert the shared secret to BigInt and sometime, they remove leading zero
            // so, prepend 0x00 if the shared secret is not equal to 132/2 bytes
            byte[] sharedSecretByteArray = sharedSecret.ToByteArray();
            if(sharedSecretByteArray.Length != 66)
            {
                int diff = 66 - sharedSecretByteArray.Length;
                byte[] prependedSharedSecret = new byte[sharedSecretByteArray.Length + diff];
                for(int i = 0; i < diff; i++)
                {
                    prependedSharedSecret[i] = 0x00;
                }
                Array.Copy(sharedSecretByteArray, 0, prependedSharedSecret, 1, sharedSecretByteArray.Length);
                #if DEBUG
                Debug.Log("Shared secret (with leadding 0, if any): " + Conversion.HexToString(prependedSharedSecret));
                #endif
                return prependedSharedSecret;
            }
            #if DEBUG
            Debug.Log("Shared secret (no leadding 0): " + Conversion.HexToString(sharedSecret.ToByteArray()));
            #endif
            // return sharedSecret.ToByteArray();
            return sharedSecretByteArray;
        }

        internal static byte[] GetKdfKey(byte[] sharedSecret)
        {
            String salt = PlayerPrefs.GetString("salt");
            if(String.IsNullOrEmpty(salt))
            {
                throw new PlayerPrefsItemNotFoundException("salt");
            }
            var pdb = new Pkcs5S2ParametersGenerator(new Sha256Digest());
            pdb.Init(sharedSecret, Conversion.StringToByteArray(salt), CryptoConstant.Iterations);
            var key = (KeyParameter)pdb.GenerateDerivedMacParameters(CryptoConstant.KdfKeySize * 8);
            return key.GetKey();
        }

        private static String GetPublicKey()
        {
            String publicKeyPem = PlayerPrefs.GetString("pubKey");
            if(String.IsNullOrEmpty(publicKeyPem))
            {
                throw new PlayerPrefsItemNotFoundException("pubKey");
            }
            String EcPublicKeyDerHexStr = EcPemtoDer(publicKeyPem, KeyType.Public);
            return EcPublicKeyDerHexStr;
        }

        private static String GetPrivateKey()
        {
            String privateKeyPem = PlayerPrefs.GetString("privKey");
            if(String.IsNullOrEmpty(privateKeyPem))
            {
                throw new PlayerPrefsItemNotFoundException("privKey");
            }
            String EcPrivateKeyDerHexStr = EcPemtoDer(privateKeyPem, KeyType.Private);
            return EcPrivateKeyDerHexStr;
        }

        public static bool IsKeyPairExists()
        {
            String privateKey = PlayerPrefs.GetString("privKey");
            String publicKey = PlayerPrefs.GetString("pubKey");
            if(String.IsNullOrEmpty(privateKey) || String.IsNullOrEmpty(publicKey))
            {
                return false;
            }
            return true;
        }

        // Can't find available function to do this in BoucyCastle, so, we'll try this
        private static String EcPemtoDer(String pemKey, String type)
        {
            String trim = "";
            if(type == KeyType.Public)
            {
                trim = pemKey.Replace("-----BEGIN PUBLIC KEY-----\n", "")
                        .Replace("\n-----END PUBLIC KEY-----\n", "")
                        .Replace("\n", "");
            }
            if(type == KeyType.Private)
            {
                trim = pemKey.Replace("-----BEGIN EC PRIVATE KEY-----\n", "")
                        .Replace("\n-----END EC PRIVATE KEY-----\n", "")
                        .Replace("\n", "");
            }
            byte[] base64Pem = Convert.FromBase64String(trim);
            return Conversion.HexToString(base64Pem);
        }
    }
}