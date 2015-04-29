using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.Helpers.Cryptography
{
    //в этом классе храняться ключи
    internal class PrivateKeyManager
    {
        //этот метод вызывать только так через checkkey которые доступен AccessManager.checkKey
        internal const string AssemblySigningPublicKey =
            "<RSAKeyValue><Modulus>uTNbl8WJFeO3lx7LEM6kulfGPXLe1AztYo3Bzind8IZlhKTr0KfrJRgEme1DnXl2/oZCkNfpqgxhT3rmNO8rzw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        internal static string GetMegatec3DESKey()
        {
            return GetKeyInternal(new[]
                {
                    "D052", "684E-3", "E", "E1-", "4CAC-9",
                    "CD1-B", "2FD", "F206", "2EC6"
                });
        }

        internal static string GetKeyInternal(string[] strArr)
        {
            var sb = new StringBuilder();
            foreach (string str in strArr)
            {
                sb.Append(str);
            }

            return sb.ToString();
        }


        internal static string GetUrlEncodingKey()
        {
            return GetKeyInternal(new[]
                {
                    "G052", "6a4E-3", "G",
                    "Eg8", "4CAC-9", "CD1-B", "2DD", "F2a6", "2dEC6"
                });
        }

        internal static string GetAssemblyDataEncKey()
        {
            return "0sdfd4dd0-9dsf3c-4dsf4f-a891-f67fsdf580d6a";
        }

        internal static string GetAssemblyGuidHashingSalt()
        {
            return
                GetKeyInternal(new[] { "sadf32", "45Salt", "1241".Replace("4", "3"), "24ujj", "hsdfsd", "jfqwer", "t67744" });
        }
    }
}
