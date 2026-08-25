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
            if (!TransactionService.Validate(tx))
            {
                Console.WriteLine("[MEMPOOL] Транзакцію відхилено: недійсний підпис.");
                return;
            }

            bool isDuplicate = Chain.Any(block => block.Transactions.Any(t => t.Id == tx.Id))
                              || Mempool.Any(t => t.Id == tx.Id);
            if (isDuplicate)
            {
                Console.WriteLine("Дублікат транзакції");
                return;
            }

            Mempool.Add(tx);
        }

        public void MinePendingTransactions(string minerAddress)
        {
            int nextBlockIndex = Chain.Count;
            Dictionary<string, decimal> pendingBalances = new Dictionary<string, decimal>();
            List<Transaction> validTransactions = new List<Transaction>();

            var candidates = Mempool
                .Where(tx => tx.UnlockBlockIndex <= nextBlockIndex)
                .OrderByDescending(tx => tx.IsVip)
                .ToList();

            foreach (var tx in candidates)
            {
                if (validTransactions.Count >= MaxBlockSize)
                    break;

                if (string.IsNullOrEmpty(tx.Sender) || tx.Sender == "SYSTEM")
                {
                    validTransactions.Add(tx);
                    continue;
                }

                if (!pendingBalances.ContainsKey(tx.Sender))
                {
                    pendingBalances[tx.Sender] = GetBalanceOfAddress(tx.Sender);
                }

                if (pendingBalances[tx.Sender] >= tx.Amount)
                {
                    pendingBalances[tx.Sender] -= tx.Amount;
                    validTransactions.Add(tx);
                }
            }

            var blockTransactions = new List<Transaction>(validTransactions);

            Transaction rewardTx = new Transaction("SYSTEM", minerAddress, MiningReward);
            blockTransactions.Add(rewardTx);

            Block newBlock = new Block(DateTime.Now, blockTransactions, GetLatestBlock().Hash)
            {
                Index = nextBlockIndex
            };

            MiningService.MineBlock(newBlock, Difficulty);
            Chain.Add(newBlock);

            foreach (var tx in validTransactions)
            {
                Mempool.Remove(tx);
            }
        }

        public decimal GetTotalSupply()
        {
            decimal totalSupply = 0;
            foreach (Block block in Chain)
            {
                foreach (Transaction tx in block.Transactions)
                {
                    if (tx.Sender == "SYSTEM" || string.IsNullOrEmpty(tx.Sender))
                    {
                        totalSupply += tx.Amount;
                    }
                }
            }
            return totalSupply;
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