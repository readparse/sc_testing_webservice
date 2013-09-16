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
        Paste in your IDs in the field below and hit "Go".
        <asp:Button ID="Go" runat="server" Text="Go" onclick="Go_Click" />
        <br />
        <textarea ID="TextArea1" runat="server" cols=100 rows=20></textarea>
        <br />
        <asp:Label ID="Label1" runat="server"></asp:Label>
        
    </div>
    </form>
</body>
</html>
