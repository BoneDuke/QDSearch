using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с таблицей QuestionnaireTouristCases (варианты ответов)
    /// </summary>
    public static class QuestionnaireTouristCasesExtension
    {
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "QuestionnaireTouristCase";

        /// <summary>
        /// Возвращает список вариантов ответов по туристу
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="touristKey">КЛюч туриста</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IEnumerable<QuestionnaireTouristCase> GetQuestCasesByTourist(this MtMainDbDataContext dc, int touristKey, out string hash)
        {
            List<QuestionnaireTouristCase> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, touristKey);
            if ((result = CacheHelper.GetCacheItem<List<QuestionnaireTouristCase>>(hash)) != null) return result;

            result = (from q in dc.QuestionnaireTouristCases
                where q.QTC_TUKey == touristKey
                select q)
                .ToList();

            if (!CacheHelper.IsCacheKeyExists(TableName))
                CacheHelper.AddCacheData(TableName, String.Empty, TableName);

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
