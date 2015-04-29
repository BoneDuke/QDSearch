<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HotelsFilter.ascx.cs" Inherits="Controls.ExtendedControls.HotelsFilter" %>

<%--<asp:ScriptManagerProxy ID="SmProxy" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/scripts/hotelsFilter.js" />
    </Scripts>
</asp:ScriptManagerProxy>--%>
<asp:Panel ID="ControlWrapper" CssClass="HotelsFilterWrapper ControlWrapper" runat="server">
    <asp:Label ID="ControlTitle" CssClass="ListTitle" runat="server" AssociatedControlID="ChbAllOptions">Title</asp:Label>
    <div class="HotelNameFilterWrapper">
        <asp:TextBox ID="TbHotelNameFilter" runat="server" ClientIDMode="Static" CssClass="HotelNameFilter" ToolTip="Введите сюда имя отеля для его поиска в списке отелей."></asp:TextBox>
    </div>
    <asp:UpdatePanel ID="UpAllOptions" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False" class="OptionAllWrapper">
        <ContentTemplate>
            <asp:CheckBox ID="ChbAllOptions" Checked="True" runat="server" Text="любой" AutoPostBack="True" OnCheckedChanged="OptionsControl_CheckedChanged" />
            <asp:CheckBox ID="ChbFilterByArrNights" ClientIDMode="Static" Checked="False" runat="server" Text="показывать отели с учетом дат заезда и выбранных ночей" AutoPostBack="True" OnCheckedChanged="OptionsControl_CheckedChanged" ToolTip="При включении данной опции, список отелей будет отображаться с учетом выбранных дат заездов и выбранных продолжительностей в ночах. Т.е. если отель не был посчитан на указанные даты заездов и указанные продолжительности он не будет отображаться в списке отелей. Выключение этой опции позволит увидеть все отели по которым посчитаны туры с учетом выбранных города отправления, страны, региона и тура. ВАЖНО! Если при выклюнной опции вы выбирите отель который не был посчитан на выбранную дату заезда или продолжительность, то в резльтатах поиска тура на этот отель отображены не будут." />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ChbAllOptions" />
            <asp:AsyncPostBackTrigger ControlID="ChblOptions" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="UpOptions" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False" class="OptionsListWrapper">
        <ContentTemplate>
            <asp:CheckBoxList ID="ChblOptions" runat="server" CssClass="OptionsList" RepeatLayout="UnorderedList" RepeatDirection="Vertical" AutoPostBack="True" OnDataBound="ChblOptions_DataBound" OnSelectedIndexChanged="OptionsControl_CheckedChanged">
            </asp:CheckBoxList>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ChbAllOptions" />
            <asp:AsyncPostBackTrigger ControlID="ChbFilterByArrNights" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="SelectedOptionsListWrapper">
        <span class="SelectedOptionsList"></span>
    </div>
</asp:Panel>
