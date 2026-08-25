using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockChain.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public int UnlockBlockIndex { get; set; }
        public bool IsVip { get; set; }
        public string SenderPublicKey { get; set; }
        public byte[] Signature { get; set; }

        public Transaction(string sender, string recipient, decimal amount, int unlockBlockIndex = 0, bool isVip = false)
        {
            Sender = sender;
            Recipient = recipient;
            Amount = amount;
            Timestamp = DateTime.Now;
            UnlockBlockIndex = unlockBlockIndex;
            IsVip = isVip;
            Id = GenerateHashId();
        }

        public string GenerateHashId()
        {
            string rawData = $"{Sender}{Recipient}{Amount}{Timestamp:yyyy-MM-dd HH:mm:ss.fff}";
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
            if (wallet.Address != Sender)
            {
                throw new InvalidOperationException("Неможливо підписати транзакцію чужим гаманцем!");
            }
            SenderPublicKey = wallet.PublicKey;
            Signature = wallet.SignData(Id);
        }
    }
}