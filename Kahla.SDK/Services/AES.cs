using Aiursoft.Pylon.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kahla.SDK.Services
{
    public class AES : ITransientDependency
    {
        public string OpenSSLDecrypt(string encrypted, string passphrase)
        {
            byte[] encryptedBytesWithSalt = Convert.FromBase64String(encrypted);
            byte[] salt = new byte[8];
            byte[] encryptedBytes = new byte[encryptedBytesWithSalt.Length - salt.Length - 8];
            Buffer.BlockCopy(encryptedBytesWithSalt, 8, salt, 0, salt.Length);
            Buffer.BlockCopy(encryptedBytesWithSalt, salt.Length + 8, encryptedBytes, 0, encryptedBytes.Length);
            // get key and iv
            DeriveKeyAndIV(passphrase, salt, out byte[] key, out byte[] iv);
            return DecryptStringFromBytesAes(encryptedBytes, key, iv);
        }

        private void DeriveKeyAndIV(string passphrase, byte[] salt, out byte[] key, out byte[] iv)
        {
            var concatenatedHashes = new List<byte>(48);
            var password = Encoding.UTF8.GetBytes(passphrase);
            var currentHash = new byte[0];
            var md5 = MD5.Create();
            bool enoughBytesForKey = false;
            while (!enoughBytesForKey)
            {
                int preHashLength = currentHash.Length + password.Length + salt.Length;
                byte[] preHash = new byte[preHashLength];
                Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                Buffer.BlockCopy(password, 0, preHash, currentHash.Length, password.Length);
                Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + password.Length, salt.Length);
                currentHash = md5.ComputeHash(preHash);
                concatenatedHashes.AddRange(currentHash);
                if (concatenatedHashes.Count >= 48)
                    enoughBytesForKey = true;
            }
            key = new byte[32];
            iv = new byte[16];
            concatenatedHashes.CopyTo(0, key, 0, 32);
            concatenatedHashes.CopyTo(32, iv, 0, 16);
            md5.Clear();
        }

        private string DecryptStringFromBytesAes(byte[] cipherText, byte[] key, byte[] iv)
        {
            RijndaelManaged aesAlg = null;
            string plaintext;
            try
            {
                aesAlg = new RijndaelManaged { Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Key = key, IV = iv };
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using MemoryStream msDecrypt = new MemoryStream(cipherText);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);
                plaintext = srDecrypt.ReadToEnd();
                srDecrypt.Close();
                return plaintext;
            }
            finally
            {
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                }
            }
        }
    }
}
