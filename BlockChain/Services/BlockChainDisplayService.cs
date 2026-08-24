using System;
using BlockChain.Models;

namespace BlockChain.Services
{
    public static class BlockChainDisplayService
    {
        public static void DisplayChain(BlockChainService blockChain)
        {
            Console.WriteLine("\n=================== СТАН БЛОКЧЕЙНУ ===================");
            foreach (Block block in blockChain.Chain)
            {
                Console.WriteLine($"\n--- Блок #{block.Index} ---");
                Console.WriteLine($"Час:           {block.Timestamp}");
                Console.WriteLine($"Попередній Хеш:{block.PreviousHash}");
                Console.WriteLine($"Хеш:           {block.Hash}");
                Console.WriteLine($"Nonce:         {block.Nonce}");
                Console.WriteLine("Транзакції:");

                if (block.Transactions.Count == 0)
                {
                    Console.WriteLine("  (Немає транзакцій / Генезис-блок)");
                }
                else
                {
                    foreach (Transaction tx in block.Transactions)
                    {
                        string sender = string.IsNullOrEmpty(tx.Sender) ? "[Системна Винагорода]" : tx.Sender;
                        string lockInfo = tx.UnlockBlockIndex > 0 ? $" | LockUntilBlock: {tx.UnlockBlockIndex}" : "";
                        string idShort = string.IsNullOrEmpty(tx.Id) ? "N/A" : tx.Id[..8];
                        Console.WriteLine($"  * [{idShort}] Від: {sender} -> До: {tx.Recipient} | Сума: {tx.Amount}{lockInfo}");
                    }
                }
            }
            Console.WriteLine("\n=======================================================\n");
        }
    }
}