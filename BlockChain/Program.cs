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

            BlockChainService coin = new BlockChainService(difficulty: 2, miningReward: 50);

            Wallet alice = new Wallet();
            Wallet bob = new Wallet();

            Console.WriteLine($"Адреса Аліси: {alice.Address}");
            Console.WriteLine($"Адреса Боба:  {bob.Address}\n");

            Transaction tx = new Transaction(alice.Address, bob.Address, 25);
            tx.SignTransaction(alice);

            coin.AddTransaction(tx);
            coin.MinePendingTransactions(bob.Address);
        }
    }
}