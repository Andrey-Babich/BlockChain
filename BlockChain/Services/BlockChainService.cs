using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockChain.Models;
using BlockChain.Services;

namespace BlockChain.Services
{
    // Сервіс для управління блокчейном
    public class BlockChainService
    {
        public List<Block> Chain {  get; set; } // Список блоків у ланцюзі
        private readonly HashingService _hashingService; // Сервіс для обчислення хешів

        public BlockChainService()
        {
            _hashingService = new HashingService();
            Chain = new List<Block>();
            CreateGenesisBlock(); // Створення генезис-блоку при ініціалізаціі
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, DateTime.UtcNow, "Genesis Block", "0"); // Створення генезис-блоку з індексом 0,
                                                                                    // поточним часом, даними "Genesis Block" та попереднім хешем "0"
            genesisBlock.Hash = _hashingService.ComputeHash(genesisBlock); // Обчислення хешу генезис-блоку
            Chain.Add(genesisBlock);
        }

        // Метод для додавання нового блоку до ланцюга
        public void AddBlock(string data)
        {
            var prevBlock = Chain.Last(); // Отримання останього блоку у ланцюзі
            var newIndex = prevBlock.Index + 1; // Обчислення індексу нового блоку
            var newTimeStamp = DateTime.UtcNow; // Поточний час для нового блоку
            var newPrevHash = prevBlock.Hash; // Хеш попереднього блоку
            var newBlock = new Block(newIndex, newTimeStamp, data, newPrevHash); // Створення нового блоку
            newBlock.Hash = _hashingService.ComputeHash(newBlock); // Обчислення хешу нового блоку
            Chain.Add(newBlock); // Додавання нвого блоку до ланцюга
        }

        // Метод для переввірки валідності ланцюга блоків
        public bool isValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var prevBlock = Chain[i - 1];
                // Перевірка, чи хеш поточногу блоку відповідає обчисленому хешу
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                {
                    return false;
                }
                // Перевірка, чи попередній хеш поточного блоку відповідає хешу попереднього блоку
                if (currentBlock.PrevHash != prevBlock.Hash)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
