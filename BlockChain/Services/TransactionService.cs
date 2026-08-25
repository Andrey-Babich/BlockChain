using System;
using System.Security.Cryptography;
using System.Text;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class TransactionService
    {
        public static bool VerifyTransaction(Transaction transaction)
        {
            if (transaction == null) return false;

            if (string.IsNullOrEmpty(transaction.Signature) || string.IsNullOrEmpty(transaction.SenderPublicKey))
            {
                return true;
            }

            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes($"{transaction.From}{transaction.To}{transaction.Amount}");
                byte[] signatureBytes = Convert.FromBase64String(transaction.Signature);

                using (var rsa = RSA.Create())
                {
                    rsa.FromXmlString(transaction.SenderPublicKey);
                    return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}