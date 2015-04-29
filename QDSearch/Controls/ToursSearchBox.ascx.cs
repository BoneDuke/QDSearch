using System;
using QDSearch.Helpers;

namespace Controls
{
    public partial class ToursSearchBox : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ToursFilters_OnSearchBottonClick(object sender, EventArgs e)
        {
            //todo:дописать автоматическую перезагрузку результатов поиска в случае если изменено количество строк в выдаче
            if (ToursFilter.IsPriceForRoom)
            {
                ToursSearch.ShowTours(ToursFilter.SelectedCityFromKey, ToursFilter.SelectedCountryToKey,
                    ToursFilter.SelectedTourKeys, ToursFilter.SelectedArrivalDates, ToursFilter.SelectedNights,
                    ToursFilter.SelectedHotelKeys, ToursFilter.SelectedPansionKeys, ToursFilter.SelectedAdultsNumber,
                    ToursFilter.SelectedChildsNumber, ToursFilter.SelectedFistChildAge,
                    ToursFilter.SelectedSecondChildAge, null, ToursFilter.SelectedRoomsQuotesStates,
                    ToursFilter.SelectedAviaQuotesStates, ToursFilter.SelectedRateKey, ToursFilter.SelectedMaxPrice, ToursFilter.SelectedRowsNumber);
            }
            else
            {
                ToursSearch.ShowTours(ToursFilter.SelectedCityFromKey, ToursFilter.SelectedCountryToKey,
                    ToursFilter.SelectedTourKeys, ToursFilter.SelectedArrivalDates, ToursFilter.SelectedNights,
                    ToursFilter.SelectedHotelKeys, ToursFilter.SelectedPansionKeys, null,
                    null, null,
                    null, ToursFilter.SelectedRoomTypeKey, ToursFilter.SelectedRoomsQuotesStates,
                    ToursFilter.SelectedAviaQuotesStates, ToursFilter.SelectedRateKey, ToursFilter.SelectedMaxPrice, ToursFilter.SelectedRowsNumber);
            }
            Web.ScrollToElement(this, ToursSearch.ClientID);
        }
        protected void ToursFilter_FilterSelectionChanged(object sender, EventArgs e)
        {
            ToursSearch.ClearSeachedTours();
        }
    }
}