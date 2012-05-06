<%@ Page Title="Piczard - Simple Image Upload Control" Language="C#" MasterPageFile="~/test/masters/DefaultMasterPage.master" AutoEventWireup="true" CodeFile="default.aspx.cs" Inherits="test_default" %>
<%@ Register src="~/piczardUserControls/simpleImageUploadUserControl/SimpleImageUpload.ascx" tagname="SimpleImageUpload" tagprefix="uc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="pageBody" Runat="Server">

    <div class="PageTitle">
        <h1>
            SimpleImageUpload User Control
        </h1>
    </div>
    
    <asp:ScriptManager runat="server" ID="ScriptManager1">
    </asp:ScriptManager>            
        
    <asp:UpdatePanel runat="server" ID="UpdatePanel1">
        <ContentTemplate>
        
            <div class="pageContainer">  
                <br />    
                <strong>
                    Interactive image crop test:<br />
                </strong>            
                <uc1:SimpleImageUpload ID="ImageUpload1" runat="server" 
                    Width="500px"
                    AutoOpenImageEditPopupAfterUpload="true"
                 />
                
                <br />    
                <br />   
                <strong>
                    Automatic image resize test:<br />
                </strong>                  
                <uc1:SimpleImageUpload ID="ImageUpload2" runat="server" 
                    Width="500px"
                    EnableEdit="false"
                 />                 
            </div>
            
        </ContentTemplate>
    </asp:UpdatePanel>
    
</asp:Content>

