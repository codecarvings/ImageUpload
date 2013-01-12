<%@ Page Title="Piczard - Simple Image Upload Control" Language="VB" MasterPageFile="~/test/masters/DefaultMasterPage.master" AutoEventWireup="false" CodeFile="default.aspx.vb" Inherits="test_default" %>
<%@ Register src="~/piczardUserControls/simpleImageUploadUserControl/SimpleImageUpload.ascx" tagname="SimpleImageUpload" tagprefix="ccPiczardUC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="pageBody" Runat="Server">

    <div class="PageTitle">
        <h1>
            SimpleImageUpload User Control - WebSite - VB
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
                <ccPiczardUC:SimpleImageUpload ID="ImageUpload1" runat="server" 
                    Width="500px"
                    AutoOpenImageEditPopupAfterUpload="true"
                    Culture="en"
                 />
                
                <br />   
                <br />    
                <strong>
                    Automatic image resize test:<br />
                </strong>                  
                <ccPiczardUC:SimpleImageUpload ID="ImageUpload2" runat="server" 
                    Width="500px"
                    EnableEdit="false"
                 />  
                 
                <br />    
                <br />   
                <asp:Button runat="server" ID="btnDummyPostback" Text="PostBack" style="padding:10px;" />             

                &nbsp;&nbsp;&nbsp;
                <asp:Button runat="server" ID="btnLoadImage" Text="Load image" style="padding:10px;" />
            </div>
            
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>

