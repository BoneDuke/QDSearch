<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ToursSearchFilter.ascx.cs" Inherits="Controls.ToursSearchFilter" EnableViewState="true" ViewStateMode="Enabled" %>
<%@ Register Src="ExtendedControls/CheckBoxListAll.ascx" TagName="CheckBoxListAll" TagPrefix="uc2" %>
<%@ Register Src="ExtendedControls/ArrivalDatePicker.ascx" TagName="ArrivalDatePicker" TagPrefix="uc2" %>
<%@ Register Src="ExtendedControls/HotelsFilter.ascx" TagName="HotelsFilter" TagPrefix="uc3" %>

<asp:Panel ID="PnTourSearchFilter" runat="server" CssClass="TourSearchFilter">
    <asp:ScriptManagerProxy ID="SmProxy" runat="server">
        <Scripts>
            <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.Core.js" />
            <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQuery.js" />
            <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQueryInclude.js" />
        </Scripts>
    </asp:ScriptManagerProxy>

    <asp:UpdatePanel ID="UpCountryInfo" runat="server" UpdateMode="Conditional" class="TFS_CountryInfoBlock" ChildrenAsTriggers="False">
        <ContentTemplate>
            <asp:Literal ID="LtCountryInfo" runat="server"></asp:Literal>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
            <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
        </Triggers>
    </asp:UpdatePanel>

    <div class="TFS_FiltersShadow">
        <div class="TFS_TopBlock">
            <span class="TFS_ContriesToWrapper">
                <asp:UpdatePanel ID="UpContriesTo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true" RenderMode="Inline">
                    <ContentTemplate>
                        <span class="TFS_FilterNumber">1</span>
                        <asp:Label runat="server" CssClass="TFS_FilterTitle" AssociatedControlID="DdlContriesToFilter">Страна:</asp:Label>
                        <asp:DropDownList ID="DdlContriesToFilter" runat="server" DataValueField="Key" DataTextField="Value"
                            AutoPostBack="True" OnSelectedIndexChanged="Filter_SelectedIndexChanged">
                        </asp:DropDownList>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </span>
            <span class="TFS_CityFromWrapper">
                <asp:UpdatePanel ID="UPRblCitiesFromFilter" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true" RenderMode="Inline">
                    <ContentTemplate>
                        <span class="TFS_FilterNumber">2</span>
                        <asp:Label runat="server" CssClass="TFS_FilterTitle" AssociatedControlID="RblCitiesFromFilter">Билеты из:</asp:Label>
                        <asp:RadioButtonList ID="RblCitiesFromFilter" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal"
                            DataValueField="Key" DataTextField="Value" AutoPostBack="True" OnSelectedIndexChanged="Filter_SelectedIndexChanged">
                        </asp:RadioButtonList>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                    </Triggers>
                </asp:UpdatePanel>
            </span>
            <a href='~/' runat="server" class="TFS_ResetSearch"><span>Очистить фильтры</span></a>
        </div>

        <div class="TFS_FirstBlock">
            <div class="TFS_SecondBlock">

                <div class="TFS_TourTypesWrapper TFS_FilterWrapper">
                    <asp:UpdatePanel ID="UpTourTypesFilter" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="True" class="OptionsListWrapper">
                        <ContentTemplate>
                            <uc2:CheckBoxListAll ID="ChblTourTypes" runat="server" RepeatLayout="Flow" RepeatDirection="Vertical" Title="<span class='TFS_FilterNumber'>3</span> Тип тура:" AllOptionTitle="любой"
                                DataValueField="Key" DataTextField="Value" OnCheckedChanged="Filter_SelectedIndexChanged" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>

                <asp:UpdatePanel ID="UpCitiesToFilter" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False" class="TFS_CitiesToWrapper TFS_FilterWrapper">
                    <ContentTemplate>
                        <uc2:CheckBoxListAll ID="ChblCitiesToFilter" runat="server" RepeatLayout="Flow" RepeatDirection="Vertical" Title="<span class='TFS_FilterNumber'>4</span> Регион:" AllOptionTitle="любой"
                            DataValueField="Key" DataTextField="Value" OnCheckedChanged="Filter_SelectedIndexChanged" />
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                        <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                        <asp:AsyncPostBackTrigger ControlID="ChblTourTypes" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>

            <div class="TFS_RigthWrapper">
                <div class="TFS_ThirdBlock">
                    <div class="TSF_TourListWrapper TFS_FilterWrapper">
                        <asp:Label runat="server" CssClass="TFS_FilterTitle" AssociatedControlID="DdlToursFilter"><span class='TFS_FilterNumber'>5</span> СПО и туры:</asp:Label>
                        <asp:UpdatePanel ID="UpToursFilter" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False">
                            <ContentTemplate>
                                <asp:DropDownList ID="DdlToursFilter" runat="server" DataValueField="Key" DataTextField="Value"
                                    AutoPostBack="True" OnSelectedIndexChanged="Filter_SelectedIndexChanged">
                                </asp:DropDownList>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                                <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                                <asp:AsyncPostBackTrigger ControlID="ChblTourTypes" />
                                <asp:AsyncPostBackTrigger ControlID="ChblCitiesToFilter" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>

                    <asp:UpdatePanel ID="UpArrivalDatesFilter" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="True" class="TFS_ArrivalWrapper TFS_FilterWrapper">
                        <ContentTemplate>
                            <uc2:ArrivalDatePicker ID="ArrivalDatesFilter" runat="server" Title="<span class='TFS_FilterNumber'>6</span> Дата заезда" OnSelectedArrivalRangeChanged="Filter_SelectedIndexChanged" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblTourTypes" />
                            <asp:AsyncPostBackTrigger ControlID="ChblCitiesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlToursFilter" />
                        </Triggers>
                    </asp:UpdatePanel>

                    <asp:UpdatePanel ID="UpNightsFilter" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False" class="TFS_NightsWrapper TFS_FilterWrapper">
                        <ContentTemplate>
                            <uc2:CheckBoxListAll ID="ChblNightsFilter" runat="server" Title="<span class='TFS_FilterNumber'>7</span> Ночи:" AllOptionTitle="все" RepeatDirection="Horizontal" RepeatLayout="Flow"
                                OnCheckedChanged="Filter_SelectedIndexChanged" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblTourTypes" />
                            <asp:AsyncPostBackTrigger ControlID="ChblCitiesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlToursFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ArrivalDatesFilter" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>

                <div class="TFS_ForthBlock">
                    <asp:UpdatePanel ID="UpHotelCategoriesFilter" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False" class="TFS_HotelCtgWrapper TFS_FilterWrapper">
                        <ContentTemplate>
                            <uc2:CheckBoxListAll ID="ChblHotelCategoriesFilter" runat="server" RepeatLayout="Flow" RepeatDirection="Vertical" Title="<span class='TFS_FilterNumber'>8</span> Категория:"
                                AllOptionTitle="любая" OnCheckedChanged="Filter_SelectedIndexChanged" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblTourTypes" />
                            <asp:AsyncPostBackTrigger ControlID="ChblCitiesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlToursFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ArrivalDatesFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblNightsFilter" />
                        </Triggers>
                    </asp:UpdatePanel>

                    <asp:UpdatePanel ID="UpPansionsFilter" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False" class="TFS_PansionsWrapper TFS_FilterWrapper">
                        <ContentTemplate>
                            <uc2:CheckBoxListAll ID="ChblPansionsFilter" runat="server" RepeatLayout="Flow" RepeatDirection="Vertical" DataValueField="Key" DataTextField="Value"
                                Title="<span class='TFS_FilterNumber'>9</span> Питание:" AllOptionTitle="любое" OnCheckedChanged="Filter_SelectedIndexChanged" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblTourTypes" />
                            <asp:AsyncPostBackTrigger ControlID="ChblCitiesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlToursFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ArrivalDatesFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblNightsFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblHotelCategoriesFilter" />
                        </Triggers>
                    </asp:UpdatePanel>

                    <asp:UpdatePanel ID="UpHotelsFilter" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False" class="TFS_HotelsWrapper TFS_FilterWrapper">
                        <ContentTemplate>
                            <uc3:HotelsFilter ID="UcHotelsFilter" runat="server" RepeatLayout="Flow" RepeatDirection="Vertical" DataValueField="Key" DataTextField="Value"
                                Title="<span class='TFS_FilterNumber'>10</span> Отель:" AllOptionTitle="любой" OnCheckedChanged="Filter_SelectedIndexChanged" OnFilterByArrNightsChanged="UcHotelsFilter_OnFilterByArrNightsChanged" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblTourTypes" />
                            <asp:AsyncPostBackTrigger ControlID="ChblCitiesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlToursFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ArrivalDatesFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblNightsFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblHotelCategoriesFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblPansionsFilter" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>

                <div class="TFS_FifthBlock">
                    <div class="TFS_MenRoomsQuotasWrapper">
                        <asp:UpdatePanel ID="UpRoomPersonsWrapper" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False" class="TFS_RoomPersonsWrapper TFS_FilterWrapper">
                            <ContentTemplate>
                                <%--к классу привязан JS--%>
                                <asp:Panel ID="PnPersons" runat="server" CssClass="TFS_PersonsWrapper">
                                    <asp:Label runat="server" AssociatedControlID="PnPersons" CssClass="TFS_FilterTitle">
                                        <span class='TFS_FilterNumber'>11</span> Количество человек в номере:</asp:Label>
                                    <div class="TFS_PersonsLeft">
                                        <asp:TextBox ID="TbAdultsFilter" ClientIDMode="Static" runat="server">2</asp:TextBox>
                                        <asp:Label runat="server" AssociatedControlID="TbAdultsFilter">&nbsp;взрослых</asp:Label><br />
                                        <asp:TextBox ID="TbChildsFilter" ClientIDMode="Static" runat="server">0</asp:TextBox>
                                        <asp:Label runat="server" AssociatedControlID="TbChildsFilter">&nbsp;детей</asp:Label>
                                    </div>
                                    <div id="ExtraChildrenDiv" style='<%= SelectedChildsNumber > 0 ? String.Empty: "display: none" %>' class="TFS_ExtraChildrenWrapper">
                                        <asp:Label runat="server" AssociatedControlID="TbFirstChildAge">1-й ребенок</asp:Label>&nbsp;<asp:TextBox ID="TbFirstChildAge" ClientIDMode="Static" runat="server"></asp:TextBox>&nbsp;<span>лет</span><br />
                                        <span id="SecondChlSpan" style='<%= SelectedChildsNumber > 1 ? String.Empty: "display: none" %>'>
                                            <asp:Label runat="server" AssociatedControlID="TbSecondChildAge">2-й ребенок</asp:Label>&nbsp;<asp:TextBox ID="TbSecondChildAge" ClientIDMode="Static" runat="server"></asp:TextBox>&nbsp;<span>лет</span></span>
                                    </div>
                                </asp:Panel>
                                <asp:Panel ID="PnRoomTypes" ClientIDMode="Static" runat="server" CssClass="TFS_RoomsWrapper" Visible="False">
                                    <asp:Label runat="server" AssociatedControlID="DdlRoomTypesFilter" CssClass="TFS_FilterTitle"><span class='TFS_FilterNumber'>11</span> Тип номера:</asp:Label>
                                    <asp:DropDownList ID="DdlRoomTypesFilter" ClientIDMode="Static" runat="server" DataValueField="Key" DataTextField="Value" AutoPostBack="True"
                                        OnSelectedIndexChanged="Filter_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </asp:Panel>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                                <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                                <asp:AsyncPostBackTrigger ControlID="ChblTourTypes" />
                                <asp:AsyncPostBackTrigger ControlID="ChblCitiesToFilter" />
                                <asp:AsyncPostBackTrigger ControlID="DdlToursFilter" />
                                <asp:AsyncPostBackTrigger ControlID="ArrivalDatesFilter" />
                                <asp:AsyncPostBackTrigger ControlID="ChblNightsFilter" />
                                <asp:AsyncPostBackTrigger ControlID="ChblHotelCategoriesFilter" />
                                <asp:AsyncPostBackTrigger ControlID="ChblPansionsFilter" />
                                <asp:AsyncPostBackTrigger ControlID="UcHotelsFilter" />
                            </Triggers>
                        </asp:UpdatePanel>

                        <div class="TFS_QuotasWrapper TFS_FilterWrapper">
                            <div>
                                <label class="TFS_FilterTitle"><span class='TFS_FilterNumber'>12</span> Наличие мест:</label>
                                <table class="TFS_QuotesTbl">
                                    <tbody>
                                        <tr class="TFS_QuotasCaption">
                                            <td>&nbsp;
                                            </td>
                                            <td class="QTitleEnabled TFS_FilterTitle">+
                                            </td>
                                            <td class="QTitleRQ TFS_FilterTitle">?
                                            </td>
                                            <td class="QTitleNo TFS_FilterTitle">&ndash;
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="TFS_HotelsQuotesImgCell">&nbsp;
                                            </td>
                                            <td class="QValEnabled">
                                                <asp:CheckBox ID="ChRoomsEnabled" ClientIDMode="Static" runat="server" Checked="True" />
                                            </td>
                                            <td class="QValRQ">
                                                <asp:CheckBox ID="ChRoomsRQ" ClientIDMode="Static" runat="server" Checked="True" />
                                            </td>
                                            <td class="QValNo">
                                                <asp:CheckBox ID="ChRoomsNo" ClientIDMode="Static" runat="server" Checked="True" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="TFS_FlightQuotesImgCell">&nbsp;
                                            </td>
                                            <td class="QValEnabled">
                                                <asp:CheckBox ID="ChAviaAvailiable" ClientIDMode="Static" runat="server" Checked="True" />
                                            </td>
                                            <td class="QValRQ">
                                                <asp:CheckBox ID="ChAviaRQ" ClientIDMode="Static" runat="server" Checked="True" />
                                            </td>
                                            <td class="QValNo">
                                                <asp:CheckBox ID="ChAviaNo" ClientIDMode="Static" runat="server" Checked="True" />
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>

                        <div class="TFS_MoneyWrapper TFS_FilterWrapper">
                            <div>
                                <div class="TFS_RatesWrapper TFS_FilterWrapper">
                                    <asp:Label runat="server" AssociatedControlID="RblRates" class="TFS_FilterTitle"><span class='TFS_FilterNumber'>13</span> Валюта:</asp:Label>
                                    <asp:RadioButtonList ID="RblRates" ClientIDMode="Static" runat="server" RepeatDirection="Horizontal"
                                        RepeatLayout="Flow" DataValueField="Key" DataTextField="Value" />
                                </div>

                                <div class="TFS_MaxPriceWrapper TFS_FilterWrapper">
                                    <asp:Label runat="server" AssociatedControlID="TbMaxPrice" class="TFS_FilterTitle"><span class='TFS_FilterNumber'>14</span> Максимальная цена:</asp:Label>
                                    <asp:TextBox ID="TbMaxPrice" ClientIDMode="Static" runat="server"></asp:TextBox>&nbsp;<span id="RateTitle" class="TFS_RateLabel"></span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="TFS_SixthBlock">
                    <asp:Panel ID="TFS_RowsNumberWrapper" runat="server" CssClass='TFS_RowsNumberWrapper'>
                        <asp:Label runat="server" CssClass="TFS_FilterTitle" AssociatedControlID="DdlRowsNumber">Строк на странице: </asp:Label>
                        <asp:DropDownList ID="DdlRowsNumber" AutoPostBack="True" OnSelectedIndexChanged="Filter_SelectedIndexChanged" runat="server">
                            <asp:ListItem>20</asp:ListItem>
                            <asp:ListItem>30</asp:ListItem>
                            <asp:ListItem>40</asp:ListItem>
                            <asp:ListItem>50</asp:ListItem>
                        </asp:DropDownList>
                    </asp:Panel>
                    <asp:UpdatePanel ID="UpSearchBtnWrapper" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False" class="TFS_SearchBtnWrapper">
                        <ContentTemplate>
                            <asp:Button ID="BtnSearch" ClientIDMode="Static" runat="server" CssClass="TFS_SearchBtn" Text="Подобрать" OnClick="BtnSearch_Click" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="RblCitiesFromFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlContriesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblTourTypes" />
                            <asp:AsyncPostBackTrigger ControlID="ChblCitiesToFilter" />
                            <asp:AsyncPostBackTrigger ControlID="DdlToursFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ArrivalDatesFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblNightsFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblHotelCategoriesFilter" />
                            <asp:AsyncPostBackTrigger ControlID="ChblPansionsFilter" />
                            <asp:AsyncPostBackTrigger ControlID="UcHotelsFilter" />
                            <asp:PostBackTrigger ControlID="BtnSearch" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <asp:UpdateProgress ID="updateProgress" runat="server" class="TFS_ProgressLoadBlock">
        <ProgressTemplate>
            <div class="ProgressBackground">
            </div>
            <img runat="server" class="ProgressIcon" height="48" width="48" src="~/styles/images/loading2.gif" alt="" />
        </ProgressTemplate>
    </asp:UpdateProgress>
</asp:Panel>





