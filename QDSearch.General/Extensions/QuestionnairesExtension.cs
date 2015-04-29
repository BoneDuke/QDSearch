using System.Collections.Generic;
using System.Linq;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с таблицей Questionnaires (анкеты)
    /// </summary>
    public static class QuestionnairesExtension
    {
        private static readonly object LockQuestionnaires = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "Questionnaire";

        /// <summary>
        /// Возвращает список всех анкет из базы
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <returns></returns>
        public static IEnumerable<Questionnaire> GetAllQuestionnaires(this MtMainDbDataContext dc)
        {
            List<Questionnaire> result;
            if ((result = CacheHelper.GetCacheItem<List<Questionnaire>>(TableName)) != default(List<Questionnaire>)) return result;
            lock (LockQuestionnaires)
            {
                if ((result = CacheHelper.GetCacheItem<List<Questionnaire>>(TableName)) != default(List<Questionnaire>)) return result;

                result = (from q in dc.Questionnaires
                           select q)
                    .ToList();

                CacheHelper.AddCacheData(TableName, result, TableName);
            }
            return result;
        }
    }
}
