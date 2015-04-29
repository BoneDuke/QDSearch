using System.Collections.Generic;
using System.Linq;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с полями анкет (таблица QuestionnaireFields)
    /// </summary>
    public static class QuestionnaireFieldsExtension
    {
        private static readonly object LockFields = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "QuestionnaireField";

        /// <summary>
        /// Возвращает список всех полей анкет
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <returns></returns>
        public static IEnumerable<QuestionnaireField> GetAllQuestFields(this MtMainDbDataContext dc)
        {
            List<QuestionnaireField> result;
            if ((result = CacheHelper.GetCacheItem<List<QuestionnaireField>>(TableName)) != default(List<QuestionnaireField>)) return result;
            lock (LockFields)
            {
                if ((result = CacheHelper.GetCacheItem<List<QuestionnaireField>>(TableName)) != default(List<QuestionnaireField>)) return result;

                result = (from q in dc.QuestionnaireFields
                          select q)
                    .ToList();

                CacheHelper.AddCacheData(TableName, result, TableName);
            }
            return result;
        }
    }
}
