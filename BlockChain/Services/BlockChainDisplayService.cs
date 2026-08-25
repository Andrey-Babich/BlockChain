using System;
using BlockChain.Models;

namespace BlockChain.Services
{
    public static class BlockChainDisplayService
    {
        public static void PrintAccountStatement(BlockChainService blockchain, string address)
        {
            decimal totalReceived = 0;
            decimal totalSent = 0;
            int txCount = 0;

            foreach (Block block in blockchain.Chain)
            {
                foreach (Transaction tx in block.Transactions)
                {
                    bool involved = false;
                    if (tx.To == address)
                    {
                        totalReceived += tx.Amount;
                        involved = true;
                    }
                    if (tx.From == address)
                    {
                        totalSent += tx.Amount;
                        involved = true;
                    }
                    if (involved)
                    {
                        txCount++;
                    }
                }
            }

            decimal currentBalance = totalReceived - totalSent;

            Console.WriteLine($"\n================ ВИПИСКА ПО РАХУНКУ ================");
            Console.WriteLine($"Адреса:             {address}");
            Console.WriteLine($"Отримано монет:     {totalReceived}");
            Console.WriteLine($"Відправлено монет: {totalSent}");
            Console.WriteLine($"Поточний баланс:   {currentBalance}");
            Console.WriteLine($"Кількість транзакцій: {txCount}");
            Console.WriteLine($"====================================================\n");
        }
    }
}