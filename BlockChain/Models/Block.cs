using System;
using System.Collections.Generic;

namespace BlockChain.Models
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public int Nonce { get; set; }

        public Block(int index, DateTime timestamp, List<Transaction> transactions, string previousHash = "")
        {
            Index = index;
            Timestamp = timestamp;
            Transactions = transactions;
            PreviousHash = previousHash;
            Hash = "";
            Nonce = 0;
        }
    }
}