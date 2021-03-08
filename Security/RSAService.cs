using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;


namespace SP.StudioCore.Security
{
    public static class RSAService
    {

        private static Encoding encoding = Encoding.UTF8;



        private static AsymmetricKeyParameter CreateKEY(string key, bool isPrivate = true)
        {
            byte[] keyInfoByte = Convert.FromBase64String(key);

            if (isPrivate)
                return PrivateKeyFactory.CreateKey(keyInfoByte);
            else
                return PublicKeyFactory.CreateKey(keyInfoByte);
        }

        public static string Sign(string content, string privatekey, string SignerSymbol = "SHA1WithRSA")
        {
            ISigner sig = SignerUtilities.GetSigner(SignerSymbol);
            var bytes = encoding.GetBytes(content);
            sig.Init(true, CreateKEY(privatekey));

            sig.BlockUpdate(bytes, 0, bytes.Length);
            byte[] signature = sig.GenerateSignature();

            /* Base 64 encode the sig so its 8-bit clean */
            var signedString = Convert.ToBase64String(signature);
            return signedString;
        }

        public static bool Verify(string content, string signData, string publickey, string SignerSymbol = "SHA1WithRSA")
        {
            ISigner signer = SignerUtilities.GetSigner(SignerSymbol);

            signer.Init(false, CreateKEY(publickey, false));

            var expectedSig = Convert.FromBase64String(signData);

            /* Get the bytes to be signed from the string */
            var msgBytes = encoding.GetBytes(content);

            /* Calculate the signature and see if it matches */
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            return signer.VerifySignature(expectedSig);
        }


        /// <summary>
        /// 创建RSA 密钥
        /// </summary>
        /// <returns>string[] 0:私钥;1:公钥</returns>  
        public static string[] Create()
        {
            //RSA密钥对的构造器
            RsaKeyPairGenerator keyGenerator = new RsaKeyPairGenerator();
            //RSA密钥构造器的参数
            RsaKeyGenerationParameters param = new RsaKeyGenerationParameters(
                     Org.BouncyCastle.Math.BigInteger.ValueOf(3),
              new Org.BouncyCastle.Security.SecureRandom(),
               1024,   //密钥长度
               25);
            //用参数初始化密钥构造器
            keyGenerator.Init(param);
            //产生密钥对
            AsymmetricCipherKeyPair keyPair = keyGenerator.GenerateKeyPair();

            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keyPair.Private);
            pemWriter.Writer.Flush();
            string privateKey = textWriter.ToString();


            TextWriter textpubWriter = new StringWriter();
            PemWriter pempubWriter = new PemWriter(textpubWriter);
            pempubWriter.WriteObject(keyPair.Public);
            pempubWriter.Writer.Flush();
            string pubKey = textpubWriter.ToString();

            string[] keys = new string[2];
            keys[0] = privateKey;
            keys[1] = pubKey;
            return keys;
        }

  


    }
}