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
        public int HalvingInterval { get; set; } = 2;

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
                Console.WriteLine("Транзакцію відхилено системою безпеки.");
                return;
            }

            bool isDuplicate = Chain.Any(block => block.Transactions.Any(t => t.Id == tx.Id))
                              || Mempool.Any(t => t.Id == tx.Id);
            if (isDuplicate)
            {
                Console.WriteLine("Дублікат транзакції");
                return;
            }

            if (tx.From != "SYSTEM" && !string.IsNullOrEmpty(tx.From))
            {
                decimal currentBalance = GetBalanceOfAddress(tx.From);
                decimal frozenBalance = Mempool.Where(t => t.From == tx.From).Sum(t => t.Amount);
                decimal availableBalance = currentBalance - frozenBalance;

                if (availableBalance < tx.Amount)
                {
                    Console.WriteLine("Недостатньо коштів! Частина вашого балансу вже зарезервована в Mempool");
                    return;
                }
            }

            Mempool.Add(tx);
            Console.WriteLine($"Транзакцію [{tx.Id[..8]}...] додано до Mempool.");
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

                if (string.IsNullOrEmpty(tx.From) || tx.From == "SYSTEM")
                {
                    validTransactions.Add(tx);
                    continue;
                }

                if (!pendingBalances.ContainsKey(tx.From))
                {
                    pendingBalances[tx.From] = GetBalanceOfAddress(tx.From);
                }

                if (pendingBalances[tx.From] >= tx.Amount)
                {
                    pendingBalances[tx.From] -= tx.Amount;
                    validTransactions.Add(tx);
                }
            }

            var blockTransactions = new List<Transaction>(validTransactions);

            int pastHalvings = nextBlockIndex / HalvingInterval;
            decimal currentReward = MiningReward / (decimal)Math.Pow(2, pastHalvings);

            Transaction rewardTx = new Transaction("SYSTEM", minerAddress, currentReward);
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
            decimal minted = 0;
            decimal burned = 0;

            foreach (Block block in Chain)
            {
                foreach (Transaction tx in block.Transactions)
                {
                    if (tx.From == "SYSTEM" || string.IsNullOrEmpty(tx.From))
                    {
                        minted += tx.Amount;
                    }
                    if (tx.To == "BURN")
                    {
                        burned += tx.Amount;
                    }
                }
            }

            return minted - burned;
        }

        public decimal GetBalanceOfAddress(string address)
        {
            decimal balance = 0;
            foreach (Block block in Chain)
            {
                foreach (Transaction tx in block.Transactions)
                {
                    if (tx.From == address) balance -= tx.Amount;
                    if (tx.To == address) balance += tx.Amount;
                }
            }
            return balance;
        }

        public bool IsChainValid()
        {
            if (Chain.Count == 0) return false;

            Block genesisBlock = Chain[0];
            if (genesisBlock.Hash != HashingService.CalculateHash(genesisBlock)) return false;
            if (genesisBlock.PreviousHash != "0" && !string.IsNullOrEmpty(genesisBlock.PreviousHash)) return false;

            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];

                if (currentBlock.Hash != HashingService.CalculateHash(currentBlock)) return false;
                if (currentBlock.PreviousHash != previousBlock.Hash) return false;

                Transaction systemTx = currentBlock.Transactions.FirstOrDefault(tx => tx.From == "SYSTEM" || string.IsNullOrEmpty(tx.From));
                if (systemTx != null)
                {
                    int pastHalvings = currentBlock.Index / HalvingInterval;
                    decimal expectedReward = MiningReward / (decimal)Math.Pow(2, pastHalvings);

                    if (systemTx.Amount > expectedReward)
                    {
                        Console.WriteLine($"[ПОМИЛКА ЕМІСІЇ] Блок #{currentBlock.Index}: очікувалось {expectedReward}, отримано {systemTx.Amount}");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}