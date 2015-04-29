using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QDSearch;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using Seemplexity.Extensions.Questionnaire.DataModel;

namespace Seemplexity.Extensions.Questionnaire
{
    public static class Logic
    {
        public static IDictionary<string, Question> GetAllQuestionsByQuestionnaire(this MtMainDbDataContext dc, int questKey, int turistKey, out string hash)
        {
            Dictionary<string, Question> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, questKey);
            if ((result = CacheHelper.GetCacheItem<Dictionary<string, Question>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            result = (from q in dc.GetAllQuestFields()
                where q.QF_QUKey == questKey
                select q)
                .ToDictionary(q => q.QF_Bookmark, q => new Question
                {
                    Comment = q.QF_Comment,
                    Example = q.QF_Example,
                    Format = Helpers.Converters.StringToFormatType(q.QF_Format),
                    Key = q.QF_Key,
                    Name = q.QF_Name,
                    Value = q.QF_DefaultValue
                });
            cacheDependencies.Add(QuestionnaireFieldsExtension.TableName);

            foreach (var item in result.Values)
            {
                item.Cases = (from c in dc.GetAllQuestFieldCases()
                    where c.QFC_QFKey == item.Key
                    select new QuestionCase
                    {
                        IsChecked = c.QFC_IsDefault,
                        Key = c.QFC_Key,
                        Order = c.QFC_Order.HasValue ? c.QFC_Order.Value : 0,
                        Value = c.QFC_Value
                    })
                    .ToList();
            }
            cacheDependencies.Add(QuestionnaireFieldCasesExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        public static bool SaveAnswersByTourist(this MtMainDbDataContext dc, int questKey, int turistKey, IDictionary<string, Question> answers)
        {

            return true;
        }
    }
}
