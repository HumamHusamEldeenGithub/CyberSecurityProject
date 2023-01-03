using System;
using System.Security.Cryptography;
using System.Text;
namespace Client
{
    class RsaEncryption
    {
        public RSAParameters publicKey;
        public RSAParameters privateKey;

        public RsaEncryption()
        {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))
            {
                this.publicKey = RSA.ExportParameters(false);
                this.privateKey = RSA.ExportParameters(true);
            }
        }

        public RsaEncryption(string key , bool includePrivateKey)
        {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))
            {
                RSA.FromXmlString(key);
                this.publicKey = RSA.ExportParameters(false);
                if (includePrivateKey)
                    this.privateKey = RSA.ExportParameters(includePrivateKey);
            }
        }

        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }

        public static byte[] CreateSignature(byte[] data, RSAParameters RSAKeyInfo)
        {
            byte[] signedHash;

            SHA256 alg = SHA256.Create();
            byte[] hash = alg.ComputeHash(data);

            // Generate signature
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
            {
                rsa.ImportParameters(RSAKeyInfo);

                RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(rsa);
                RSAFormatter.SetHashAlgorithm(nameof(SHA256));

                signedHash = RSAFormatter.CreateSignature(hash);
            }
            return signedHash;
        }

        public static bool VerifySignature(byte[] data, RSAParameters RSAKeyInfo, byte[] signedHash)
        {
            SHA256 alg = SHA256.Create();
            byte[] hash = alg.ComputeHash(data);
            // Verify signature
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
            {
                rsa.ImportParameters(RSAKeyInfo);

                RSAPKCS1SignatureDeformatter RSADeormatter = new RSAPKCS1SignatureDeformatter(rsa);
                RSADeormatter.SetHashAlgorithm(nameof(SHA256));

                if (RSADeormatter.VerifySignature(hash, signedHash))
                {
                    Console.WriteLine("The signature is valid.");
                    return true;
                }
                else
                {
                    Console.WriteLine("The signature is not valid.");
                    return false;
                }
            }
        }
    }
}
