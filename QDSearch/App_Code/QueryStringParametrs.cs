using System;
using System.Linq;
using System.Net;
using System.Web;
using QDSearch.DataModel;

/// <summary>
/// Summary description for QueryStringParametrs
/// </summary>
public class QueryStringParametrs
{
    private readonly HttpRequest _request;

    public bool IsParametrsValid { get; private set; }

    public bool IsEmpty { get; private set; }

    public int? CityFromKey { get; private set; }

    public int? CountryToKey { get; private set; }

    public int[] TourTypeKeys { get; private set; }

    public int[] CitiesToKeys { get; private set; }

    public int[] TourKeys { get; private set; }

    public DateTime? ArrivalDateFrom { get; private set; }
    public DateTime? ArrivalDateTo { get; private set; }

    public int[] Nights { get; private set; }

    public string[] HotelCategoriesKeys { get; private set; }

    public int[] PansionKeys { get; private set; }

    public bool IsHotelsFiltredByArrNights { get; private set; }

    public int[] HotelKeys { get; private set; }

    public int[] RoomTypeKeys { get; private set; }

    public ushort? AdultsNumber { get; private set; }

    public ushort? ChildsNumber { get; private set; }

    public ushort? FirstChildAge { get; private set; }

    public ushort? SecondChildAge { get; private set; }

    public QuotesStates? RoomsQuotesStates { get; private set; }

    public QuotesStates? AviaQuotesStates { get; private set; }

    public int? RateKey { get; private set; }

    public uint? MaxPrice { get; private set; }
    
    public ushort? RowsNumber { get; private set; }

    public bool? ShowResults { get; private set; }

    public QueryStringParametrs(HttpRequest request)
    {
        _request = request;
        var queryString = _request.QueryString;
        if (!queryString.HasKeys())
        {
            IsEmpty = true;
            IsParametrsValid = false;
            return;
        }

        IsParametrsValid = true;
        IsEmpty = true;

        try
        {
            // todo: доделать обработку обязательных параметров в строке запроса.
            var strPrmValues = queryString["country"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                CountryToKey = int.Parse(strPrmValues);

            strPrmValues = queryString["departFrom"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                CityFromKey = int.Parse(strPrmValues);


            strPrmValues = queryString["tourtype"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                TourTypeKeys = strPrmValues.Split(',').Select(int.Parse).ToArray();

            strPrmValues = queryString["city"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                CitiesToKeys = strPrmValues.Split(',').Select(int.Parse).ToArray();

            strPrmValues = queryString["tour"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                TourKeys = strPrmValues.Split(',').Select(int.Parse).ToArray();

            strPrmValues = queryString["dateFrom"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                ArrivalDateFrom = DateTime.Parse(strPrmValues);
            strPrmValues = queryString["dateTo"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
            {
                if (ArrivalDateFrom.HasValue)
                    ArrivalDateTo = DateTime.Parse(strPrmValues);
                else
                    throw new FormatException("QueryString parametr dateTo mast be used with parametr dateFrom");
            }
            else
            {
                if (ArrivalDateFrom.HasValue)
                    throw new FormatException("QueryString parametr dateFrom mast be used with parametr dateTo");
            }

            strPrmValues = queryString["nights"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                Nights = strPrmValues.Split(',').Select(int.Parse).ToArray();

            strPrmValues = WebUtility.UrlDecode(queryString["stars"]);
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                HotelCategoriesKeys = strPrmValues.Split(',').ToArray();

            strPrmValues = queryString["pansion"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                PansionKeys = strPrmValues.Split(',').Select(int.Parse).ToArray();

            strPrmValues = queryString["filterHotelsArrNights"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                IsHotelsFiltredByArrNights = bool.Parse(strPrmValues);

            strPrmValues = queryString["hotel"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                HotelKeys = strPrmValues.Split(',').Select(int.Parse).ToArray();

            strPrmValues = queryString["room"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                RoomTypeKeys = strPrmValues.Split(',').Select(int.Parse).ToArray();

            strPrmValues = queryString["adults"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                AdultsNumber = ushort.Parse(strPrmValues);
            strPrmValues = queryString["childs"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                ChildsNumber = ushort.Parse(strPrmValues);
            strPrmValues = queryString["firstChildAge"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                FirstChildAge = ushort.Parse(strPrmValues);
            strPrmValues = queryString["secondChildAge"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                SecondChildAge = ushort.Parse(strPrmValues);

            strPrmValues = queryString["aviaQuotaMask"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
            {
                QuotesStates tmp;
                if (!Enum.TryParse(strPrmValues, true, out tmp))
                    throw new FormatException("aviaQuotaMask querystring parametr is wrong.");
                AviaQuotesStates = tmp | QuotesStates.None;
            }

            strPrmValues = queryString["hotelQuotaMask"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
            {
                QuotesStates tmp;
                if (!Enum.TryParse(strPrmValues, true, out tmp))
                    throw new FormatException("hotelQuotaMask querystring parametr is wrong.");
                RoomsQuotesStates = tmp;
            }

            strPrmValues = queryString["currency"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                RateKey = int.Parse(strPrmValues);
            strPrmValues = queryString["priceLimit"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                MaxPrice = uint.Parse(strPrmValues);
            strPrmValues = queryString["pageSize"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                RowsNumber = ushort.Parse(strPrmValues);
            strPrmValues = queryString["showResults"];
            if (!String.IsNullOrWhiteSpace(strPrmValues))
                ShowResults = bool.Parse(strPrmValues);
        }
        catch (FormatException e)
        {
            IsEmpty = false;
            IsParametrsValid = false;
            return;
        }

        IsEmpty =
            !(CityFromKey.HasValue || CountryToKey.HasValue || (TourTypeKeys != null && TourTypeKeys.Any()) ||
              (CitiesToKeys != null && CitiesToKeys.Any()) || (TourKeys != null && TourKeys.Any()) ||
              (ArrivalDateFrom.HasValue && ArrivalDateTo.HasValue) || (Nights != null && Nights.Any()) ||
              (HotelCategoriesKeys != null && HotelCategoriesKeys.Any()) || (PansionKeys != null && PansionKeys.Any()) ||
              (HotelKeys != null && HotelKeys.Any()) || (RoomTypeKeys != null && RoomTypeKeys.Any()) ||
              AdultsNumber.HasValue || ChildsNumber.HasValue || FirstChildAge.HasValue || SecondChildAge.HasValue ||
              AviaQuotesStates.HasValue || RoomsQuotesStates.HasValue || RateKey.HasValue || MaxPrice.HasValue || RowsNumber.HasValue || ShowResults.HasValue);
    }
}