using BlockChain.Models;
using System.Text;

namespace BlockChain.Services
{
    public class HashingService
    {
        public string ComputeHash(Block block)
        {
            var input =
                $"{block.Index}" +
                $"{block.TimeStamp.ToString("o")}" +
                $"{block.Data}" +
                $"{block.PrevHash}" +
                $"{block.Nonce}";

            return ComputeHash(input);
        }

        public string ComputeHash(string input)
        {
            using (var sha256 =
                   System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);

                return BitConverter
                    .ToString(hashBytes)
                    .Replace("-", "")
                    .ToLower();
            }
        }
    }
}