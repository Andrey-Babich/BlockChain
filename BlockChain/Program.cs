using System;
using System.Threading.Tasks;
using BlockChain.Models;
using BlockChain.Services;

namespace BlockChain
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            string nodeName = args.Length > 0 ? args[0] : "NodeA";
            int port = args.Length > 1 ? int.Parse(args[1]) : 5001;

            Console.Title = $"{nodeName} (Port: {port})";

            BlockChainService chainService = new BlockChainService(difficulty: 2);
            P2PService p2pService = new P2PService(chainService, port);

            p2pService.StartServer();

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine($"  ВУЗОЛ: {nodeName} | ПОРТ: {port}");
                Console.WriteLine("==================================================\n");

                Console.WriteLine("1. Переглянути Blockchain");
                Console.WriteLine("2. Переглянути Mempool");
                Console.WriteLine("3. Створити транзакцію (локально)");
                Console.WriteLine("4. Змайнити блок");
                Console.WriteLine("5. Відправити транзакцію іншій ноді");
                Console.WriteLine("6. Переглянути поточний стан ноди");
                Console.WriteLine("7. Відправити останній блок іншій ноді");
                Console.WriteLine("8. Запустити синхронізацію Blockchain");
                Console.WriteLine("0. Вихід");
                Console.Write("\nОберіть пункт: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        ShowBlockchain(chainService);
                        break;
                    case "2":
                        ShowMempool(chainService);
                        break;
                    case "3":
                        CreateLocalTransaction(chainService);
                        break;
                    case "4":
                        MineBlock(chainService);
                        break;
                    case "5":
                        await SendTransactionToPeer(chainService, p2pService);
                        break;
                    case "6":
                        ShowNodeStatus(nodeName, port, chainService);
                        break;
                    case "7":
                        await SendBlockToPeer(chainService, p2pService);
                        break;
                    case "8":
                        await SyncChainWithPeer(p2pService);
                        break;
                    case "0":
                        exit = true;
                        continue;
                    default:
                        Console.WriteLine("Невірний вибір.");
                        break;
                }

                Console.WriteLine("\nНатисніть Enter для продовження...");
                Console.ReadLine();
            }
        }

        static void ShowBlockchain(BlockChainService service)
        {
            Console.WriteLine($"=== BLOCKCHAIN (Блоків: {service.Chain.Count}) ===");
            foreach (var block in service.Chain)
            {
                Console.WriteLine($"Блок #{block.Index}");
                Console.WriteLine($"  Час: {block.Timestamp}");
                Console.WriteLine($"  Previous Hash: {block.PreviousHash}");
                Console.WriteLine($"  Merkle Root: {block.MerkleRoot}");
                Console.WriteLine($"  Hash: {block.Hash}");
                Console.WriteLine($"  Nonce: {block.Nonce}");
                Console.WriteLine($"  Транзакцій у блоці: {block.Transactions.Count}");
                Console.WriteLine("---------------------------------------------");
            }
        }

        static void ShowMempool(BlockChainService service)
        {
            Console.WriteLine($"=== MEMPOOL (Транзакцій: {service.PendingTransactions.Count}) ===");
            foreach (var tx in service.PendingTransactions)
            {
                Console.WriteLine($"  [ID: {tx.Id}] {tx.From} -> {tx.To}: {tx.Amount}");
            }
        }

        static void CreateLocalTransaction(BlockChainService service)
        {
            Console.Write("Відправник: ");
            string sender = Console.ReadLine();
            Console.Write("Отримувач: ");
            string recipient = Console.ReadLine();
            Console.Write("Сума: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                var tx = new Transaction(sender, recipient, amount);
                if (service.AddTransaction(tx))
                {
                    Console.WriteLine($"Транзакція додана в Mempool. ID: {tx.Id}");
                }
                else
                {
                    Console.WriteLine("Помилка створення транзакції.");
                }
            }
            else
            {
                Console.WriteLine("Некоректна сума.");
            }
        }

        static void MineBlock(BlockChainService service)
        {
            Console.WriteLine("Майнінг нового блоку...");
            service.MinePendingTransactions("Miner1");
            Console.WriteLine($"Блок успішно змайнено! Хеш: {service.GetLatestBlock().Hash}");
            Console.WriteLine($"Merkle Root: {service.GetLatestBlock().MerkleRoot}");
        }

        static async Task SendTransactionToPeer(BlockChainService service, P2PService p2p)
        {
            if (service.PendingTransactions.Count == 0)
            {
                Console.WriteLine("Mempool порожній!");
                return;
            }

            Console.WriteLine("Оберіть номер транзакції:");
            for (int i = 0; i < service.PendingTransactions.Count; i++)
            {
                var t = service.PendingTransactions[i];
                Console.WriteLine($"{i + 1}. [ID: {t.Id}] {t.From} -> {t.To}: {t.Amount}");
            }

            Console.Write("Номер: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= service.PendingTransactions.Count)
            {
                var tx = service.PendingTransactions[index - 1];
                Console.Write("Введіть порт ноди-отримувача: ");
                if (int.TryParse(Console.ReadLine(), out int targetPort))
                {
                    bool success = await p2p.SendTransactionAsync(targetPort, tx);
                    Console.WriteLine(success ? "Транзакцію успішно передано!" : "Не вдалося передати транзакцію.");
                }
            }
        }

        static async Task SendBlockToPeer(BlockChainService service, P2PService p2p)
        {
            var latestBlock = service.GetLatestBlock();
            if (latestBlock.Index == 0)
            {
                Console.WriteLine("Неможливо відправити Genesis-блок!");
                return;
            }

            Console.Write("Введіть порт ноди-отримувача: ");
            if (int.TryParse(Console.ReadLine(), out int targetPort))
            {
                string result = await p2p.SendBlockAsync(targetPort, latestBlock);
                Console.WriteLine($"Відповідь ноди: {result}");
            }
        }

        static async Task SyncChainWithPeer(P2PService p2p)
        {
            Console.Write("Введіть порт ноди для синхронізації: ");
            if (int.TryParse(Console.ReadLine(), out int targetPort))
            {
                Console.WriteLine("Синхронізація...");
                string result = await p2p.SyncChainAsync(targetPort);
                Console.WriteLine(result);
            }
        }

        static void ShowNodeStatus(string name, int port, BlockChainService service)
        {
            Console.WriteLine("=== ПОТОЧНИЙ СТАН НОДИ ===");
            Console.WriteLine($"Назва ноди: {name}");
            Console.WriteLine($"Порт: {port}");
            Console.WriteLine($"Кількість блоків: {service.Chain.Count}");
            Console.WriteLine($"Hash останнього блоку: {service.GetLatestBlock().Hash}");
            Console.WriteLine($"Merkle Root останнього блоку: {service.GetLatestBlock().MerkleRoot}");
            Console.WriteLine($"Транзакцій у Mempool: {service.PendingTransactions.Count}");
            Console.WriteLine($"Складність (Difficulty): {service.Difficulty}");
        }
    }
}