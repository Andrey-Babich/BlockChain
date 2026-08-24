using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BlockChain.Models;

namespace BlockChain.Services
{
    public static class HashingService
    {
        public static string CalculateHash(Block block)
        {
            string transactionsJson = JsonSerializer.Serialize(block.Transactions);
            string rawData = $"{block.Index}{block.Timestamp:yyyy-MM-dd HH:mm:ss.fff}{block.PreviousHash}{transactionsJson}{block.Nonce}";

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
    }
}