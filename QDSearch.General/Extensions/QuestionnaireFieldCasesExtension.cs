using System.Collections.Generic;
using System.Linq;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение для работы с таблицей вариантов ответов анкеты (таблица QuestionnaireFieldCases)
    /// </summary>
    public static class QuestionnaireFieldCasesExtension
    {
        private static readonly object LockFieldCases = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "QuestionnaireFieldCase";

        /// <summary>
        /// Возвращает список всех вариантов ответов
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <returns></returns>
        public static IEnumerable<QuestionnaireFieldCase> GetAllQuestFieldCases(this MtMainDbDataContext dc)
        {
            List<QuestionnaireFieldCase> result;
            if ((result = CacheHelper.GetCacheItem<List<QuestionnaireFieldCase>>(TableName)) != default(List<QuestionnaireFieldCase>)) return result;
            lock (LockFieldCases)
            {
                if ((result = CacheHelper.GetCacheItem<List<QuestionnaireFieldCase>>(TableName)) != default(List<QuestionnaireFieldCase>)) return result;

                result = (from q in dc.QuestionnaireFieldCases
                          select q)
                    .ToList();

                CacheHelper.AddCacheData(TableName, result, TableName);
            }
            return result;
        }
    }
}
