<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ToursSearchResults.ascx.cs" Inherits="Controls.ToursSearchResults" %>
<%@ Import Namespace="QDSearch.DataModel" %>
<div id="<%= ClientID %>" class="TFS_ToursSearchResults">
    <asp:ScriptManagerProxy ID="SmProxy" runat="server">
        <Scripts>
            <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.Core.js" />
            <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQuery.js" />
            <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQueryInclude.js" />
        </Scripts>
    </asp:ScriptManagerProxy>

    <asp:Literal ID="LtMessage" runat="server" ViewStateMode="Disabled" EnableViewState="False"></asp:Literal>

    <asp:Repeater ID="RepPagesTop" runat="server">
        <HeaderTemplate>
            <div id="<%= RepPagesTop.ClientID %>" class="TFS_PagerBlockTop">
                <asp:LinkButton ID="LbPrev" runat="server" CssClass="TFS_PagerBtnPrev" CommandName="PreviosPage" Visible='<%# CurrentPage > 1 %>' OnCommand="RepPagesItem_Command"><span class="TFS_PagerText">Предыдущая страница</span></asp:LinkButton>
        </HeaderTemplate>
        <ItemTemplate>
            <asp:LinkButton runat="server" CssClass="TFS_PagerItem" CommandName="PageToGo" CommandArgument='<%# Eval("Key") %>' Visible='<%# CurrentPage != (uint)Eval("Key") && SearchPages.Count > 1 %>' OnCommand="RepPagesItem_Command"><span class="TFS_PagerPageNumber"><%# Eval("Key") %></span></asp:LinkButton>
            <span id="SpanPage" runat="server" class="TFS_PagerItem TFS_PagerItemSelected TFS_PagerText" visible='<%# CurrentPage == (uint)Eval("Key") && SearchPages.Count > 1 %>'><%# Eval("Key") %></span>
        </ItemTemplate>
        <FooterTemplate>
            <asp:LinkButton ID="LbNext" runat="server" CssClass="TFS_PagerBtnNext" CommandName="NextPage" Visible='<%# CurrentPage != SearchPages.Count %>' OnCommand="RepPagesItem_Command"><span class="TFS_PagerText">Следующая страница</span></asp:LinkButton>
            <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                <ProgressTemplate>
                    <asp:Image runat="server" ImageUrl="~/styles/images/loading.gif" />
                </ProgressTemplate>
            </asp:UpdateProgress>
            </div>
        </FooterTemplate>
    </asp:Repeater>

    <asp:Repeater ID="RepFindedTours" runat="server">
        <HeaderTemplate>
            <div class="TFS_SeachedToursTableWrapper">
                <table id="tblFindedTours" class="TFS_SeachedToursTable" cellpadding="0" cellspacing="0">
                    <thead>
                        <tr class="THeadRow">
                            <th class="TFS_Sortable" rowspan="2">
                                <asp:LinkButton ID="LnbDates" CommandName="Sort" CommandArgument="<%# SortingColumn.TourDate %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Даты тура</span></asp:LinkButton>
                            </th>
                            <th class="TFS_Sortable" rowspan="2">
                                <asp:LinkButton ID="LnbCity" CommandName="Sort" CommandArgument="<%# SortingColumn.HotelRegionName %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Город</span></asp:LinkButton>
                            </th>
                            <th class="TFS_Sortable" rowspan="2">
                                <asp:LinkButton ID="LnbHotel" CommandName="Sort" CommandArgument="<%# SortingColumn.HotelName %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Отель</span></asp:LinkButton>
                            </th>
                            <th class="TFS_Sortable" rowspan="2">
                                <asp:LinkButton ID="LnbHotelCategory" CommandName="Sort" CommandArgument="<%# SortingColumn.HotelCategoryName %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Категория</span></asp:LinkButton>
                            </th>
                            <th class="TFS_Sortable">
                                <asp:LinkButton ID="LnbRoomType" CommandName="Sort" CommandArgument="<%# SortingColumn.RoomTypeName %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Тип номера</span></asp:LinkButton>
                            </th>
                            <th class="TFS_Sortable" rowspan="2">
                                <asp:LinkButton ID="LnbAccomodation" CommandName="Sort" CommandArgument="<%# SortingColumn.AccomodationName %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Размещение</span></asp:LinkButton>
                            </th>
                            <th class="TFS_Sortable" rowspan="2">
                                <asp:LinkButton ID="LnbPansion" CommandName="Sort" CommandArgument="<%# SortingColumn.PansionName %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Питание</span></asp:LinkButton>
                            </th>
                            <th class="TFS_Sortable" rowspan="2">
                                <asp:LinkButton ID="LnbNights" CommandName="Sort" CommandArgument="<%# SortingColumn.NightsCount %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Ночи</span></asp:LinkButton>
                            </th>
                            <th class="TFS_Sortable" rowspan="2" class="SortedUp">
                                <asp:LinkButton ID="LnbPrice" CommandName="Sort" CommandArgument="<%# SortingColumn.Price %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Цена</span></asp:LinkButton>
                            </th>
                            <th class="TFS_Sortable" rowspan="2">
                                <asp:LinkButton ID="LnbTour" CommandName="Sort" CommandArgument="<%# SortingColumn.TourName %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Тур</span></asp:LinkButton>
                            </th>
                            <th colspan="7" class="TFS_HeaderLastCell">Наличие мест
                            </th>
                        </tr>
                        <tr>
                            <th class="TFS_Sortable">
                                <asp:LinkButton ID="LinkButton2" CommandName="Sort" CommandArgument="<%# SortingColumn.RoomCategoryName %>" runat="server" OnCommand="HeaderCol_OnCommand"><span>Категория номера</span></asp:LinkButton>
                            </th>

                            <th class="TFS_SubHeaderCell">Отель</th>
                            <th colspan="2" class="TFS_SubHeaderCell">Эконом</th>
                            <th colspan="2" class="TFS_SubHeaderCell">Премиум</th>
                            <th colspan="2" class="TFS_SubHeaderLastCell">Бизнес</th>
                        </tr>
                    </thead>
                    <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <%# GetTourRecord(Container.DataItem as SearchResultItem, false) %>
        </ItemTemplate>
        <AlternatingItemTemplate>
            <%# GetTourRecord(Container.DataItem as SearchResultItem, true) %>
        </AlternatingItemTemplate>
        <FooterTemplate>
            </tbody>
            </table>
            </div>
        </FooterTemplate>
    </asp:Repeater>

    <asp:Repeater ID="RepPagesBottom" runat="server">
        <HeaderTemplate>
            <div id="<%= RepPagesTop.ClientID %>" class="TFS_PagerBlockButtom">
                <asp:LinkButton ID="LbPrev" runat="server" CssClass="TFS_PagerBtnPrev" CommandName="PreviosPage" Visible='<%# CurrentPage > 1 %>' OnCommand="RepPagesItem_Command"><span class="TFS_PagerText">Предыдущая страница</span></asp:LinkButton>
        </HeaderTemplate>
        <ItemTemplate>
            <asp:LinkButton runat="server" CssClass="TFS_PagerItem" CommandName="PageToGo" CommandArgument='<%# Eval("Key") %>' Visible='<%# CurrentPage != (uint)Eval("Key") && SearchPages.Count > 1 %>' OnCommand="RepPagesItem_Command"><span class="TFS_PagerPageNumber"><%# Eval("Key") %></span></asp:LinkButton>
            <span id="SpanPage" runat="server" class="TFS_PagerItem TFS_PagerItemSelected TFS_PagerText" visible='<%# CurrentPage == (uint)Eval("Key") && SearchPages.Count > 1  %>'><%# Eval("Key") %></span>
        </ItemTemplate>
        <FooterTemplate>
            <asp:LinkButton ID="LbNext" runat="server" CssClass="TFS_PagerBtnNext" CommandName="NextPage" Visible='<%# CurrentPage != SearchPages.Count %>' OnCommand="RepPagesItem_Command"><span class="TFS_PagerText">Следующая страница</span></asp:LinkButton>
            </div>
        </FooterTemplate>
    </asp:Repeater>

    <telerik:RadWindowManager ID="rWndManager" runat="server">
    </telerik:RadWindowManager>
</div>

