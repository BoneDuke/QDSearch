using System.Reflection;
using System.Text;
using System.Web.Caching;
using System.Web.UI.WebControls;
using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Класс по работе с расписаниями
    /// </summary>
    public static class CostsExtension
    {
        private static readonly object LockCosts = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "tbl_Costs";

        /// <summary>
        /// Возвращает список ключей пакетов для перелетов по направлению
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<int> GetFlightPacketKeysByDirection(this MtSearchDbDataContext dc, int cityKeyFrom, int cityKeyTo, out string hash)
        {
            List<int> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, cityKeyTo);
            if ((result = CacheHelper.GetCacheItem<List<int>>(hash)) != default(List<int>)) return result;

            result = new List<int>();

            var cacheDependencies = new List<string>();
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct CS_PKKey");
            commandBuilder.AppendLine("from tbl_Costs ");
            commandBuilder.AppendLine("join Charter on CH_Key = CS_Code ");
            commandBuilder.AppendLine(String.Format("where CS_SVKey = {0}", (int)ServiceClass.Flight));
            commandBuilder.AppendLine("and (CS_DateEnd is null or CS_DATEEND >= dateadd(d, -1, getdate())) ");
            commandBuilder.AppendLine(String.Format("and CH_CITYKEYFROM = {0} and CH_CITYKEYTO = {1}", cityKeyFrom, cityKeyTo));

            using (var command = dc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                dc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt32("CS_PKKey"));
                    }
                }
                dc.Connection.Close();
            }

            cacheDependencies.Add(TableName);
            cacheDependencies.Add(CharterExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        public static List<Repository.MtSearch.tbl_Cost> GetFlightCostsByPacketKey(this MtSearchDbDataContext dc, int packetKey, out string hash)
        {
            List<Repository.MtSearch.tbl_Cost> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, packetKey);
            if ((result = CacheHelper.GetCacheItem<List<Repository.MtSearch.tbl_Cost>>(hash)) != default(List<Repository.MtSearch.tbl_Cost>)) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.CostHash, (int) ServiceClass.Flight, packetKey)
            };

            result = dc.tbl_Costs
                .Where(c => c.CS_SVKEY == (int)ServiceClass.Flight 
                    && c.CS_SUBCODE1 != null 
                    && c.CS_PKKEY.HasValue 
                    && c.CS_PKKEY == packetKey
                    && (!c.CS_DATEEND.HasValue || DateTime.Now.Date <= c.CS_DATEEND.Value)
                    // todo: уточнить необходимость условия 14.08.2014
                    //&& (!c.CS_CHECKINDATEEND.HasValue || (DateTime.Now.Date >= c.CS_CHECKINDATEBEG.Value && DateTime.Now.Date <= c.CS_CHECKINDATEEND.Value))
                    )
                .ToList();

            CacheHelper.AddCacheData(hash, result, cacheDependencies, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        ///// <summary>
        ///// Возвращает список цен
        ///// </summary>
        ///// <param name="dc">Контекст базы данных</param>
        ///// <returns></returns>
        //public static List<Repository.MtSearch.tbl_Cost> GetAllFlightCosts(this MtSearchDbDataContext dc)
        //{
        //    List<Repository.MtSearch.tbl_Cost> costs;
        //    if ((costs = CacheHelper.GetCacheItem<List<Repository.MtSearch.tbl_Cost>>(TableName)) != default(List<Repository.MtSearch.tbl_Cost>)) return costs;

        //    lock (LockCosts)
        //    {
        //        if ((costs = CacheHelper.GetCacheItem<List<Repository.MtSearch.tbl_Cost>>(TableName)) != default(List<Repository.MtSearch.tbl_Cost>)) return costs;

        //        costs = (from c in dc.tbl_Costs
        //                 where (!c.CS_CHECKINDATEEND.HasValue || c.CS_CHECKINDATEEND.Value >= DateTime.Now.Date)
        //                 && (!c.CS_DATEEND.HasValue || c.CS_DATEEND.Value >= DateTime.Now.Date)
        //                 && (c.CS_SVKEY == (int)ServiceClass.Flight)
        //                      select c)
        //            .ToList<Repository.MtSearch.tbl_Cost>();

        //        CacheHelper.AddCacheData(TableName, costs, TableName);
        //    }

        //    return costs;
        //}

        /// <summary>
        /// Расчет цены по услуге через хранимку Мегатек
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="svKey">Класс услуги</param>
        /// <param name="code">Code услуги</param>
        /// <param name="subCode1">SubCode1 услуги</param>
        /// <param name="subCode2">SubCode2 услуги</param>
        /// <param name="partnerKey">Ключ партнера</param>
        /// <param name="packetKey">Ключ пакета</param>
        /// <param name="startDate">Дата начала услуги</param>
        /// <param name="days">Продолжительность услуги</param>
        /// <param name="rate">Валюта, в которой должен быть произведен расчет</param>
        /// <param name="nmen">Число людей по услуге</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static decimal? GetServiceCost(this MtMainDbDataContext dc, int svKey, int code, int subCode1, int subCode2, int partnerKey, 
            int packetKey, DateTime startDate, int days, string rate, int nmen, out string hash)
        {
            decimal? result;
            hash = String.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}", MethodBase.GetCurrentMethod().Name, svKey, code, subCode1, subCode2, partnerKey, packetKey, startDate, days, rate, nmen);
            if ((result = CacheHelper.GetCacheItem<decimal?>(hash)) != default(decimal?)) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.CostHash, svKey, code)
            };
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendFormat(@"declare @nNetto decimal(14,2), @nBrutto decimal(14,2), @nDiscount decimal(14,2)
                        declare @nettoDetail varchar(100), @sBadRate varchar(2), @dtBadDate DateTime
		                declare @sDetailed varchar(100),  @nSPId int, @useDiscountDays int
		                declare @tourKey int, @turdate datetime, @tourDays int, @includeAddCost bit
						declare @saleDate DateTime
						set @saleDate = GetDate()
						set @includeAddCost = 1

                        exec GetServiceCost {0}, {1}, {2}, {3}, {4}, {5}, '{6:yyyy-MM-dd}', {7},
                        '{8}', {9}, 0, 0, 0,
                        @saleDate, @nNetto output, @nBrutto output, @nDiscount output,
						@nettoDetail output, @sBadRate output, @dtBadDate output,
						@sDetailed output, @nSPId output, 0, @tourKey, @turdate, @tourDays, @includeAddCost

                        select @nBrutto",
            svKey, code, subCode1, subCode2, partnerKey, packetKey, startDate, days, rate, nmen);

            using (var command = dc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                dc.Connection.Open();
                var bruttoTemp = command.ExecuteScalar();
                if (bruttoTemp.ToString() != String.Empty)
                    result = (decimal) bruttoTemp;

                dc.Connection.Close();
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список всех HotelRooms, по которым заведены цены с заданными параметрами
        /// </summary>
        /// <param name="mainDc">Контекст БД</param>
        /// <param name="serviceClass">Класс услуги</param>
        /// <param name="code">Code услуги</param>
        /// <param name="date">Дата услуги</param>
        /// <param name="packetKey">Ключ пакета</param>
        /// <param name="partnerKey">Ключ партнера</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static List<int> GetHotelRoomsFromCosts(this MtMainDbDataContext mainDc, ServiceClass serviceClass, int code, DateTime date, int packetKey, int partnerKey, out string hash)
        {
            List<int> result;
            hash = String.Format("{0}_{1}_{2}_{3}_{4}", MethodBase.GetCurrentMethod().Name, (int)serviceClass, code, date, packetKey);
            if ((result = CacheHelper.GetCacheItem<List<int>>(hash)) != null) return result;

            result = (from c in mainDc.tbl_Costs
                where c.CS_SVKEY == (int) serviceClass
                      && c.CS_CODE == code
                      && c.CS_PKKEY == packetKey
                      && c.CS_PRKEY == partnerKey
                      && c.CS_SUBCODE1.HasValue
                      && (date >= c.CS_DATE.Value && date <= c.CS_DATEEND.Value || !c.CS_DATE.HasValue || !c.CS_DATEEND.HasValue)
                      && (date >= c.CS_CHECKINDATEBEG.Value && date <= c.CS_CHECKINDATEEND.Value || !c.CS_CHECKINDATEBEG.HasValue || !c.CS_CHECKINDATEEND.HasValue)
                      && (DateTime.Now >= c.cs_DateSellBeg.Value && DateTime.Now <= c.cs_DateSellEnd.Value || !c.cs_DateSellBeg.HasValue || !c.cs_DateSellEnd.HasValue)
                select c.CS_SUBCODE1.Value)
                .Distinct()
                .ToList();

            CacheHelper.AddCacheData(hash, result, null, Globals.Settings.Cache.MediumCacheTimeout);
            return result;
        }
    }
}
