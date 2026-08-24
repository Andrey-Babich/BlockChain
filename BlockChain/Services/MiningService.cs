using BlockChain.Models;
using System.Diagnostics;

namespace BlockChain.Services
{
    public class MiningService
    {
        private readonly HashingService _hashingService;

        public MiningService(HashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public long MineBlock(Block block, string targetPrefix)
        {
            long attempts = 0;
            long foundNonce = -1;

            Stopwatch stopwatch = Stopwatch.StartNew();

            Parallel.For(
                0L,
                long.MaxValue,
                (nonce, state) =>
                {
                    if (state.IsStopped)
                        return;

                    Block testBlock = new Block(
                        block.Index,
                        block.TimeStamp,
                        block.Data,
                        block.PrevHash
                    );

                    testBlock.Nonce = nonce;
                    testBlock.Hash = _hashingService.ComputeHash(testBlock);

                    long currentAttempts =
                        Interlocked.Increment(ref attempts);

                    if (currentAttempts % 50000 == 0)
                    {
                        double seconds = stopwatch.Elapsed.TotalSeconds;

                        if (seconds > 0)
                        {
                            double hashrate = currentAttempts / seconds;

                            Console.Write(
                                $"\rШвидкість: {hashrate:F2} H/s"
                            );
                        }
                    }

                    if (testBlock.Hash.StartsWith(targetPrefix))
                    {
                        if (Interlocked.CompareExchange(
                            ref foundNonce,
                            nonce,
                            -1) == -1)
                        {
                            block.Nonce = nonce;
                            block.Hash = testBlock.Hash;

                            state.Stop();
                        }
                    }
                });

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Знайдено!");
            Console.WriteLine($"Nonce: {block.Nonce}");
            Console.WriteLine($"Hash: {block.Hash}");
            Console.WriteLine($"Спроб: {attempts}");

            return block.Nonce;
        }
    }
}