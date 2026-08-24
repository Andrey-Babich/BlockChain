using BlockChain.Models;

namespace BlockChain.Services
{
    public class MiningService
    {
        public static void MineBlock(Block block, int difficulty)
        {
            string leadingZeros = new string('0', difficulty);

            while (string.IsNullOrEmpty(block.Hash) || !block.Hash.StartsWith(leadingZeros))
            {
                block.Nonce++;
                block.Hash = HashingService.CalculateHash(block);
            }
        }
    }
}