using System;
using BlockChain.Models;

namespace BlockChain.Services
{
    public static class TransactionService
    {
        public static bool Validate(Transaction tx)
        {
            if (string.IsNullOrEmpty(tx.Sender) || tx.Sender == "SYSTEM")
            {
                return true;
            }

            if (string.IsNullOrEmpty(tx.SenderPublicKey) || tx.Signature == null)
            {
                Console.WriteLine($"[ПОМИЛКА] Транзакція [{tx.Id[..8]}...] не підписана!");
                return false;
            }

            bool isValid = Wallet.VerifySignature(tx.SenderPublicKey, tx.Id, tx.Signature);
            if (!isValid)
            {
                Console.WriteLine($"[ПОМИЛКА] Цифровий підпис транзакції [{tx.Id[..8]}...] недійсний!");
            }

            return isValid;
        }
    }
}