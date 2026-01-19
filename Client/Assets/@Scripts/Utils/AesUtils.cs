using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class AesUtils
{
    // 32-byte Key (AES-256) - Randomly mixed bytes to avoid direct string search on GitHub
    private static readonly byte[] KeyBytes = { 
        0x1A, 0xC4, 0x72, 0xFD, 0x88, 0x12, 0x4B, 0x33, 
        0x90, 0xEE, 0x21, 0x05, 0xBC, 0x3F, 0x8D, 0xA2, 
        0x77, 0x61, 0x43, 0x59, 0x11, 0x02, 0xB5, 0x48, 
        0x2E, 0x19, 0x67, 0x93, 0xAA, 0xBB, 0xCC, 0xDD 
    };
    
    // 16-byte IV
    private static readonly byte[] IVBytes = { 
        0x45, 0x66, 0x12, 0x99, 0x01, 0x22, 0x33, 0xEE, 
        0x10, 0x88, 0x77, 0x66, 0x55, 0x44, 0x33, 0x22 
    };

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = KeyBytes;
                aesAlg.IV = IVBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Encryption Error] {e.Message}");
            return plainText;
        }
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = KeyBytes;
                aesAlg.IV = IVBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Decryption Failed] Key mismatch or tampered data: {e.Message}");
            return null;
        }
    }
}
