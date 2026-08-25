using System;
using BlockChain.Models;
using BlockChain.Services;

namespace BlockChain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            BlockChainService coin = new BlockChainService(difficulty: 2, miningReward: 100);

            Wallet alice = new Wallet();
            Wallet bob = new Wallet();
            Wallet charlie = new Wallet();

            coin.MinePendingTransactions(alice.Address);

            Console.WriteLine($"Початковий баланс Аліси: {coin.GetBalanceOfAddress(alice.Address)}");

            Transaction tx1 = new Transaction(alice.Address, bob.Address, 100);
            tx1.SignTransaction(alice);
            Console.WriteLine("\nСпроба відправити перші 100 монет Бобу:");
            coin.AddTransaction(tx1);

            Transaction tx2 = new Transaction(alice.Address, charlie.Address, 100);
            tx2.SignTransaction(alice);
            Console.WriteLine("\nСпроба відправити ще 100 монет Карло (поки перша транзакція в Mempool):");
            coin.AddTransaction(tx2);
        }
    }
}