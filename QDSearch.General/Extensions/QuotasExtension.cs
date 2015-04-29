using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Класс для проверки квот
    /// </summary>
    public static class QuotasExtension
    {
        private static readonly object LockQuotas = new object();
        /// <summary>
        /// Название соответствующей таблицы в БД
        /// </summary>
        public const string TableName = "Quotas";

        /// <summary>
        /// Статусы квот в порядке приоретизации для алгоритма проверки
        /// </summary>
        public static Dictionary<QuotesStates, int> OrderderQuotaStates = new Dictionary<QuotesStates, int>
            {
                {QuotesStates.None, 0},
                {QuotesStates.No, 1},
                {QuotesStates.Request, 2},
                {QuotesStates.Small, 3},
                {QuotesStates.Availiable, 4}
            };

        /// <summary>
        /// Возвращает список записей из таблицы Quotas
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<Quota> GetAllQuotas(this MtSearchDbDataContext dc)
        {
            List<Quota> quotas;
            if ((quotas = CacheHelper.GetCacheItem<List<Quota>>(TableName)) != null) return quotas;
            lock (LockQuotas)
            {
                if ((quotas = CacheHelper.GetCacheItem<List<Quota>>(TableName)) != null) return quotas;

                quotas = (from q in dc.Quotas
                    select q)
                    .ToList<Quota>();

                CacheHelper.AddCacheData(TableName, quotas, null, Globals.Settings.Cache.MediumCacheTimeout);
            }
            return quotas;
        }

        /// <summary>
        /// ПОлучения плоского списка квот из БД по параметрам. Загружается сразу на все даты для уменьшения числа обращений к БД
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="serviceClass">Класс услуги</param>
        /// <param name="code">Code услуги (для перелета - ключ чартера, для отеля - ключ отеля)</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static List<QuotaPlain> GetPlainQuotasObjects(this MtSearchDbDataContext dc, ServiceClass serviceClass, Int32 code, out string hash)
        {
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, (int)serviceClass, code);

            List<QuotaPlain> result;
            if ((result = CacheHelper.GetCacheItem<List<QuotaPlain>>(hash)) != null) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.QuotaHash, (int) serviceClass, code)
            };
            result = new List<QuotaPlain>();

            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select SS_QDID, QD_Type, SS_AllotmentAndCommitment, QP_Busy, QP_CheckInPlacesBusy, QP_Places, QP_CheckInPlaces, QP_Durations, QO_ID, SS_ID, QD_ID, ISNULL(QT_PRKey, 0) as QT_PRKey, ISNULL(QP_AgentKey, 0) as QP_AgentKey, ISNULL(QO_SubCode1, 0) as QO_SubCode1, ISNULL(QO_SubCode2, 0) as QO_SubCode2, QD_Date, QD_Release ");
            commandBuilder.AppendLine("from Quotas ");
            commandBuilder.AppendLine("join QuotaObjects on QO_QTID = QT_ID ");
            commandBuilder.AppendLine("join QuotaDetails on QD_QTID = QT_ID ");
            commandBuilder.AppendLine("join QuotaParts on QP_QDID = QD_ID ");
            commandBuilder.AppendLine("left join StopSales on SS_QDID = QD_ID and (SS_IsDeleted is null or SS_IsDeleted = 0) ");
            commandBuilder.AppendLine(String.Format("where QO_SVKey = {0} ", (int)serviceClass));
            commandBuilder.AppendLine(String.Format("and QO_Code = {0} ", code));
            commandBuilder.AppendLine("and (SS_Date is null or SS_Date = QD_Date) ");
            commandBuilder.AppendLine("and (QP_IsDeleted is null or QP_IsDeleted = 0) ");
            commandBuilder.AppendLine("and (QD_IsDeleted is null or QD_IsDeleted = 0) ");
            //commandBuilder.AppendLine("and (QP_AgentKey is null or QP_AgentKey = 0) ");
            commandBuilder.AppendLine("and (QP_IsNotCheckin is null or QP_IsNotCheckin = 0) ");
            commandBuilder.AppendLine(String.Format("and QD_Date >= '{0:yyyy-MM-dd}'", DateTime.Now.Date));
            commandBuilder.AppendLine("union ");
            commandBuilder.AppendLine("select null as SS_QDID, null as QD_Type, SS_AllotmentAndCommitment, 0 as QP_Busy, 0 as QP_CheckInPlacesBusy, 0 as QP_Places, 0 as QP_CheckInPlaces, '' as QP_Durations, QO_ID, SS_ID, 0 as QD_ID, SS_PRKey as QT_PRKey, 0 as QP_AgentKey, ISNULL(QO_SubCode1, 0) as QO_SubCode1, ISNULL(QO_SubCode2, 0) as QO_SubCode2, SS_Date as QD_Date, null as QD_Release ");
            commandBuilder.AppendLine("from QuotaObjects ");
            commandBuilder.AppendLine("join StopSales on SS_QOID = QO_ID ");
            commandBuilder.AppendLine("where QO_QTID is null ");
            commandBuilder.AppendLine("and (SS_IsDeleted is null or SS_IsDeleted = 0) ");
            commandBuilder.AppendLine(String.Format("and QO_SVKey = {0} ", (int)serviceClass));
            commandBuilder.AppendLine(String.Format("and QO_Code = {0} ", code));
            commandBuilder.AppendLine(String.Format("and SS_Date >= '{0:yyyy-MM-dd}'", DateTime.Now.Date));

            using (var command = dc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                dc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new QuotaPlain()
                        {
                            SsQdId = reader.GetInt32OrNull("SS_QDID"),
                            Type = reader.GetInt16OrNull("QD_Type") == null || reader.GetInt16OrNull("QD_Type") == 1 ? QuotaType.Allotment : QuotaType.Commitment,
                            IsAllotmentAndCommitment = reader.GetBooleanOrNull("SS_AllotmentAndCommitment") != null && reader.GetBooleanOrNull("SS_AllotmentAndCommitment") != false,
                            Busy = reader.GetInt32("QP_Busy"),
                            CheckInPlacesBusy = reader.GetInt32OrNull("QP_CheckInPlacesBusy"),
                            Places = reader.GetInt32("QP_Places"),
                            CheckInPlaces = reader.GetInt32OrNull("QP_CheckInPlaces"),
                            Duration = reader.GetString("QP_Durations"),
                            QoId = reader.GetInt32("QO_ID"),
                            SsId = reader.GetInt32OrNull("SS_ID"),
                            QdId = reader.GetInt32("QD_ID"),
                            PartnerKey = reader.GetInt32("QT_PRKey"),
                            AgentKey = reader.GetInt32("QP_AgentKey"),
                            SubCode1 = reader.GetInt32("QO_SubCode1"),
                            SubCode2 = reader.GetInt32("QO_SubCode2"),
                            Date = reader.GetDateTime("QD_Date"),
                            Release = reader.GetInt16OrNull("QD_Release")
                        });
                    }
                }
                dc.Connection.Close();
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список квот по заданным параметрам
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="serviceClass">Класс услуги</param>
        /// <param name="code">Code услуги (для авиаперелетов - Charter, для проживания - ключ отеля)</param>
        /// <param name="subCode1">SubCode1 услуги, для перелета - класс, для проживания - тип номера (Room)</param>
        /// <param name="subCode2">SubCode1 услуги, для перелета - не используется, для проживания - категория номера (RoomCategory)</param>
        /// <param name="partnerKey">Ключ партнера по услуге</param>
        /// <param name="date">Дата проверки квоты</param>
        /// <param name="agentKey">Ключ агентства</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static List<QuotaPlain> GetPlainQuotasObjects(this MtSearchDbDataContext dc, ServiceClass serviceClass, Int32 code,
            Int32 subCode1, Int32? subCode2, Int32 partnerKey, int? agentKey, DateTime date, out string hash)
        {
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, (int)serviceClass, code, 
                CacheHelper.GetCacheKeyHashed(new[]
                {
                    partnerKey.ToString(), 
                    agentKey.ToString(),
                    subCode1.ToString(), 
                    subCode2.ToString(), 
                    date.ToString("yyyy-MM-dd")
                }));
            List<QuotaPlain> result;
            if ((result = CacheHelper.GetCacheItem<List<QuotaPlain>>(hash)) != null) return result;

            string cacheDep;
            result = (from pq in dc.GetPlainQuotasObjects(serviceClass, code, out cacheDep)
                where (pq.PartnerKey <= 0 || pq.PartnerKey == partnerKey)
                      && ((agentKey != null && (pq.AgentKey == agentKey.Value || pq.AgentKey == 0)) || (agentKey == null && pq.AgentKey == 0))
                      && (pq.SubCode1 <= 0 || pq.SubCode1 == subCode1)
                      && (subCode2 == null || pq.SubCode2 <= 0 || pq.SubCode2 == subCode2.Value)
                      && (pq.Date == date)
                select pq)
                .ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { cacheDep }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
