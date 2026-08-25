using System;
using BlockChain.Models;

namespace BlockChain.Services
{
    public static class TransactionService
    {
        public static bool Validate(Transaction tx)
        {
            if (tx.From == "BURN")
            {
                Console.WriteLine("[БЕЗПЕКА] Помилка: Спроба витратити монети з адреси BURN заблокована!");
                return false;
            }

            if (string.IsNullOrEmpty(tx.From) || tx.From == "SYSTEM")
            {
                return true;
            }

            if (string.IsNullOrEmpty(tx.SenderPublicKey))
            {
                Console.WriteLine("[ПОМИЛКА] Відсутній публічний ключ.");
                return false;
            }

            string derivedAddress = Wallet.DeriveAddress(tx.SenderPublicKey);
            if (derivedAddress != tx.From)
            {
                Console.WriteLine("Критична помилка: Публічний ключ не відповідає адресі відправника");
                return false;
            }

            bool isSignatureValid = Wallet.VerifySignature(tx.SenderPublicKey, tx.Id, tx.Signature);
            if (!isSignatureValid)
            {
                Console.WriteLine("[ПОМИЛКА] Цифровий підпис недійсний!");
                return false;
            }

            return true;
        }
    }
}