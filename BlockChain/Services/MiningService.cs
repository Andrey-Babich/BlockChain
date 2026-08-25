using BlockChain.Models;

namespace BlockChain.Services
{
    public static class MiningService
    {
        public static void MineBlock(Block block, int difficulty)
        {
            string target = new string('0', difficulty);
            while (block.Hash == null || !block.Hash.StartsWith(target))
            {
                block.Nonce++;
                block.Hash = HashingService.CalculateHash(block);
            }
        }
    }
}