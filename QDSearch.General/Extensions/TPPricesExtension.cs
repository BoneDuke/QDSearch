using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с расчитанными ценами
    /// </summary>
    public static class TPPricesExtension
    {
        private static readonly object LockTPPrices = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "TP_Prices";

        /// <summary>
        /// Возвращает tp_price по ключу
        /// </summary>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="priceKey">Ключ цены</param>
        /// <returns></returns>
        public static TP_Price GetTPPriceByKey(this MtMainDbDataContext mainDc, int priceKey)
        {
            return mainDc.TP_Prices.SingleOrDefault(p => p.TP_Key == priceKey);
        }

        /// <summary>
        /// Возрвращает ключ 
        /// </summary>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="priceKey">Ключ цены</param>
        /// <param name="hotelKey">Ключ отеля</param>
        /// <param name="hash">Хэш</param>
        /// <returns></returns>
        public static int GetHotelPacketKey(this MtMainDbDataContext mainDc, int priceKey, int hotelKey, out string hash)
        {
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, priceKey);
            if (CacheHelper.IsCacheKeyExists(hash))
                return CacheHelper.GetCacheItem<int>(hash);

            var result = -1;
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select ts_oppacketkey ");
            commandBuilder.AppendLine("from tp_services ");
            commandBuilder.AppendLine(String.Format("where ts_key in (select tl_tskey from tp_servicelists where tl_tikey in (select tp_tikey from tp_prices where tp_key = {0})) ", priceKey));
            commandBuilder.AppendLine(String.Format("and ts_svkey = {0} and ts_code = {1} ", (int)ServiceClass.Hotel, hotelKey));

            using (var command = mainDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();
                command.CommandTimeout = 100;

                mainDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader.GetInt32("ts_oppacketkey");
                    }
                }
                mainDc.Connection.Close();
            }
            
            CacheHelper.AddCacheData(hash, result, null, Globals.Settings.Cache.MediumCacheTimeout);
            return result;
        }
    }
}
