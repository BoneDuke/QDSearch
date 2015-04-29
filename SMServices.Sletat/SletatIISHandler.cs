using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using QDSearch.Repository.SftWeb;

namespace SMServices.Sletat
{
    public class SletatIISHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var httpCachePolicy = context.Response.Cache;
            httpCachePolicy.SetCacheability(HttpCacheability.NoCache);
            httpCachePolicy.SetLastModified(DateTime.Now);
            httpCachePolicy.SetMaxAge(TimeSpan.Zero);
            httpCachePolicy.SetRevalidation(HttpCacheRevalidation.AllCaches);
            httpCachePolicy.SetValidUntilExpires(false);
            httpCachePolicy.SetNoStore();

            context.Response.ContentType = "text/xml; charset=utf-8";
            context.Response.Write(@"<?xml version=""1.0"" encoding=""utf-8""?>");

            var queryString = context.Request.QueryString;

            if (QDSearch.Globals.Settings.Cache.Enabled)
            {
                var strXmlResponse = context.Cache[GetCacheKey(context)];
                if (strXmlResponse != null)
                {
                    context.Response.Write(strXmlResponse.ToString());
                    return;
                }
            }

            switch (queryString["action"])
            {
                case "GetCountries":
                    MakeResponse(context, @"getCountriesResult", (mtmDc, mtsDc, sftDc, id) =>
                    {
                        string hash;
                        return Tuple.Create(mtsDc.GetCountries(sftDc, id, out hash), hash);
                    });
                    break;
                case "GetDepartCities":
                    MakeResponse(context, @"getDepartCitiesResult", (mtmDc, mtsDc, sftDc, id) =>
                    {
                        string hash;
                        return Tuple.Create(mtsDc.GetDepartCities(sftDc, id, out hash), hash);
                    });
                    break;
                case "GetResorts":
                    MakeResponse(context, @"getResortsResult", (mtmDc, mtsDc, sftDc, id) =>
                    {
                        string hash;
                        return Tuple.Create(mtsDc.GetResorts(sftDc, id, out hash), hash);
                    });
                    break;
                case "GetHotelCategories":
                    MakeResponse(context, @"getHotelCategoriesResult", (mtmDc, mtsDc, sftDc, id) =>
                    {
                        string hash;
                        return Tuple.Create(mtmDc.GetHotelCategories(id, out hash), hash);
                    });
                    break;
                case "GetHotels":
                    MakeResponse(context, @"getHotelsResult", (mtmDc, mtsDc, sftDc, id) =>
                    {
                        string hash;
                        return Tuple.Create(mtmDc.GetHotels(mtsDc, sftDc, id, out hash), hash);
                    });
                    break;
                case "GetMeals":
                    MakeResponse(context, @"getMealsResult", (mtmDc, mtsDc, sftDc, id) =>
                    {
                        string hash;
                        return Tuple.Create(mtsDc.GetMeals(id, out hash), hash);
                    });
                    break;
                case "GetCurrencies":
                    MakeResponse(context, @"getCurrenciesResult", (mtmDc, mtsDc, sftDc, currencyKey) =>
                    {
                        string hash;
                        return Tuple.Create(mtsDc.GetCurrencies(currencyKey, out hash), hash);
                    });
                    break;
                case "GetTours":
                    MakeToursXmlResponse(context);
                    break;
                case "ActualizeTour":
                    MakeActualizeTourResponse(context);
                    break;
                default:
                    context.Response.Write("<error>Incorrect request.</error>");
                    break;
            }
        }

        #endregion

        private static string GetCacheKey(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return context.Request.QueryString.ToString().GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        private static void SaveXmlToResponse(HttpContext context, string resaultName, IXmlCompatible elementsList, string hash = null)
        {
            SaveXmlToResponse(context, resaultName, new List<IXmlCompatible> { elementsList }, hash);
        }

        private static void SaveXmlToResponse(HttpContext context, string resaultName, List<IXmlCompatible> elementsList, string hash = null)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (resaultName == null || String.IsNullOrWhiteSpace(resaultName)) throw new ArgumentNullException("resaultName");
            if (elementsList == null) throw new ArgumentNullException("elementsList");

            var sb = new StringBuilder();
            sb.AppendFormat(@"<{0} version=""1.0"">", resaultName);
            elementsList.ForEach(c => sb.Append(c.ToXml()));
            sb.AppendFormat(@"</{0}>", resaultName);
            context.Response.Write(sb.ToString());
            if (QDSearch.Globals.Settings.Cache.Enabled)
            {
                if (String.IsNullOrWhiteSpace(hash))
                    throw new ApplicationException("Ключ зависимости кеша - hash не может быть пустым в методах шлюза для слетать при включенном кешировании.");
                context.Cache.Add(GetCacheKey(context), sb.ToString(), new CacheDependency(null, new[] { hash }),
                    Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }
        }

        private static void MakeResponse<T>(HttpContext context, string resaultName, Func<MtMainDbDataContext, MtSearchDbDataContext, SftWebDbDataContext, int?, Tuple<List<T>, string>> getElementsFunc)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (getElementsFunc == null) throw new ArgumentNullException("getElementsFunc");
            if (resaultName == null || String.IsNullOrWhiteSpace(resaultName)) throw new ArgumentNullException("resaultName");

            var queryString = context.Request.QueryString;

            int? id = QDSearch.Helpers.Converters.GetIntSafe(queryString["id"]);

            if (queryString.AllKeys.Contains("id") && !id.HasValue)
            {
                context.Response.Write("<error>Incorrect parametr.</error>");
                return;
            }
            using (var mtmDc = new MtMainDbDataContext())
            {
                using (var mtsDc = new MtSearchDbDataContext())
                {
                    using (var sftDc = new SftWebDbDataContext())
                    {
                        Tuple<List<T>, string> tuple = getElementsFunc(mtmDc, mtsDc, sftDc, id);
                        SaveXmlToResponse(context, resaultName, tuple.Item1.ConvertAll(c => c as IXmlCompatible), tuple.Item2);
                    }
                }
            }
        }

        private static void MakeToursXmlResponse(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var queryString = context.Request.QueryString;

            var offerId = QDSearch.Helpers.Converters.GetLongSafe(queryString["offerId"]);
            int currencyId;

            if (queryString.AllKeys.Contains("offerId") && !offerId.HasValue)
            {
                context.Response.Write("<error>Incorrect parametr.</error>");
                return;
            }

            try
            {
                currencyId = int.Parse(queryString["currencyId"]);
            }
            catch (Exception)
            {
                context.Response.Write("<error>Incorrect parametr.</error>");
                return;
            }

            if (offerId.HasValue)
            {
                using (var mtmDc = new MtMainDbDataContext())
                {
                    using (var mtsDc = new MtSearchDbDataContext())
                    {
                        using (var sftDc = new SftWebDbDataContext())
                        {
                            string hash;
                            var elementsList = mtsDc.GetTours(mtmDc, sftDc, offerId.Value, currencyId, out hash).Item2.ConvertAll(c => c as IXmlCompatible);
                            SaveXmlToResponse(context, @"getToursResult", elementsList, hash);
                        }
                    }
                }
                return;
            }

            int count;
            int countryId;
            int departCityId;
            DateTime dateFrom;
            DateTime dateTo;
            ushort adults;
            ushort kids;
            List<ushort> kidsAges;
            int nightsMin;
            int nightsMax;
            List<int> resorts;
            List<int> hotelCategories;
            List<int> hotels;
            List<int> meals;
            uint? priceMin;
            uint? priceMax;
            int? hotelIsNotInStop;
            int? ticketsIncluded;
            int? hasTickets;

            try
            {
                count = int.Parse(queryString["count"]);
                countryId = int.Parse(queryString["countryId"]);
                departCityId = int.Parse(queryString["departCityId"]);
                dateFrom = DateTime.ParseExact(queryString["dateFrom"], "dd.MM.yyyy", new CultureInfo("ru-RU"));
                dateTo = DateTime.ParseExact(queryString["dateTo"], "dd.MM.yyyy", new CultureInfo("ru-RU"));
                adults = ushort.Parse(queryString["adults"]);
                kids = ushort.Parse(queryString["kids"]);
                kidsAges = kids > 0
                    ? queryString["kidsAges"].Split(',').Select(ushort.Parse).ToList()
                    : null;
                nightsMin = int.Parse(queryString["nightsMin"]);
                nightsMax = int.Parse(queryString["nightsMax"]);
                resorts = queryString.AllKeys.Contains("resorts")
                    ? queryString["resorts"].Split(',').Select(int.Parse).ToList()
                    : null;
                hotelCategories = queryString.AllKeys.Contains("hotelCategories")
                    ? queryString["hotelCategories"].Split(',').Select(int.Parse).ToList()
                    : null;
                hotels = queryString.AllKeys.Contains("hotels")
                    ? queryString["hotels"].Split(',').Select(int.Parse).ToList()
                    : null;
                meals = queryString.AllKeys.Contains("meals")
                    ? queryString["meals"].Split(',').Select(int.Parse).ToList()
                    : null;
                currencyId = int.Parse(queryString["currencyId"]);
                priceMin = queryString.AllKeys.Contains("priceMin")
                    ? uint.Parse(queryString["priceMin"])
                    : (uint?)null;
                priceMax = queryString.AllKeys.Contains("priceMax")
                    ? uint.Parse(queryString["priceMax"])
                    : (uint?)null;
                hotelIsNotInStop = queryString.AllKeys.Contains("hotelIsNotInStop")
                    ? int.Parse(queryString["hotelIsNotInStop"])
                    : (int?)null;
                ticketsIncluded = queryString.AllKeys.Contains("ticketsIncluded")
                    ? int.Parse(queryString["ticketsIncluded"])
                    : (int?)null;
                hasTickets = queryString.AllKeys.Contains("hasTickets")
                    ? int.Parse(queryString["hasTickets"])
                    : (int?)null;
            }
            catch (Exception)
            {
                context.Response.Write("<error>Incorrect parametr.</error>");
                return;
            }
            using (var mtmDc = new MtMainDbDataContext())
            {
                using (var mtsDc = new MtSearchDbDataContext())
                {
                    using (var sftDc = new SftWebDbDataContext())
                    {
                        string hash;
                        var elementsList = mtsDc.GetTours(mtmDc, sftDc, count, countryId, departCityId, dateFrom, dateTo, adults, kids, kidsAges, nightsMin, nightsMax, resorts, hotelCategories, hotels, meals, currencyId, priceMin, priceMax, hotelIsNotInStop, ticketsIncluded, hasTickets, out hash).Item2.ConvertAll(c => c as IXmlCompatible);
                        SaveXmlToResponse(context, "getToursResult", elementsList, hash);
                    }
                }
            }
        }

        private static void MakeActualizeTourResponse(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var queryString = context.Request.QueryString;

            long offerId;
            int currencyId;

            try
            {
                offerId = long.Parse(queryString["offerId"]);
                currencyId = int.Parse(queryString["currencyId"]);
            }
            catch (Exception)
            {
                context.Response.Write("<error>Incorrect parametr.</error>");
                return;
            }

            using (var mtmDc = new MtMainDbDataContext())
            {
                using (var mtsDc = new MtSearchDbDataContext())
                {
                    using (var sftDc = new SftWebDbDataContext())
                    {
                        string hash;
                        var elementsList = mtsDc.ActualizeTour(mtmDc, sftDc, offerId, currencyId, out hash) as IXmlCompatible;
                        SaveXmlToResponse(context, "actualizeTourResult", elementsList, hash);
                    }
                }
            }
        }
    }
}
