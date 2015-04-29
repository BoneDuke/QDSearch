// <copyright file="EncryptionHelper.cs" company="GinkoSolutions.com">
// Copyright (c) 2011 All Right Reserved
// </copyright>
// <author>Sean Greasley</author>
// <email>sean@ginkosolutions.com</email>
// <summary>Enables simple TripleDES encryption with or without hashing</summary>

using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace QDSearch.Helpers.Cryptography
{
    /// <summary>
    /// Enables simple TripleDES encryption with or without hashing
    /// </summary>
    public class EncryptionHelper
    {
        /// <summary>
        /// Encryption key. Used to encrypt and decrypt.
        /// </summary>
        private const string EncryptionKey = "SFTSECRETKEY";

        /// <summary>
        /// Encrypt text string
        /// </summary>
        /// <param name="toEncrypt">The string of data to encrypt</param>
        /// <param name="useHashing">Weather hashing is used or not</param>
        /// <returns>An encrypted string</returns>
        public static string Encrypt(string toEncrypt, bool useHashing)
        {
            var hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, toEncrypt, useHashing);
            if (CacheHelper.IsCacheKeyExists(hash))
                return CacheHelper.GetCacheItem<string>(hash);

            byte[] keyArray;
            var toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

            // If hashing use get hashcode regards to your key
            if (useHashing)
            {
                var hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
                hashmd5.Clear();
            }
            else
                keyArray = Encoding.UTF8.GetBytes(EncryptionKey);

            // Set the secret key for the tripleDES algorithm
            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            // Transform the specified region of bytes array to resultArray
            var cTransform = tdes.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();

            var result = Convert.ToBase64String(resultArray, 0, resultArray.Length);

            CacheHelper.AddCacheData(hash, result, null, Globals.Settings.Cache.LongCacheTimeout);
            // Return the encrypted data into unreadable string format
            return result;
        }

        /// <summary>
        /// Decrypts a text string
        /// </summary>
        /// <param name="cipherString">The encrypted string</param>
        /// <param name="useHashing">Weather hashing is used or not</param>
        /// <returns>Decrypted text string</returns>
        public static string Decrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString.Replace(' ', '+'));

            if (useHashing)
            {
                // If hashing was used get the hash code with regards to your key
                var hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
                hashmd5.Clear();
            }
            else
            {
                // If hashing was not implemented get the byte code of the key
                keyArray = Encoding.UTF8.GetBytes(EncryptionKey);
            }

            // Set the secret key for the tripleDES algorithm
            var tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            var cTransform = tdes.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();

            // Return the Clear decrypted TEXT
            return Encoding.UTF8.GetString(resultArray);
        }
    }
}