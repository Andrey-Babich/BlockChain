using System;
using BlockChain.Models;
using BlockChain.Services;

namespace BlockChain
{
    class Program
    {
        static void Main(string[] args)
        {
            BlockChainService chain = new BlockChainService(difficulty: 2);

            chain.AddTransaction(new Transaction("UserA", "UserB", 50));
            chain.AddTransaction(new Transaction("UserB", "UserC", 20));

            Console.WriteLine("Майнинг блока...");
            chain.MinePendingTransactions("Miner1");

            Console.WriteLine($"Блокчейн валиден: {chain.IsChainValid()}");
        }
    }
}