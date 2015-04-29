<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckBoxListAll.ascx.cs" Inherits="Controls.ExtendedControls.CheckBoxListAll" %>

<asp:Panel ID="ControlWrapper" CssClass="ControlWrapper" runat="server">
    <asp:Label ID="ControlTitle" CssClass="ListTitle" runat="server" AssociatedControlID="ChbAllOptions">Title</asp:Label>
    <asp:UpdatePanel ID="UpAllOptions" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="True" class="OptionAllWrapper">
        <ContentTemplate>
            <asp:CheckBox ID="ChbAllOptions" Checked="True" runat="server" Text="любой" AutoPostBack="True" OnCheckedChanged="OptionsControl_CheckedChanged" />
        </ContentTemplate>
        <Triggers>
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
        </Triggers>
    </asp:UpdatePanel>
</asp:Panel>

