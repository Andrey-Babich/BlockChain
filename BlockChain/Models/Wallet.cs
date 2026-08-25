using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockChain.Models
{
    public class Wallet
    {
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }
        public string Address { get; private set; }

        public Wallet()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                PrivateKey = rsa.ToXmlString(true);
                PublicKey = rsa.ToXmlString(false);
            }
            Address = DeriveAddress(PublicKey);
        }

        public static string DeriveAddress(string publicKey)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(publicKey));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString().Substring(0, 40);
            }
        }

        public byte[] Sign(string data)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(PrivateKey);
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                return rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }

        public static bool VerifySignature(string publicKey, string data, byte[] signature)
        {
            if (signature == null || string.IsNullOrEmpty(publicKey)) return false;
            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(publicKey);
                    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                    return rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}