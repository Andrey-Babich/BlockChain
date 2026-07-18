using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockChain.Models;
using System.Threading.Tasks;

namespace BlockChain.Services
{
    public class BlockChainDisplayService
    {
        public void ShowBlockChain(List<Block> chain)
        {
            foreach (var block in chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"TimeStamp: {block.TimeStamp}");
                Console.WriteLine($"Data: {block.Data}");
                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine($"Previous Hash: {block.PrevHash}");
                Console.WriteLine(new string('-', 50));
            }
        }

        public void ShowValidationResult(bool isValid)
        {
            if (isValid)
            {
                Console.WriteLine("The blockchain is valid.");
            }
            else
            {
                Console.WriteLine("The blockchain is NOT valid.");
            }
        }
    }
}
