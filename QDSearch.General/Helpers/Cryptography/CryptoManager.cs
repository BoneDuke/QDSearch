using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.Helpers.Cryptography
{
    public static class CryptoManager
    {
        private static readonly Dictionary<string, string> InvalidUrlChar;

        static CryptoManager()
        {
            InvalidUrlChar = new Dictionary<string, string>();
            InvalidUrlChar[@"&"] = "laaaal";
            InvalidUrlChar[@"+"] = "lbbbbl";
            InvalidUrlChar[@"#"] = "lccccl";
            InvalidUrlChar[@"/"] = "lddddl";
            InvalidUrlChar[@"\"] = "leeeel";
            InvalidUrlChar[@"*"] = "lffffl";
            InvalidUrlChar[@":"] = "lggggl";
        }

        /// <summary>
        ///     зашифровать строку через алгоритм TripleDES
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string EncodeTripleDesString(string source)
        {
            return TripleDesEncryption.EncryptString(source, TripleDesEncryption.KeySize192, PrivateKeyManager.GetMegatec3DESKey());
        }

        /// <summary>
        ///     расшифровать строку через TripleDES
        /// </summary>
        /// <param name="encodedSource"></param>
        /// <returns></returns>
        public static string DecodeTripleDesString(string encodedSource)
        {
            return TripleDesEncryption.DecryptString(encodedSource, TripleDesEncryption.KeySize192, PrivateKeyManager.GetMegatec3DESKey());
        }


        /// <summary>
        ///     Шифрует строку для Url
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string EncryptStringUrl(string text)
        {
            string res = TripleDesEncryption.EncryptString(text, TripleDesEncryption.KeySize128, PrivateKeyManager.GetUrlEncodingKey());
            foreach (var kvpchar in InvalidUrlChar)
                res = res.Replace(kvpchar.Key, kvpchar.Value);
            return res;
        }

        /// <summary>
        ///     Расшифровать строку для Url
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string DecryptStringUrl(string text)
        {
            if (text == null)
                return null;

            foreach (var kvpchar in InvalidUrlChar)
                text = text.Replace(kvpchar.Value, kvpchar.Key);

            string res = TripleDesEncryption.DecryptString(text, TripleDesEncryption.KeySize128, PrivateKeyManager.GetUrlEncodingKey());
            return res;
        }
    }
}
