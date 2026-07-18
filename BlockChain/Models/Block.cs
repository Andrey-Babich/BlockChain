using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain.Models
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Data { get; set; }
        public string Hash { get; set; }
        public string PrevHash { get; set; }
        public Block(int index, DateTime timeStamp, string data, string prevHash)
        {
            Index = index;
            TimeStamp = timeStamp;
            Data = data;
            PrevHash = prevHash;
        }
    }
}
