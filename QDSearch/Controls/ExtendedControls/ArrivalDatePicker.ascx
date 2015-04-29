<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ArrivalDatePicker.ascx.cs" Inherits="Controls.ExtendedControls.ArrivalDatePicker" %>

<asp:Panel runat="server" class="ArrivalDatePicker">
    <asp:ScriptManagerProxy ID="scriptManagerProxy" runat="server">
    </asp:ScriptManagerProxy>
    <asp:Label ID="LbFilterTitle" runat="server" class="FilterTitle">Дата заезда&nbsp;</asp:Label>
    <asp:Label runat="server" AssociatedControlID="clDatesFrom">
        с:&nbsp;</asp:Label>

    <telerik:RadDatePicker ID="clDatesFrom" runat="server" Width="100px" ShowPopupOnFocus="True" AutoPostBack="True" Culture="ru-RU" SharedCalendarID="clShared" OnSelectedDateChanged="clDatesFrom_SelectedDateChanged">
        <DateInput DisplayDateFormat="dd.MM.yyyy" DateFormat="dd.MM.yyyy" LabelWidth="40%" AutoPostBack="True">
            <EmptyMessageStyle Resize="None"></EmptyMessageStyle>

            <ReadOnlyStyle Resize="None"></ReadOnlyStyle>

            <FocusedStyle Resize="None"></FocusedStyle>

            <DisabledStyle Resize="None"></DisabledStyle>

            <InvalidStyle Resize="None"></InvalidStyle>

            <HoveredStyle Resize="None"></HoveredStyle>

            <EnabledStyle Resize="None"></EnabledStyle>
        </DateInput>

        <DatePopupButton ImageUrl="" HoverImageUrl=""></DatePopupButton>
    </telerik:RadDatePicker>


    <asp:Label runat="server" AssociatedControlID="clDatesTo">&nbsp;по:&nbsp;</asp:Label>

    <telerik:RadDatePicker ID="clDatesTo" runat="server" Width="100px" ShowPopupOnFocus="True" AutoPostBack="True" Culture="ru-RU" SharedCalendarID="clShared" OnSelectedDateChanged="clDatesTo_SelectedDateChanged">
        <DateInput DisplayDateFormat="dd.MM.yyyy" DateFormat="dd.MM.yyyy" LabelWidth="40%" AutoPostBack="True">
            <EmptyMessageStyle Resize="None"></EmptyMessageStyle>

            <ReadOnlyStyle Resize="None"></ReadOnlyStyle>

            <FocusedStyle Resize="None"></FocusedStyle>

            <DisabledStyle Resize="None"></DisabledStyle>

            <InvalidStyle Resize="None"></InvalidStyle>

            <HoveredStyle Resize="None"></HoveredStyle>

            <EnabledStyle Resize="None"></EnabledStyle>
        </DateInput>

        <DatePopupButton ImageUrl="" HoverImageUrl=""></DatePopupButton>
    </telerik:RadDatePicker>

    <asp:Panel ID="pnArrivalRangeInfo" runat="server" CssClass="AvlbPeriodBlk">
        <label class="FilterTitle">Доступный период:</label>
        <asp:Literal ID="ltAvlArrivalDates" runat="server"></asp:Literal>
    </asp:Panel>

    <telerik:RadCalendar ID="clShared" runat="server" CssClass="ArrivalCalendar" AutoPostBack="False" CultureInfo="ru-RU" EnableKeyboardNavigation="True"
        EnableMultiSelect="False" EnableWeekends="True" ShowRowHeaders="False" ShowOtherMonthsDays="true" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False">
        <SpecialDays>
            <telerik:RadCalendarDay Date="" Repeatable="Today">
                <ItemStyle CssClass="rcToday" />
            </telerik:RadCalendarDay>
        </SpecialDays>
        <FastNavigationSettings EnableTodayButtonSelection="True">
        </FastNavigationSettings>
        <WeekendDayStyle CssClass="rcWeekend"></WeekendDayStyle>

        <CalendarTableStyle CssClass="rcMainTable"></CalendarTableStyle>

        <OtherMonthDayStyle CssClass="rcOtherMonth"></OtherMonthDayStyle>

        <OutOfRangeDayStyle CssClass="rcOutOfRange"></OutOfRangeDayStyle>

        <DisabledDayStyle CssClass="rcDisabled"></DisabledDayStyle>

        <SelectedDayStyle CssClass="rcSelected"></SelectedDayStyle>

        <DayOverStyle CssClass="rcHover"></DayOverStyle>

        <FastNavigationStyle CssClass="RadCalendarMonthView RadCalendarMonthView_Default"></FastNavigationStyle>

        <ViewSelectorStyle CssClass="rcViewSel"></ViewSelectorStyle>
    </telerik:RadCalendar>

</asp:Panel>
