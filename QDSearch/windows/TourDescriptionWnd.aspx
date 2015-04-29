<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TourDescriptionWnd.aspx.cs" Inherits="windows.TourDescriptionWnd" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Условия бронирования.</title>
    <link id="LnkResetStyle" rel="stylesheet" runat="server" href="~/styles/reset.css" />
    <link id="LnkGeneralStyle" rel="stylesheet" runat="server" href="~/styles/trdescwnd.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="TFS_TourDescWrapper">
            <asp:Literal ID="LtContent" runat="server" Mode="Transform"></asp:Literal>
        </div>
    </form>
</body>
</html>
