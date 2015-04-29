using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.Helpers.Cryptography
{
    /// <summary>
    ///     класс для шифрования/дешифрования строк через TripleDES
    /// </summary>
    public class TripleDesEncryption
    {
        /// <summary>
        ///     возможный размер = 128 ключа TripleDES
        /// </summary>
        public const int KeySize128 = 128;

        /// <summary>
        ///     возможный размер = 192 ключа TripleDES
        /// </summary>
        public const int KeySize192 = 192;

        /// <summary>
        ///     шифрует строку
        /// </summary>
        /// <param name="inputString">строка для шифрования</param>
        /// <param name="dwKeySize">размер ключа</param>
        /// <param name="privateKey">ключевого секретное слово для генерации ключа</param>
        /// <returns></returns>
        public static string EncryptString(string inputString, int dwKeySize, string privateKey)
        {
            TripleDES alg = TripleDES.Create();
            var pdb = new PasswordDeriveBytes(privateKey, null); //класс, позволяющий генерировать ключи на базе паролей
            pdb.HashName = "SHA512"; //будем использовать SHA512            
            alg.KeySize = dwKeySize; //устанавливаем размер ключа
            alg.Key = pdb.GetBytes(dwKeySize >> 3); //получаем ключ из пароля
            alg.Mode = CipherMode.CBC; //используем режим CBC
            alg.IV = new Byte[alg.BlockSize >> 3]; //и пустой инициализационный вектор
            ICryptoTransform tr = alg.CreateEncryptor(); //создаем encryptor
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            var instream = new MemoryStream(bytes);
            var stringBuilder = new StringBuilder();
            int buflen = ((2 << 16) / alg.BlockSize) * alg.BlockSize;
            var inbuf = new byte[buflen];
            var outbuf = new byte[buflen];
            int len;

            while ((len = instream.Read(inbuf, 0, buflen)) == buflen)
            {
                int enclen = tr.TransformBlock(inbuf, 0, buflen, outbuf, 0); //собственно шифруем

                Array.Reverse(outbuf);
                // Why convert to base 64?
                // Because it is the largest power-of-two base printable using only ASCII characters
                stringBuilder.Append(Convert.ToBase64String(outbuf));
            }
            instream.Close();
            outbuf = tr.TransformFinalBlock(inbuf, 0, len); //шифруем финальный блок


            //Array.Reverse(outbuf);
            // Why convert to base 64?
            // Because it is the largest power-of-two base printable using only ASCII characters
            stringBuilder.Append(Convert.ToBase64String(outbuf));
            alg.Clear();
            return stringBuilder.ToString();
        }

        /// <summary>
        ///     дешифрует строку
        /// </summary>
        /// <param name="inputString">зашифрованная строка</param>
        /// <param name="dwKeySize">размер ключа</param>
        /// <param name="privateKey">ключевого секретное слово для генерации ключа</param>
        /// <returns></returns>
        public static string DecryptString(string inputString, int dwKeySize, string privateKey)
        {
            TripleDES alg = TripleDES.Create();
            var pdb = new PasswordDeriveBytes(privateKey, null);
            pdb.HashName = "SHA512";

            alg.KeySize = dwKeySize;
            dwKeySize >>= 3;
            alg.Key = pdb.GetBytes(dwKeySize);
            alg.Mode = CipherMode.CBC;
            alg.IV = new Byte[alg.BlockSize >> 3];
            ICryptoTransform tr = alg.CreateDecryptor();

            //!!!! minas для очень длинных строк надо получить bytes по кускам
            byte[] encryptedBytes = Convert.FromBase64String(inputString);

            var instream = new MemoryStream(encryptedBytes);
            //FileStream outstream = new FileStream(outFile.Text, FileMode.Create, FileAccess.Write, FileShare.None);
            int buflen = ((2 << 16) / alg.BlockSize) * alg.BlockSize;
            var inbuf = new byte[buflen];
            var outbuf = new byte[buflen];
            int len;
            var arrayList = new ArrayList();
            while ((len = instream.Read(inbuf, 0, buflen)) == buflen)
            {
                int declen = tr.TransformBlock(inbuf, 0, buflen, outbuf, 0);
                //Array.Reverse(outbuf);
                arrayList.AddRange(outbuf);
            }
            instream.Close();
            outbuf = tr.TransformFinalBlock(inbuf, 0, len);
            //Array.Reverse(outbuf);
            arrayList.AddRange(outbuf);
            alg.Clear();
            return Encoding.UTF8.GetString(arrayList.ToArray(Type.GetType("System.Byte")) as byte[]);
        }
    }
}
