using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using BlockChain.Models;

namespace BlockChain.Services
{
    public static class HashingService
    {
        public static string CalculateHash(Block block)
        {
            string rawData = $"{block.Index}-{block.Timestamp}-{block.MerkleRoot}-{block.PreviousHash}-{block.Nonce}";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string ComputeMerkleRoot(List<Transaction> transactions)
        {
            if (transactions == null || transactions.Count == 0)
                return "0000000000000000000000000000000000000000000000000000000000000000";

            List<string> hashes = transactions.Select(t => CalculateTxHash(t)).ToList();

            while (hashes.Count > 1)
            {
                if (hashes.Count % 2 != 0)
                {
                    hashes.Add(hashes[hashes.Count - 1]);
                }

                List<string> newHashes = new List<string>();
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    string combined = hashes[i] + hashes[i + 1];
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                        StringBuilder builder = new StringBuilder();
                        foreach (byte b in bytes) builder.Append(b.ToString("x2"));
                        newHashes.Add(builder.ToString());
                    }
                }
                hashes = newHashes;
            }

            return hashes[0];
        }

        private static string CalculateTxHash(Transaction tx)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{tx.Id}{tx.From}{tx.To}{tx.Amount}"));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}