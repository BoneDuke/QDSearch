using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Helpers.Cryptography;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    public static class DupUsersExtension
    {
        private static readonly object LockUsers = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "DUP_USER";

        private static string GetPasswordHash(string password)
        {
            var myHash = new MD5CryptoServiceProvider();
            myHash.ComputeHash(Encoding.ASCII.GetBytes(password));
            return Convert.ToBase64String(myHash.Hash);
        }

        /// <summary>
        /// Возвращает онлайн-пользователя по логину / паролю
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="name">Логин пользователя</param>
        /// <param name="decryptedPassword">Расшифрованный пароль (его обычно вводит пользователь)</param>
        /// <returns>Возвращает пользователя или null, если не нашли комбинации логин / пароль</returns>
        public static DUP_USER GetDupUser(this MtMainDbDataContext dc, string name, string decryptedPassword)
        {
            // для пользователей с зашифрованным паролем
            var user = dc.GetAllDupUsers().SingleOrDefault(u => u.US_ID == name
                    && (u.US_Attribute & (int) DupUserAttributes.Converted) == (int) DupUserAttributes.Converted
                    && CryptoManager.DecodeTripleDesString(u.US_PASSWORD) == decryptedPassword);

            if (user != null)
                return user;
            
            // для пользователей с закодированным паролем
            user = dc.GetAllDupUsers().SingleOrDefault(u => u.US_ID == name
                && (u.US_Attribute & (int)DupUserAttributes.Converted) != (int)DupUserAttributes.Converted
                && u.US_PASSWORD == GetPasswordHash(decryptedPassword));
            return user;
        }

        /// <summary>
        /// Возвращает расшифрованный пароль пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns></returns>
        /// <exception cref="CryptographicException"></exception>
        public static string GetDecryptedPassword(this DUP_USER user)
        {
            string result;
            if ((user.US_Attribute & (int) DupUserAttributes.Converted) == (int) DupUserAttributes.Converted)
                result = String.IsNullOrEmpty(user.US_PASSWORD) ? String.Empty : CryptoManager.DecodeTripleDesString(user.US_PASSWORD);
            else
                throw new CryptographicException("Невозможно расшифровать пароль пользователя, так как он не зашифрован");

            return result;
        }

        /// <summary>
        /// Возвращает список всех онлайн-пользователей
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<DUP_USER> GetAllDupUsers(this MtMainDbDataContext dc)
        {
            List<DUP_USER> users;
            if ((users = CacheHelper.GetCacheItem<List<DUP_USER>>(TableName)) != null) return users;
            lock (LockUsers)
            {
                if ((users = CacheHelper.GetCacheItem<List<DUP_USER>>(TableName)) != null) return users;

                users = (from u in dc.DUP_USERs
                           select u)
                    .ToList<DUP_USER>();

                CacheHelper.AddCacheData(TableName, users, TableName);
            }
            return users;
        }


    }
}
