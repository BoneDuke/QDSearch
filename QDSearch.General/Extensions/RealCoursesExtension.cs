using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение для работы с реальными курсами
    /// </summary>
    public static class RealCoursesExtension
    {
        private static readonly object LockRealCourses = new object();
        private static readonly object LockCrossCourses = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "RealCourses";

        /// <summary>
        /// Возвращает список реальных курсов на будущие даты + сегодня
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static IList<SimpleCourse> GetActualRealCourses(this MtSearchDbDataContext dc)
        {
            List<SimpleCourse> realCourses;
            if ((realCourses = CacheHelper.GetCacheItem<List<SimpleCourse>>(TableName)) != null) return realCourses;

            lock (LockRealCourses)
            {
                if ((realCourses = CacheHelper.GetCacheItem<List<SimpleCourse>>(TableName)) != null) return realCourses;

                realCourses = (from rcourse in dc.RealCourses
                               join r1 in dc.Rates on rcourse.RC_RCOD1 equals r1.RA_CODE
                               join r2 in dc.Rates on rcourse.RC_RCOD2 equals r2.RA_CODE
                               where rcourse.RC_DATEEND >= DateTime.Now.Date
                               select new SimpleCourse
                               {
                                   Course = rcourse.RC_COURSE.Value,
                                   CourseCb = rcourse.RC_COURSE_CB.Value,
                                   DateFrom = rcourse.RC_DATEBEG.Value,
                                   DateTo = rcourse.RC_DATEEND.Value,
                                   RateKeyFrom = r2.ra_key,
                                   RateKeyTo = r1.ra_key
                               })
                    .ToList();

                CacheHelper.AddCacheData(TableName, realCourses, TableName);
            }

            return realCourses;
        }

        /// <summary>
        /// Возвращает список реальных курсов на сегодня
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static IList<CrossCourse> GetTodayCrossCourses(this MtSearchDbDataContext dc)
        {
            List<CrossCourse> crossCourses;
            const string hash = "CrossCourses";
            if ((crossCourses = CacheHelper.GetCacheItem<List<CrossCourse>>(hash)) != null) return crossCourses;

            lock (LockCrossCourses)
            {
                if ((crossCourses = CacheHelper.GetCacheItem<List<CrossCourse>>(hash)) != null) return crossCourses;

                crossCourses = (from rcourse in dc.RealCourses
                               join r1 in dc.Rates on rcourse.RC_RCOD1 equals r1.RA_CODE
                               join r2 in dc.Rates on rcourse.RC_RCOD2 equals r2.RA_CODE
                               where DateTime.Now.Date >= rcourse.RC_DATEBEG
                               && DateTime.Now.Date <= rcourse.RC_DATEEND
                               select new CrossCourse
                               {
                                   Course = rcourse.RC_COURSE.Value,
                                   RateCodeFrom = r2.RA_CODE,
                                   RateCodeTo = r1.RA_CODE
                               })
                    .ToList();

                CacheHelper.AddCacheData(hash, crossCourses, TableName);
            }

            return crossCourses;
        }

        /// <summary>
        /// Реальный курс на дату
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="date">Дата курса</param>
        /// <param name="rateCodeFrom">Валюта, из которой конвертируем</param>
        /// <param name="rateCodeTo">Валюта, в которую конвертируем</param>
        /// <returns></returns>
        public static decimal GetRateRealCourse(this MtSearchDbDataContext dc, DateTime date, string rateCodeFrom, string rateCodeTo)
        {
            return dc.GetRateRealCourse(date, dc.GetRateKeyByCode(rateCodeFrom), dc.GetRateKeyByCode(rateCodeTo));
        }

        /// <summary>
        /// Реальный курс на дату
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="date">Дата курса</param>
        /// <param name="rateFrom">Валюта, из которой конвертируем</param>
        /// <param name="rateTo">Валюта, в которую конвертируем</param>
        /// <returns></returns>
        public static decimal GetRateRealCourse(this MtSearchDbDataContext dc, DateTime date, int rateFrom, int rateTo)
        {
            if (rateFrom == rateTo)
                return 1;

            var hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, date.ToString("dd.MM.yyyy"), rateFrom, rateTo);
            if (CacheHelper.IsCacheKeyExists(hash))
            {
                return CacheHelper.GetCacheItem<decimal>(hash);
            }

            var realCourses = dc.GetActualRealCourses();

            var result = (from rc in realCourses
                where date >= rc.DateFrom
                      && date <= rc.DateTo
                      && rc.RateKeyFrom == rateFrom
                      && rc.RateKeyTo == rateTo
                select rc.Course)
                .FirstOrDefault();

            if (result == 0)
            {
                result = (from rc in realCourses
                          where date >= rc.DateFrom
                          && date <= rc.DateTo
                          && rc.RateKeyFrom == rateTo
                          && rc.RateKeyTo == rateFrom
                          select rc.Course)
                              .FirstOrDefault();

                if (result != 0)
                    result = 1 / result;
            }

            if (result == 0)
                throw new ApplicationException(MethodBase.GetCurrentMethod().Name + ". Отсутствует реальный курс на дату " + date.ToString("dd.MM.yyyy"));

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);

            return result;
        }
    }
}
