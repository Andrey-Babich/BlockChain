using System;
using System.Linq;
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

            coin.MinePendingTransactions(alice.Address);

            Transaction tx = new Transaction(alice.Address, bob.Address, 10, fee: 5);
            tx.SignTransaction(alice);
            coin.AddTransaction(tx);

            coin.MinePendingTransactions(bob.Address);

            Console.WriteLine($"Перевірка валідності (чесна нагорода + комісія): {coin.IsChainValid()}");

            Transaction systemTx = coin.Chain[2].Transactions.First(t => t.From == "SYSTEM" || string.IsNullOrEmpty(t.From));
            systemTx.Amount += 100;
            MiningService.MineBlock(coin.Chain[2], coin.Difficulty);

            Console.WriteLine($"Перевірка валідності після підробки системної транзакції: {coin.IsChainValid()}");
        }
    }
}