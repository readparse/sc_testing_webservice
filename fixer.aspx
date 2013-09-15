<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="fixer.aspx.cs" Inherits="AttachmentFixer.fixer" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
<style>
    .blob 
    {
        color: Green;
    }
    .path 
    {
        color: Red;
    }
</style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Start Item Path: <asp:TextBox ID="start_item" runat="server" Width="600"></asp:TextBox>
        <asp:Button ID="Lookup" runat="server" Text="Lookup" onclick="Lookup_Click" />
        <br />
        <asp:ListBox ID="ListBox1" runat="server" Height="300" Width="800"></asp:ListBox>
        <br />
        <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
    </div>
    </form>
</body>
</html>
