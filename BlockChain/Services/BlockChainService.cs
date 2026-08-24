using BlockChain.Models;
using BlockChain.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockChain.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }

        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;

        public BlockChainService()
        {
            _hashingService = new HashingService();
            _miningService = new MiningService(_hashingService);

            Chain = new List<Block>();

            CreateGenesisBlock();
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(
                0,
                DateTime.UtcNow,
                "Genesis Block",
                "0"
            );

            _miningService.MineBlock(genesisBlock, "cafe");

            Chain.Add(genesisBlock);
        }

        public void AddBlock(string data)
        {
            var prevBlock = Chain.Last();

            var newIndex = prevBlock.Index + 1;
            var newTimeStamp = DateTime.UtcNow;
            var newPrevHash = prevBlock.Hash;

            var newBlock = new Block(
                newIndex,
                newTimeStamp,
                data,
                newPrevHash
            );

            _miningService.MineBlock(newBlock, "cafe");

            Chain.Add(newBlock);
        }

        public bool isValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var prevBlock = Chain[i - 1];

                if (currentBlock.Hash !=
                    _hashingService.ComputeHash(currentBlock))
                {
                    return false;
                }

                if (currentBlock.PrevHash != prevBlock.Hash)
                {
                    return false;
                }

                if (!currentBlock.Hash.StartsWith("cafe"))
                {
                    return false;
                }
            }

            return true;
        }
    }
}