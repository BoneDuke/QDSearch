<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPageGeneral.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<%@ Register Src="Controls/ToursSearchBox.ascx" TagName="ToursSearchBox" TagPrefix="uc2" %>

<asp:Content runat="server" ID="CnHead" ContentPlaceHolderID="CphHead">
</asp:Content>
<asp:Content runat="server" ID="CnMain" ContentPlaceHolderID="CphMain">
    <uc2:ToursSearchBox ID="ToursSearchFilters" runat="server" />
</asp:Content>
