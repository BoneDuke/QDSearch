using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using QDSearch.Extensions;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using QDSearch.Repository.SftWeb;
using Seemplexity.Services.Wcf.ToursSearch.DataModel;

namespace Seemplexity.Services.Wcf.ToursSearch
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ToursSearchService" in both code and config file together.
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class ToursSearchService : IToursSearchService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public IDictionary<int, string> GetCountriesTo()
        {
            IDictionary<int, string> countriesTo;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    countriesTo = mtsDc.GetCountriesTo(sftDc);
                }
            }
            return countriesTo;
        }

        public string GetCountryDescription(int countryKey)
        {
            string countryDescription;
            using (var mtDc = new MtMainDbDataContext())
            {
                countryDescription = mtDc.GetCountryDescription(countryKey);
            }
            return countryDescription;
        }

        public IDictionary<int, string> GetCitiesFrom(int countryKey)
        {
            IDictionary<int, string> citiesFrom;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    citiesFrom = mtsDc.GetCitiesDeparture(sftDc, countryKey);
                }
            }
            return citiesFrom;
        }

        public IDictionary<int, string> GetTourTypes(int cityKeyFrom, int countryKey)
        {
            IDictionary<int, string> tourTypes;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    tourTypes = mtsDc.GetTourTypes(sftDc, cityKeyFrom, countryKey);
                }
            }
            return tourTypes;
        }

        public IDictionary<int, string> GetCitiesTo(int cityKeyFrom, int countryKey, List<int> tourTypes)
        {
            IDictionary<int, string> citiesTo;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    citiesTo = mtsDc.GetCitiesTo(sftDc, cityKeyFrom, countryKey, tourTypes);
                }
            }
            return citiesTo;
        }

        public IDictionary<int, string> GetTours(int cityKeyFrom, int countryKey, List<int> tourTypes, List<int> cityToKeys)
        {
            IDictionary<int, string> tours;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    tours = mtsDc.GetTours(sftDc, cityKeyFrom, countryKey, tourTypes, cityToKeys);
                }
            }
            return tours;
        }

        public IList<DateTime> GetTourDates(int cityKeyFrom, int countryKey, List<int> cityToKeys, List<int> tourKeys)
        {
            IList<DateTime> tourDates;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    tourDates = mtsDc.GetTourDates(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys);
                }
            }
            return tourDates;
        }

        public IEnumerable<int> GetTourNights(int cityKeyFrom, int countryKey, List<int> cityToKeys, List<int> tourKeys, List<DateTime> tourDates)
        {
            IEnumerable<int> tourNights;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    tourNights = mtsDc.GetTourNights(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates);
                }
            }
            return tourNights;
        }

        public IEnumerable<string> GetTourHotelClasses(int cityKeyFrom, int countryKey, List<int> cityToKeys, List<int> tourKeys, List<DateTime> tourDates,
            List<int> tourNights)
        {
            IEnumerable<string> hotelClasses;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    hotelClasses = mtsDc.GetTourHotelClasses(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys,
                        tourDates, tourNights);
                }
            }
            return hotelClasses;
        }

        public IDictionary<int, string> GetTourPansions(int cityKeyFrom, int countryKey, List<int> cityToKeys, List<int> tourKeys, List<DateTime> tourDates,
            List<int> tourNights, List<string> hotelCats)
        {
            IDictionary<int, string> tourPansions;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    tourPansions = mtsDc.GetTourPansions(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates, tourNights, hotelCats);
                }
            }
            return tourPansions;
        }

        // ReSharper disable RedundantNameQualifier
        public IEnumerable<Seemplexity.Services.Wcf.ToursSearch.DataModel.Hotel> GetTourHotels(int cityKeyFrom, int countryKey, List<int> cityToKeys, List<int> tourKeys, List<DateTime> tourDates,
            List<int> tourNights, List<string> hotelCats, List<int> pansionKeys)
        // ReSharper restore RedundantNameQualifier
        {
            // ReSharper disable RedundantNameQualifier
            IEnumerable<Seemplexity.Services.Wcf.ToursSearch.DataModel.Hotel> tourHotels;
            // ReSharper restore RedundantNameQualifier
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    tourHotels =
                        // ReSharper disable RedundantNameQualifier
                        Seemplexity.Services.Wcf.ToursSearch.DataModel.Hotel.GetHotelsList(
                        // ReSharper restore RedundantNameQualifier
                            mtsDc.GetTourHotels(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates,
                        // ReSharper disable once RedundantArgumentDefaultValue
                                tourNights, hotelCats, pansionKeys, false).ToList());
                }
            }
            return tourHotels;
        }

        public string GetTourDescription(int tourKey)
        {
            string description;
            using (var dc = new MtSearchDbDataContext())
            {
                description = dc.GetTourDescription(tourKey);
            }
            return description;
        }

        public bool IsCountryPriceForMen(int countryKey)
        {
            bool isCountryPriceForMen;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                isCountryPriceForMen = mtsDc.IsCountryPriceForMen(countryKey);
            }
            return isCountryPriceForMen;
        }

        public IDictionary<int, string> GetTourRooms(int cityKeyFrom, int countryKey, List<int> cityToKeys, List<int> tourKeys, List<DateTime> tourDates, List<int> tourNights, List<int> hotelKeys, List<int> pansionKeys)
        {
            IDictionary<int, string> tourRooms;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var sftDc = new SftWebDbDataContext())
                {
                    tourRooms = mtsDc.GetTourRooms(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates, tourNights, hotelKeys, pansionKeys);
                }
            }
            return tourRooms;
        }

        public IDictionary<int, string> GetCurrencies()
        {
            IDictionary<int, string> currencies;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                currencies = mtsDc.GetCurrencies();
            }
            return currencies;
        }

        // ReSharper disable RedundantNameQualifier
        public Seemplexity.Services.Wcf.ToursSearch.DataModel.SearchResult SearchToursWithRoomPrice(int cityKeyFrom, int countryKey, List<int> tourKeys,
            // ReSharper restore RedundantNameQualifier
            List<DateTime> tourDates, List<int> tourNights, List<int> hotelKeys, List<int> pansionKeys,
            ushort? adults, ushort? childs, ushort? firstChildYears, ushort? secondChildYears,
            QuotesStates hotelQuotaMask, QuotesStates aviaQuotaMask, int rateKey, uint? maxTourPrice, ushort rowsPerPage,
            uint rowCounterFrom, SortingColumn sortingColumn, SortingDirection sortingDirection)
        {
            if (IsCountryPriceForMen(countryKey))
                throw new ApplicationException(
                    "Данная страна посчитана с ценами за человека. Используйте для поиска метод SearchToursWithPriceForMen.");

            // ReSharper disable RedundantNameQualifier
            Seemplexity.Services.Wcf.ToursSearch.DataModel.SearchResult searchResult;
            // ReSharper restore RedundantNameQualifier
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var mtmDc = new MtMainDbDataContext())
                {
                    searchResult =
                        // ReSharper disable RedundantNameQualifier
                        new Seemplexity.Services.Wcf.ToursSearch.DataModel.SearchResult(mtsDc.PagingOnClient(mtmDc,
                        // ReSharper restore RedundantNameQualifier
                            cityKeyFrom, countryKey, tourKeys, tourDates, tourNights, hotelKeys,
                            pansionKeys, adults, childs, firstChildYears, secondChildYears, null,
                            (QDSearch.DataModel.QuotesStates)hotelQuotaMask,
                            (QDSearch.DataModel.QuotesStates)aviaQuotaMask, rateKey, maxTourPrice, rowsPerPage,
                            rowCounterFrom,
                            new Tuple<QDSearch.DataModel.SortingColumn, QDSearch.DataModel.SortingDirection>(
                                (QDSearch.DataModel.SortingColumn)sortingColumn,
                                (QDSearch.DataModel.SortingDirection)sortingDirection)));
                }
            }

            return searchResult;
        }

        public SearchResult SearchToursWithPriceForMen(int cityKeyFrom, int countryKey, List<int> tourKeys,
            List<DateTime> tourDates, List<int> tourNights, List<int> hotelKeys, List<int> pansionKeys,
            List<int> roomTypeKeys, QuotesStates hotelQuotaMask, QuotesStates aviaQuotaMask, int rateKey,
            uint? maxTourPrice, ushort rowsPerPage, uint rowCounterFrom, SortingColumn sortingColumn,
            SortingDirection sortingDirection, FlightTicketState flightTicketState)
        {
            if (!IsCountryPriceForMen(countryKey))
                throw new ApplicationException(
                    "Данная страна посчитана с ценами за номер. Используйте для поиска метод SearchToursWithRoomPrice.");

            // ReSharper disable RedundantNameQualifier
            Seemplexity.Services.Wcf.ToursSearch.DataModel.SearchResult searchResult;
            // ReSharper restore RedundantNameQualifier
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var mtmDc = new MtMainDbDataContext())
                {
                    searchResult =
                        // ReSharper disable RedundantNameQualifier
                        new Seemplexity.Services.Wcf.ToursSearch.DataModel.SearchResult(mtsDc.PagingOnClient(mtmDc,
                        // ReSharper restore RedundantNameQualifier
                            cityKeyFrom, countryKey, tourKeys, tourDates, tourNights, hotelKeys,
                            pansionKeys, null, null, null, null, roomTypeKeys,
                            (QDSearch.DataModel.QuotesStates)hotelQuotaMask,
                            (QDSearch.DataModel.QuotesStates)aviaQuotaMask, rateKey, maxTourPrice, rowsPerPage,
                            rowCounterFrom,
                            new Tuple<QDSearch.DataModel.SortingColumn, QDSearch.DataModel.SortingDirection>(
                                (QDSearch.DataModel.SortingColumn)sortingColumn,
                                (QDSearch.DataModel.SortingDirection)sortingDirection)));
                }
            }

            return searchResult;
        }
    }
}
