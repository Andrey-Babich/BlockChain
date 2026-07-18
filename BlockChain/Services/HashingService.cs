using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class HashingService
    {
        // Мясоробку для блоків, яка приймає блок і повертає його хеш
        public string ComputeHash(Block block)
        {
            var input = $"{block.Index}{block.TimeStamp.ToString("o")}{block.Data}{block.PrevHash}";
            return ComputeHash(input);
        }

        // Мясоробку для рядків, яка приймає рядок і повертає його хеш
        public string ComputeHash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
