<%@ Control Language="VB" AutoEventWireup="false" CodeFile="SimpleImageUpload.ascx.vb" Inherits="SimpleImageUpload" %>
<%@ Register assembly="CodeCarvings.Piczard" namespace="CodeCarvings.Piczard.Web" tagprefix="ccPiczard" %>
<div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("container0")) %>" style="<% =Me.GetRenderStyleWidth() %>">
    <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("container1")) %>" style="padding:5px; background-color:#eeeeee; border:solid 1px #cccccc;">
    
        <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("content")) %>" style="padding:5px; background-color:#ffffff; border:solid 1px #cccccc; overflow: auto;">
            <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("content_statusMessage")) %>" style="display:<% =if(Me.HasImage, "none", "inline") %>">
                <asp:Literal runat="server" id="litStatusMessage" EnableViewState="false">
                    No image selected.
                </asp:Literal>
            </div>
            <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("content_preview")) %>" style="display:<% =if(Me.HasImage, "inline", "none") %>">
            <asp:PlaceHolder runat="server" ID="phImageContainer" EnableViewState="false">
                <asp:HyperLink runat="server" ID="hlPictureImageEdit" NavigateUrl="#" EnableViewState="false">
                    <asp:Image runat="server" ID="imgPreview" AlternateText="Preview" BorderStyle="Solid" BorderColor="#cccccc" BorderWidth="1px" EnableViewState="false" />
                </asp:HyperLink>
            </asp:PlaceHolder>
            </div>
        </div>
        
        <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("commands")) %>" style="padding: 5px 0px 0px 0px;">
            <asp:PlaceHolder runat="server" ID="phEditCommands" EnableViewState="false">
                <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("editCommands")) %>" style="display:inline; float: left;">
                    <asp:Button runat="server" ID="btnEdit" CausesValidation="false" Text="Edit..." CssClass="DoNotApplyButtonStyle" style="padding:0; margin:0;" EnableViewState="false" />
                    <asp:Button runat="server" ID="btnRemove" CausesValidation="false" Text="Remove" CssClass="DoNotApplyButtonStyle" style="padding:0; margin:0;" EnableViewState="false" />
                </div>
                <div style="display:inline; float: left; width:10px; height:10px;">
                </div>
            </asp:PlaceHolder>
            
            <asp:PlaceHolder runat="server" ID="phUploadCommands" EnableViewState="false">
                <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("uploadCommands")) %>" style="display:inline; float: left;">
                    <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("uploadContainer_0")) %>" style="position: relative; width:<% =(Me.ButtonSize.Width + 25).ToString() %>px; height:<% =(Me.ButtonSize.Height).ToString() %>px; overflow: hidden; display:none;">
                        <asp:Button runat="server" ID="btnBrowseDisabled" CausesValidation="false" Text="Browse..." Enabled="false" CssClass="DoNotApplyButtonStyle" style="display:inline; padding:0; margin:0;" EnableViewState="false" />
                        <asp:Button runat="server" ID="btnCancelUpload" CausesValidation="false" Text="Cancel upload" CssClass="DoNotApplyButtonStyle" style="display:none; padding:0; margin:0;" EnableViewState="false" />
                        <asp:Image runat="server" AlternateText="Uploading file..." ImageUrl="wait.gif" style="width:16px; height:16px; margin-left:5px;" EnableViewState="false" />
                    </div>
                    <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("uploadContainer_1")) %>" style="position: relative; width:<% =(Me.ButtonSize.Width + 25).ToString() %>px; height:<% =(Me.ButtonSize.Height).ToString() %>px; overflow: hidden; display:inline;">
                        <asp:Button runat="server" ID="btnBrowse" CausesValidation="false" Text="Browse..." OnClientClick="return false;" Enabled="false" CssClass="DoNotApplyButtonStyle" style="padding:0; margin:0;" EnableViewState="false" />
                        <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("uploadPlaceHolder")) %>"
                            style="opacity: 0; filter:alpha(opacity: 0);  position: absolute; top:0px; left:0px;">
                        </div>
                    </div>    
                </div>
            </asp:PlaceHolder>
            
            <br style="clear:both;" />
        </div>
        
        <div style="display:none;">
            <asp:Button runat="server" ID="btnPostBack" CausesValidation="false" Text="PostBack" CssClass="DoNotApplyButtonStyle" EnableViewState="false" />
        </div>
        
        <asp:Literal runat="server" ID="litScript" EnableViewState="false">
        </asp:Literal>
        
        <asp:HiddenField runat="server" ID="hfAct" Value="" EnableViewState="false" />
        
        <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("popupExtContainer")) %>" style="display: none;">
            <div id="<% =HttpUtility.HtmlAttributeEncode(Me.GetSubElementId("popupExt")) %>" style="background-color:#999; color:#fff; height: 44px; line-height:44px; vertical-align:middle; padding-right:5px; overflow: hidden; text-align:right;">
                <asp:Literal runat="server" ID="litSelectConfiguration" EnableViewState="false"></asp:Literal>
                <asp:DropDownList runat="server" ID="ddlConfigurations" EnableViewState="false"></asp:DropDownList>
            </div>
        </div>        

        <ccPiczard:PopupPictureTrimmer runat="server" id="popupPictureTrimmer1"
        ShowImageAdjustmentsPanel="true" AutoFreezeOnFormSubmit="true" AutoPostBackOnPopupClose="Never"
        OnClientBeforePopupOpenFunction="CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.onImageEditBeforePopupOpen"
        OnClientAfterPopupCloseFunction="CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.onImageEditAfterPopupClose"
        EnableViewState="false" />
    </div>
</div>

        
