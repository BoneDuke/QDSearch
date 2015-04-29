<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ToursSearchBox.ascx.cs" Inherits="Controls.ToursSearchBox" %>

<%@ Register Src="ToursSearchFilter.ascx" TagName="ToursSearchFilter" TagPrefix="uc1" %>

<%@ Register Src="ToursSearchResults.ascx" TagName="ToursSearchResults" TagPrefix="uc2" %>
<asp:ScriptManagerProxy ID="SmProxy" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/scripts/toursSearchBox.js" />
    </Scripts>
</asp:ScriptManagerProxy>
<asp:Panel ID="UpToursSearchBox" runat="server" CssClass="ToursSearchBox">
    <uc1:ToursSearchFilter ID="ToursFilter" runat="server" OnSearchBottonClick="ToursFilters_OnSearchBottonClick" OnFilterSelectionChanged="ToursFilter_FilterSelectionChanged" />
    <asp:UpdatePanel ID="UpToursSearchResults" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="True">
        <ContentTemplate>
            <asp:Panel runat="server">
                <uc2:ToursSearchResults ID="ToursSearch" runat="server" />
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ToursFilter" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Panel>
