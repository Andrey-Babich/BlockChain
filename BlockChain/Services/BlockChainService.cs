using System;
using System.Collections.Generic;
using System.Linq;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }
        public int Difficulty { get; set; }
        public List<Transaction> PendingTransactions { get; set; }

        public BlockChainService(int difficulty = 2)
        {
            Chain = new List<Block> { CreateGenesisBlock() };
            Difficulty = difficulty;
            PendingTransactions = new List<Transaction>();
        }

        private Block CreateGenesisBlock()
        {
            Block genesisBlock = new Block(0, new DateTime(2026, 1, 1), new List<Transaction>(), "0");
            genesisBlock.MerkleRoot = HashingService.ComputeMerkleRoot(genesisBlock.Transactions);
            genesisBlock.Hash = HashingService.CalculateHash(genesisBlock);
            return genesisBlock;
        }

        public Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        public bool AddTransaction(Transaction transaction)
        {
            if (!ValidateTransaction(transaction)) return false;
            PendingTransactions.Add(transaction);
            return true;
        }

        public bool ValidateTransaction(Transaction transaction)
        {
            if (transaction == null || string.IsNullOrEmpty(transaction.Id)) return false;
            if (string.IsNullOrEmpty(transaction.From) || string.IsNullOrEmpty(transaction.To)) return false;
            if (transaction.Amount <= 0) return false;

            if (PendingTransactions.Any(t => t.Id == transaction.Id)) return false;

            foreach (var block in Chain)
            {
                if (block.Transactions != null && block.Transactions.Any(t => t.Id == transaction.Id))
                    return false;
            }

            return true;
        }

        public void MinePendingTransactions(string minerAddress)
        {
            Block block = new Block(Chain.Count, DateTime.Now, new List<Transaction>(PendingTransactions), GetLatestBlock().Hash);
            MiningService.MineBlock(block, Difficulty);
            Chain.Add(block);

            ClearMinedTransactions(block.Transactions);
        }

        public string AddReceivedBlock(Block block)
        {
            if (block == null) return "Блок порожній";
            if (block.Index <= GetLatestBlock().Index) return "Такий блок вже існує";
            if (block.Index > GetLatestBlock().Index + 1) return "Нода відстає! Не вистачає попередніх блоків.";
            if (block.PreviousHash != GetLatestBlock().Hash) return "Невірний PreviousHash (розрив ланцюга)";
            if (block.MerkleRoot != HashingService.ComputeMerkleRoot(block.Transactions)) return "Невірний MerkleRoot";
            if (block.Hash != HashingService.CalculateHash(block)) return "Невірний Hash блока";
            if (!block.Hash.StartsWith(new string('0', Difficulty))) return "Блок не відповідає складності Proof of Work";

            Chain.Add(block);
            ClearMinedTransactions(block.Transactions);
            return "OK";
        }

        public bool ReplaceChain(List<Block> newChain)
        {
            if (newChain == null || newChain.Count <= Chain.Count) return false;
            if (!IsChainValid(newChain)) return false;

            Chain = newChain;
            var allChainTxIds = Chain.SelectMany(b => b.Transactions ?? new List<Transaction>()).Select(t => t.Id).ToHashSet();
            PendingTransactions.RemoveAll(t => allChainTxIds.Contains(t.Id));

            return true;
        }

        public bool IsChainValid(List<Block> chainToValidate = null)
        {
            var chain = chainToValidate ?? Chain;
            for (int i = 1; i < chain.Count; i++)
            {
                Block currentBlock = chain[i];
                Block previousBlock = chain[i - 1];

                if (currentBlock.MerkleRoot != HashingService.ComputeMerkleRoot(currentBlock.Transactions))
                    return false;

                if (currentBlock.Hash != HashingService.CalculateHash(currentBlock))
                    return false;

                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return false;
            }
            return true;
        }

        private void ClearMinedTransactions(List<Transaction> minedTransactions)
        {
            if (minedTransactions == null) return;
            var txIds = minedTransactions.Select(t => t.Id).ToHashSet();
            PendingTransactions.RemoveAll(t => txIds.Contains(t.Id));
        }
    }
}