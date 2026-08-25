using System;
using System.Collections.Generic;
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
            Block genesisBlock = new Block(0, DateTime.Now, new List<Transaction>(), "0");
            genesisBlock.Hash = HashingService.CalculateHash(genesisBlock);
            return genesisBlock;
        }

        public Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        public void AddTransaction(Transaction transaction)
        {
            PendingTransactions.Add(transaction);
        }

        public void MinePendingTransactions(string minerAddress)
        {
            Block block = new Block(Chain.Count, DateTime.Now, PendingTransactions, GetLatestBlock().Hash);
            MiningService.MineBlock(block, Difficulty);
            Chain.Add(block);
            PendingTransactions = new List<Transaction>();
        }

        public bool IsChainValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];

                if (currentBlock.Hash != HashingService.CalculateHash(currentBlock))
                {
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }
            return true;
        }
    }
}