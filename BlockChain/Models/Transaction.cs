using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockChain.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public int UnlockBlockIndex { get; set; }
        public bool IsVip { get; set; }
        public string SenderPublicKey { get; set; }
        public byte[] Signature { get; set; }

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

        public Transaction(string from, string to, decimal amount, int unlockBlockIndex = 0, bool isVip = false)
        {
            From = from;
            To = to;
            Amount = amount;
            Timestamp = DateTime.Now;
            UnlockBlockIndex = unlockBlockIndex;
            IsVip = isVip;
            Id = GenerateHashId();
        }

        public string GenerateHashId()
        {
            string rawData = $"{From}{To}{Amount}{Timestamp:yyyy-MM-dd HH:mm:ss.fff}";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public void SignTransaction(Wallet wallet)
        {
            if (wallet.Address != From)
            {
                throw new InvalidOperationException("Помилка підпису.");
            }
            SenderPublicKey = wallet.PublicKey;
            Signature = wallet.Sign(Id);
        }
    }
}