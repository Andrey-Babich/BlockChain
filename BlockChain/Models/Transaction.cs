using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockChain.Models
{
    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public string Signature { get; set; }
        public string SenderPublicKey { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string Sender
        {
            get => From;
            set => From = value;
        }

        public string Recipient
        {
            get => To;
            set => To = value;
        }

        public Transaction(string from, string to, decimal amount, string signature = "", string senderPublicKey = "")
        {
            From = from;
            To = to;
            Amount = amount;
            Signature = signature;
            SenderPublicKey = senderPublicKey;
            Timestamp = DateTime.Now;
            Id = Guid.NewGuid().ToString("N");
        }

        public Transaction() { }

        public bool VerifySignature()
        {
            if (string.IsNullOrEmpty(Signature) || string.IsNullOrEmpty(SenderPublicKey))
            {
                return true;
            }

            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes($"{From}{To}{Amount}");
                byte[] signatureBytes = Convert.FromBase64String(Signature);

                using (var rsa = RSA.Create())
                {
                    rsa.FromXmlString(SenderPublicKey);
                    return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}