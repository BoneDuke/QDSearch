using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с датами нерасчитанных туров
    /// </summary>
    public static class TurDatesExtension
    {
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "TurDate";

        /// <summary>
        /// Возвращает список дат по турам
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="tourKeys">Ключ туров (таблица tbl_TurList)</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<Tuple<int, DateTime>> GetDatesByTours(this MtMainDbDataContext dc, IList<int> tourKeys, out string hash)
        {
            List<Tuple<int, DateTime>> dates;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", tourKeys));
            if ((dates = CacheHelper.GetCacheItem<List<Tuple<int, DateTime>>>(hash)) != null) return dates;

            dates = new List<Tuple<int, DateTime>>();

            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select td_date, td_trkey ");
            commandBuilder.AppendLine("from TurDate ");
            commandBuilder.AppendLine(String.Format("where TD_TRKey in ({0})", string.Join(",", tourKeys)));
            commandBuilder.AppendLine(" and td_date >= dateadd(day, -1, getdate())");

            using (var command = dc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                dc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dates.Add(new Tuple<int, DateTime>(reader.GetInt32("td_trkey"), reader.GetDateTime("td_date")));
                    }
                }
                dc.Connection.Close();
            }

            CacheHelper.AddCacheData(hash, dates, null, Globals.Settings.Cache.MediumCacheTimeout);
            return dates;
        }
    }
}
