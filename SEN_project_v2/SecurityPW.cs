using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace SEN_project_v2
{
    public class SecurityPW
    {
        public SecurityPW()
        {

        }
        //private const int keysize = 256;
        //string initVector = "tu89geji340t89u2";
        //public string Encrypt(string plainText, string key)
        //{
        //    byte[] initVectorBytes = Encoding.UTF8.GetBytes("tu89geji340t89u2");
        //    byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        //    PasswordDeriveBytes password = new PasswordDeriveBytes(key, null);
        //    byte[] keyBytes = password.GetBytes(256 / 8);
        //    RijndaelManaged symmetricKey = new RijndaelManaged();
        //    symmetricKey.Mode = CipherMode.CBC;
        //    ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
        //    MemoryStream memoryStream = new MemoryStream();
        //    CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        //    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        //    cryptoStream.FlushFinalBlock();
        //    byte[] cipherTextBytes = memoryStream.ToArray();
        //    memoryStream.Close();
        //    cryptoStream.Close();
        //    return Convert.ToBase64String(cipherTextBytes);
        //}

        //public string Decrypt(string cipherText, string passPhrase)
        //{
        //    byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89geji340t89u2");
        //    byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
        //    PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
        //    byte[] keyBytes = password.GetBytes(256 / 8);
        //    RijndaelManaged symmetricKey = new RijndaelManaged();
        //    symmetricKey.Mode = CipherMode.CBC;
        //    ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
        //    MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
        //    CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        //    byte[] plainTextBytes = new byte[cipherTextBytes.Length];
        //    int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
        //    memoryStream.Close();
        //    cryptoStream.Close();
        //    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        //}

        public string Encryptstring(string txtPassword)
        {
            byte[] passBytes = System.Text.Encoding.Unicode.GetBytes(txtPassword);
            string encryptPassword = Convert.ToBase64String(passBytes);
            return encryptPassword;
        }

        public string Decryptstring(string encryptedPassword)
        {
            byte[] passByteData = Convert.FromBase64String(encryptedPassword);
            string originalPassword = System.Text.Encoding.Unicode.GetString(passByteData);
            return originalPassword;
        }

    }
}
