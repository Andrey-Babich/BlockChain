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
            Console.WriteLine("=================================================");
            Console.WriteLine("      БЛОКЧЕЙН - ЧАСТИНА 4 (MEMPOOL ТА LOCKTIME) ");
            Console.WriteLine("=================================================\n");

            BlockChainService coin = new BlockChainService(difficulty: 2, miningReward: 50);

            Console.WriteLine(">>> [РІВЕНЬ 1] Перевірка унікальності (Захист від Replay Attack) <<<");
            Transaction tx1 = new Transaction("Alice", "Bob", 10);
            coin.AddTransaction(tx1);
            coin.MinePendingTransactions("Miner-1");

            try
            {
                Console.WriteLine("\nСпроба повторно додати таку саму змайнену транзакцію:");
                coin.AddTransaction(tx1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Успіх: Перехоплено спробу повторної транзакції! ({ex.Message})\n");
            }

            Console.WriteLine(">>> [РІВЕНЬ 2 та 3] Демонстрація Mempool, Ліміту 3 tx/блок та LockTime <<<");

            Console.WriteLine("\n--- 1. Додаємо відкладену транзакцію (UnlockBlockIndex = 5) ---");
            Transaction lockedTx = new Transaction("SecretSender", "Bob", 500, unlockBlockIndex: 5);
            coin.AddTransaction(lockedTx);

            Console.WriteLine("\n--- 2. Додаємо 8 звичайних транзакцій у Mempool ---");
            for (int i = 1; i <= 8; i++)
            {
                coin.AddTransaction(new Transaction($"User{i}", $"Recipient{i}", i * 10));
            }

            Console.WriteLine($"\nВсього чекає у Mempool: {coin.Mempool.Count} транзакцій.");

            Console.WriteLine("\n--- 3. Запускаємо майнінг у циклі ---");
            int round = 1;
            while (coin.Mempool.Count > 0)
            {
                Console.WriteLine($"\n=== Раунд майнінгу #{round} ===");
                coin.MinePendingTransactions("Miner-1");
                round++;
            }

            BlockChainDisplayService.DisplayChain(coin);

            Console.WriteLine($"Чи є ланцюжок валідним? {coin.IsChainValid()}");
        }
    }
}