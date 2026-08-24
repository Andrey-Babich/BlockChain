using System;
using System.Collections.Generic;
using System.Linq;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; private set; }
        public List<Transaction> Mempool { get; private set; }
        public int Difficulty { get; set; }
        public decimal MiningReward { get; set; }
        public int MaxBlockSize { get; set; } = 3;

        public BlockChainService(int difficulty = 2, decimal miningReward = 50)
        {
            Chain = new List<Block>();
            Mempool = new List<Transaction>();
            Difficulty = difficulty;
            MiningReward = miningReward;

            CreateGenesisBlock();
        }

        private void CreateGenesisBlock()
        {
            Block genesisBlock = new Block(DateTime.Now, new List<Transaction>(), "0")
            {
                Index = 0
            };
            MiningService.MineBlock(genesisBlock, Difficulty);
            Chain.Add(genesisBlock);
        }

        public Block GetLatestBlock()
        {
            return Chain[^1];
        }

        public void AddTransaction(Transaction tx)
        {
            bool isDuplicate = Chain.Any(block => block.Transactions.Any(t => t.Id == tx.Id));
            if (isDuplicate)
            {
                Console.WriteLine($"[ПОМИЛКА] Помилка: Дублікат транзакції ({tx.Id[..8]}...)! Транзакцію відхилено.");
                throw new InvalidOperationException("Дублікат транзакції");
            }

            Mempool.Add(tx);
            Console.WriteLine($"[MEMPOOL] Транзакцію [{tx.Id[..8]}...] від {tx.Sender} додано до Mempool (UnlockBlockIndex: {tx.UnlockBlockIndex}).");
        }

        public void MinePendingTransactions(string minerAddress)
        {
            int nextBlockIndex = Chain.Count;

            var eligibleTransactions = Mempool
                .Where(tx => tx.UnlockBlockIndex <= nextBlockIndex)
                .Take(MaxBlockSize)
                .ToList();

            if (eligibleTransactions.Count == 0 && Mempool.Count == 0)
            {
                Console.WriteLine("[МАЙНІНГ] Mempool порожній. Немає транзакцій для майнінгу.");
                return;
            }

            var blockTransactions = new List<Transaction>(eligibleTransactions);

            Transaction rewardTx = new Transaction(null, minerAddress, MiningReward);
            blockTransactions.Add(rewardTx);

            Block newBlock = new Block(DateTime.Now, blockTransactions, GetLatestBlock().Hash)
            {
                Index = nextBlockIndex
            };

            Console.WriteLine($"\n--- Початок майнінгу Блоку #{nextBlockIndex} ---");
            Console.WriteLine($"Взето транзакцій з Mempool: {eligibleTransactions.Count} | Залишилось чекати у Mempool: {Mempool.Count - eligibleTransactions.Count}");

            MiningService.MineBlock(newBlock, Difficulty);
            Chain.Add(newBlock);

            foreach (var tx in eligibleTransactions)
            {
                Mempool.Remove(tx);
            }

            Console.WriteLine($"Блок #{newBlock.Index} успішно змайнено! Залишок у Mempool: {Mempool.Count}");
        }

        public decimal GetBalanceOfAddress(string address)
        {
            decimal balance = 0;
            foreach (Block block in Chain)
            {
                foreach (Transaction tx in block.Transactions)
                {
                    if (tx.Sender == address) balance -= tx.Amount;
                    if (tx.Recipient == address) balance += tx.Amount;
                }
            }
            return balance;
        }

        public bool IsChainValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];

                if (currentBlock.Hash != HashingService.CalculateHash(currentBlock)) return false;
                if (currentBlock.PreviousHash != previousBlock.Hash) return false;
            }
            return true;
        }
    }
}