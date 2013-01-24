/* 
 * Piczard | SimpleImageUpload User Control
 * Author: Sergio Turolla
 * <codecarvings.com>
 * 
 * Copyright (c) 2011-2013 Sergio Turolla
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the 
 * following conditions are met:
 * 
 * - Redistributions of source code must retain the above 
 *   copyright notice, this list of conditions and the 
 *   following disclaimer.
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the 
 *   following disclaimer in the documentation and/or other
 *   materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */
 
// #########
// SimpleImageUpload Version 3.0.5
// #########

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Xml;
using System.ComponentModel;

using CodeCarvings.Piczard;
using CodeCarvings.Piczard.Serialization;
using CodeCarvings.Piczard.Web;
using CodeCarvings.Piczard.Web.Helpers;

/// <summary>
/// A ready-to-use ASCX control that provides advanced image uploading features.</summary>
public partial class SimpleImageUpload
    : System.Web.UI.UserControl, IPostBackDataHandler 
{

    #region Consts

    protected static readonly Size DefaultImageEditPopupSize = new Size(800, 520);
    protected static readonly Size DefaultButtonSize = new Size(110, 26);
    protected static bool PerformTemporaryFolderWriteTestOnPageLoad = true;

    protected const string DefaultValues_BackColor = "#eeeeee";
    protected const string DefaultValues_BorderColor = "#cccccc";
    protected static readonly BorderStyle DefaultValues_BorderStyle = BorderStyle.Solid;
    protected static readonly Unit DefaultValues_BorderWidth = Unit.Pixel(1);

    protected const string DefaultValues_ContentBackColor = "#ffffff";
    protected const string DefaultValues_ContentForeColor = "#000000";
    protected const string DefaultValues_ContentErrorForeColor = "#cc0000";
    protected const string DefaultValues_ContentBorderColor = "#cccccc";
    protected static readonly BorderStyle DefaultValues_ContentBorderStyle = BorderStyle.Solid;
    protected static readonly Unit DefaultValues_ContentBorderWidth = Unit.Pixel(1);

    protected const string DefaultValues_PreviewBorderColor = "#cccccc";
    protected static readonly BorderStyle DefaultValues_PreviewBorderStyle = BorderStyle.Solid;
    protected static readonly Unit DefaultValues_PreviewBorderWidth = Unit.Pixel(1);

    #endregion

    #region Event Handlers

    protected void Page_Load(object sender, EventArgs e)
    {
        if (PerformTemporaryFolderWriteTestOnPageLoad)
        {
            // Check if the application can write on the temporary folder
            this.TemporaryFolderWriteTest();
        }

        // Load the JS file
        Type t = this.Page.GetType();
        string scriptKey = "simpleImageUpload.js";
        if (!this.Page.ClientScript.IsClientScriptIncludeRegistered(t, scriptKey))
        {
            this.Page.ClientScript.RegisterClientScriptInclude(t, scriptKey, this.ResolveUrl("simpleImageUpload.js?v=5"));
        }

        // Reset the initialization function
        this.popupPictureTrimmer1.OnClientControlLoadFunction = "";
    }

    #endregion

    #region Overrides

    #region ControlState

    protected override object SaveControlState()
    {
        List<object> values = new List<object>();
        values.Add(base.SaveControlState());

        values.Add(this.TemporaryFileId);
        values.Add(this._OutputResolution);
        values.Add(JSONSerializer.SerializeToString(this._CropConstraint));
        values.Add(JSONSerializer.SerializeToString(this._ImageUploadPreProcessingFilter, true)); // Important: also serialize the type!
        values.Add(JSONSerializer.SerializeToString(this._PostProcessingFilter, true)); // Important: also serialize the type!
        values.Add((int)this._PostProcessingFilterApplyMode);
        values.Add(JSONSerializer.SerializeToString(this._PreviewFilter, true)); // Important: also serialize the type!
        values.Add(this._ImageUploaded);
        values.Add(this._ImageEdited);
        values.Add(this._SourceImageClientFileName);
        values.Add(this._Configurations);

        // These properties are important and are not saved in the viewsate! (EnableViewState=false in ASCX)
        values.Add(this.imgPreview.ImageUrl); 
        values.Add(this.imgPreview.Width);
        values.Add(this.imgPreview.Height);

        values.Add(this._DebugUploadProblems);

        return values.ToArray();    
    }

    protected override void LoadControlState(object savedState)
    {
        if ((savedState != null) && (savedState is object[]))
        {
            object[] values = (object[])savedState;
            int i = 0;
            base.LoadControlState(values[i++]);

            this._TemporaryFileId = (string)values[i++];
            this._OutputResolution = (float)values[i++];
            this._CropConstraint = CropConstraint.FromJSON((string)values[i++]);
            this._ImageUploadPreProcessingFilter = (ImageProcessingFilter)JSONSerializer.Deserialize((string)values[i++]);
            this._PostProcessingFilter = (ImageProcessingFilter)JSONSerializer.Deserialize((string)values[i++]);
            this._PostProcessingFilterApplyMode = (SimpleImageUploadPostProcessingFilterApplyMode)(int)values[i++];
            this._PreviewFilter = (ImageProcessingFilter)JSONSerializer.Deserialize((string)values[i++]);
            this._ImageUploaded = (bool)values[i++];
            this._ImageEdited = (bool)values[i++];
            this._SourceImageClientFileName = (string)values[i++];
            this._Configurations = (string[])values[i++];

            this.imgPreview.ImageUrl = (string)values[i++];
            this.imgPreview.Width = (Unit)values[i++];
            this.imgPreview.Height = (Unit)values[i++];

            this._DebugUploadProblems = (bool)values[i++];
        }
    }

    #endregion

    #region Viewstate

    protected override object SaveViewState()
    {
        List<object> values = new List<object>();
        values.Add(base.SaveViewState());

        values.Add(this._Width);

        values.Add(this._BackColor);
        values.Add(this._BorderColor);
        values.Add(this._BorderStyle);
        values.Add(this._BorderWidth);

        values.Add(this._ContentBackColor);
        values.Add(this._ContentForeColor);
        values.Add(this._ContentErrorForeColor);
        values.Add(this._ContentBorderColor);
        values.Add(this._ContentBorderStyle);
        values.Add(this._ContentBorderWidth);

        values.Add(this._PreviewBorderColor);
        values.Add(this._PreviewBorderStyle);
        values.Add(this._PreviewBorderWidth);

        values.Add(this._AutoOpenImageEditPopupAfterUpload);
        values.Add(this._AutoDisableImageEdit);
        values.Add(this._ImageEditPopupSize);
        values.Add(this._ButtonSize);
        values.Add(this._CssClass);

        values.Add(this._EnableEdit);
        values.Add(this._EnableRemove);
        values.Add(this._EnableUpload);
        values.Add(this._EnableCancelUpload);

        values.Add(this._Text_EditButton);
        values.Add(this._Text_RemoveButton);
        values.Add(this._Text_BrowseButton);
        values.Add(this._Text_CancelUploadButton);
        values.Add(this._Text_ConfigurationLabel);
        values.Add(this._StatusMessage_NoImageSelected);
        values.Add(this._StatusMessage_UploadError);
        values.Add(this._StatusMessage_InvalidImage);
        values.Add(this._StatusMessage_InvalidImageSize);
        values.Add(this._StatusMessage_Wait);

        return values.ToArray();
    }

    protected override void LoadViewState(object savedState)
    {
        if ((savedState != null) && (savedState is object[]))
        {
            object[] values = (object[])savedState;
            int i = 0;
            base.LoadViewState(values[i++]);

            this._Width = (Unit)values[i++];

            this._BackColor = (Color)values[i++];
            this._BorderColor = (Color)values[i++];
            this._BorderStyle = (BorderStyle)values[i++];
            this._BorderWidth = (Unit)values[i++];

            this._ContentBackColor = (Color)values[i++];            
            this._ContentForeColor = (Color)values[i++];
            this._ContentErrorForeColor = (Color)values[i++];
            this._ContentBorderColor = (Color)values[i++];
            this._ContentBorderStyle = (BorderStyle)values[i++];
            this._ContentBorderWidth = (Unit)values[i++];

            this._PreviewBorderColor = (Color)values[i++];
            this._PreviewBorderStyle = (BorderStyle)values[i++];
            this._PreviewBorderWidth = (Unit)values[i++];

            this._AutoOpenImageEditPopupAfterUpload = (bool)values[i++];
            this._AutoDisableImageEdit = (bool)values[i++];
            this._ImageEditPopupSize = (Size)values[i++];
            this._ButtonSize = (Size)values[i++];
            this._CssClass = (string)values[i++];

            this._EnableEdit = (bool)values[i++];
            this._EnableRemove = (bool)values[i++];
            this._EnableUpload = (bool)values[i++];
            this._EnableCancelUpload = (bool)values[i++];

            this._Text_EditButton = (string)values[i++];
            this._Text_RemoveButton = (string)values[i++];
            this._Text_BrowseButton = (string)values[i++];
            this._Text_CancelUploadButton = (string)values[i++];
            this._Text_ConfigurationLabel = (string)values[i++];
            this._StatusMessage_NoImageSelected = (string)values[i++];
            this._StatusMessage_UploadError = (string)values[i++];
            this._StatusMessage_InvalidImage = (string)values[i++];
            this._StatusMessage_InvalidImageSize = (string)values[i++];
            this._StatusMessage_Wait = (string)values[i++];
        }
    }

    #endregion

    #region Render

    protected override void OnInit(EventArgs e)
    {
        this.Page.RegisterRequiresControlState(this);
        this.Page.RegisterRequiresPostBack(this);

        base.OnInit(e);
    }

    protected override void OnPreRender(EventArgs e)
    {
        #region Dynamic load CSS and JS files

        string crlf = ""; // "\r\n";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("<script type=\"text/javascript\">\r\n");
        //Google Chrome & Safari Ajax Bug
        bool isInAjaxPostBack = AjaxHelper.IsInAjaxPostBack(this.Page);
        if (!isInAjaxPostBack)
        {
            sb.Append("//<![CDATA[\r\n");
        }

        // JS function executed after the JS library is loaded
        sb.Append("function " + this.InitFunctionName2 + "()" + crlf);
        sb.Append("{" + crlf);
        sb.Append("var loadData={" + crlf);
        sb.Append("popupPictureTrimmerClientId:\"" + JSHelper.EncodeString(this.popupPictureTrimmer1.ClientID) + "\"" + crlf);
        sb.Append(",btnEditClientId:\"" + JSHelper.EncodeString(this.btnEdit.ClientID) + "\"" + crlf);
        sb.Append(",btnRemoveClientId:\"" + JSHelper.EncodeString(this.btnRemove.ClientID) + "\"" + crlf);
        sb.Append(",btnBrowseDisabledClientId:\"" + JSHelper.EncodeString(this.btnBrowseDisabled.ClientID) + "\"" + crlf);
        sb.Append(",btnCancelUploadClientId:\"" + JSHelper.EncodeString(this.btnCancelUpload.ClientID) + "\"" + crlf);
        sb.Append(",btnBrowseClientId:\"" + JSHelper.EncodeString(this.btnBrowse.ClientID) + "\"" + crlf);
        sb.Append(",hfActClientId:\"" + JSHelper.EncodeString(this.hfAct.ClientID) + "\"" + crlf);
        sb.Append(",ddlConfigurationsClientId:\"" + JSHelper.EncodeString(this.ddlConfigurations.ClientID) + "\"" + crlf);
        sb.Append(",hlPictureImageEditId:\"" + JSHelper.EncodeString(this.hlPictureImageEdit.ClientID) + "\"" + crlf);        

        sb.Append(",uploadUrl:\"" + JSHelper.EncodeString(this.UploadUrl) + "\"" + crlf);
        sb.Append(",uploadMonitorUrl:\"" + JSHelper.EncodeString(this.UploadMonitorUrl) + "\"" + crlf);
        sb.Append(",btnPostBack_PostBackEventReference:\"" + JSHelper.EncodeString(this.Page.ClientScript.GetPostBackEventReference(this.btnPostBack, "")) + "\"" + crlf);
        sb.Append(",imageEditPopupSize_width:" + this.ImageEditPopupSize.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) + crlf);
        sb.Append(",imageEditPopupSize_height:" + this.ImageEditPopupSize.Height.ToString(System.Globalization.CultureInfo.InvariantCulture) + crlf);
        sb.Append(",autoOpenImageEditPopup:" + JSHelper.EncodeBool(this._AutoOpenImageEditPopup) + crlf);
        sb.Append(",autoDisableImageEdit:" + JSHelper.EncodeBool(this._AutoDisableImageEdit) + crlf);
        sb.Append(",buttonSize_width:" + this.ButtonSize.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) + crlf);
        sb.Append(",buttonSize_height:" + this.ButtonSize.Height.ToString(System.Globalization.CultureInfo.InvariantCulture) + crlf);
        sb.Append(",enableEdit:" + JSHelper.EncodeBool(this._EnableEdit) + crlf);
        sb.Append(",enableRemove:" + JSHelper.EncodeBool(this._EnableRemove) + crlf);
        sb.Append(",enableCancelUpload:" + JSHelper.EncodeBool(this._EnableCancelUpload) + crlf);
        sb.Append(",dup:" + JSHelper.EncodeBool(this._DebugUploadProblems) + crlf);
        sb.Append(",statusMessage_Wait:\"" + JSHelper.EncodeString(this.StatusMessage_Wait) + "\"" + crlf);
        sb.Append("};" + crlf);
        sb.Append("var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.loadControl(\"" + JSHelper.EncodeString(this.ClientID) + "\", loadData);");
        sb.Append("}" + crlf);

        // Dynamic load JS / CSS (required for Ajax)
        sb.Append("function " + this.InitFunctionName + "()" + crlf);
        sb.Append("{" + crlf);
        sb.Append("if (typeof(window.__ccpz_siu_lt) === \"undefined\")" + crlf);
        sb.Append("{" + crlf);
        // The variable (window.__ccpz_siu_lt) (configured in simpleImageUpload.js) is undefined...
        sb.Append(JSHelper.GetLoadScript(this.ResolveUrl("simpleImageUpload.js?v=5"), this.InitFunctionName + "_load_js", this.InitFunctionName2 + "();") + crlf);
        sb.Append("}" + crlf);
        sb.Append("else" + crlf);
        sb.Append("{" + crlf);
        sb.Append(this.InitFunctionName2 + "();" + crlf);
        sb.Append("}" + crlf);
        sb.Append("}" + crlf);

        if (!isInAjaxPostBack)
        {
            sb.Append("\r\n//]]>\r\n");
        }
        sb.Append("</script>");

        string scriptToRegister = sb.ToString();
        if (!AjaxHelper.RegisterClientScriptBlockInAjaxPostBack(this.Page, "CCPZ_SIU_DAI_" + this.ClientID, scriptToRegister, false))
        {
            this.litScript.Text = scriptToRegister;
        }
        else
        {
            this.litScript.Text = "";
        }

        // Setup the initialization function
        if (this.Visible)
        {
            this.popupPictureTrimmer1.OnClientControlLoadFunction = this.InitFunctionName;
        }

        #endregion

        // Hide design-time elements
        this.phDesignTimeStart.Visible = false;
        this.phDesignTimeEnd.Visible = false;

        // Update the layout
        this.btnEdit.Visible = this.EnableEdit;
        this.btnEdit.Width = this.ButtonSize.Width;
        this.btnEdit.Height = this.ButtonSize.Height;

        this.btnRemove.Visible = this.EnableRemove;
        this.btnRemove.Width = this.ButtonSize.Width;
        this.btnRemove.Height = this.ButtonSize.Height;

        this.btnBrowse.Width = this.ButtonSize.Width;
        this.btnBrowseDisabled.Width = this.ButtonSize.Width;

        this.btnBrowse.Height = this.ButtonSize.Height;
        this.btnBrowseDisabled.Height = this.ButtonSize.Height;

        this.btnCancelUpload.OnClientClick = "CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.cancelUpload(\"" + JSHelper.EncodeString(this.ClientID) + "\"); return false;";

        this.btnCancelUpload.Width = this.ButtonSize.Width;
        this.btnCancelUpload.Height = this.ButtonSize.Height;

        this.phEditCommands.Visible = this.EnableEdit || this.EnableRemove;

        this.phUploadCommands.Visible = this.EnableUpload;
        this.btnCancelUpload.Visible = this.EnableCancelUpload;

        // Update the texts
        this.litStatusMessage.Text = this.GetCurrentStatusMessage();

        this.btnEdit.Text = this.Text_EditButton;
        this.btnRemove.Text = this.Text_RemoveButton;
        this.btnBrowse.Text = this.Text_BrowseButton;
        this.btnBrowseDisabled.Text = this.Text_BrowseButton;
        this.btnCancelUpload.Text = this.Text_CancelUploadButton;

        if (this.HasImage)
        {
            if (!File.Exists(this.PreviewImageFilePath))
            {
                // The preview file does not exists -> create it
                this._UpdatePreview = true;
            }

            if (this._UpdatePreview)
            {
                // Get the processing job (Default resolution = 96DPI)
                ImageProcessingJob job = this.GetImageProcessingJob();
                job.OutputResolution = CommonData.DefaultResolution;

                // Add the preview filter constraint
                if (this.PreviewFilter != null)
                {
                    job.Filters.Add(this.PreviewFilter);
                }

                // Save the preview image
                this.imgPreview.ImageUrl = null;
                if (File.Exists(this.TemporarySourceImageFilePath))
                {
                    // Jpeg images does not allow transparent images - Apply the right back color!
                    FormatEncoderParams format = new JpegFormatEncoderParams();
                    using (System.Drawing.Image previewImage = job.GetProcessedImage(this.TemporarySourceImageFilePath, format))
                    {
                        ImageArchiver.SaveImageToFileSystem(previewImage, this.PreviewImageFilePath, format);

                        // Force the reload of the preview
                        this.imgPreview.ImageUrl = this.PreviewImageUrl;
                        this.imgPreview.Width = Unit.Pixel(previewImage.Size.Width);
                        this.imgPreview.Height = Unit.Pixel(previewImage.Size.Height);
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(this.imgPreview.ImageUrl))
        {
            // Set a dummy image (for xhtml compliance)
            this.imgPreview.ImageUrl = this.ResolveUrl("blank.gif");
            this.imgPreview.Width = Unit.Pixel(1);
            this.imgPreview.Height = Unit.Pixel(1);
        }

        this.imgPreview.BorderColor = this._PreviewBorderColor;
        this.imgPreview.BorderStyle = this._PreviewBorderStyle;
        this.imgPreview.BorderWidth = this._PreviewBorderWidth;

        if (this._CropConstraint == null)
        {
            // Crop disabled
            this.popupPictureTrimmer1.ShowZoomPanel = false;
        }

        // Update the configuration UI
        this.litSelectConfiguration.Text = this.Text_ConfigurationLabel;
        this.ddlConfigurations.Items.Clear();
        string[] configurations = this.Configurations;
        if (configurations != null)
        {
            for (int i = 0; i < configurations.Length; i++)
            {
                this.ddlConfigurations.Items.Add(new ListItem(configurations[i], i.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }
            this.ddlConfigurations.SelectedIndex = this.SelectedConfigurationIndex.Value;
        }
        this.ddlConfigurations.Attributes["onchange"] = "CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.onConfigurationChange(\"" + JSHelper.EncodeString(this.ClientID) + "\");";

        // Hide the SELECT element to mantain compliance with XHTML specs (SELECT cannot be emtpy...)
        if (this.ddlConfigurations.Items.Count > 0)
        {
            this.ddlConfigurations.Visible = true;
        }
        else
        {
            this.ddlConfigurations.Visible = false;
        }

        if (!string.IsNullOrEmpty(this._CssClass))
        {
            string[] classes = this._CssClass.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (Array.IndexOf<string>(classes, "ccpz_fobr") >= 0)
            {
                this.popupPictureTrimmer1.CssClass = string.IsNullOrEmpty(this.popupPictureTrimmer1.CssClass) ? "ccpz_fobr" : this.popupPictureTrimmer1.CssClass + " ccpz_fobr";
            }
        }

        base.OnPreRender(e);
    }

    #endregion

    #endregion

    #region IPostBackDataHandler Members

    bool IPostBackDataHandler.LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
    {
        this._LastAct = this.hfAct.Value;
        // Reset the action
        this.hfAct.Value = "";

        string selectedConfigurationIndex = postCollection[this.ddlConfigurations.UniqueID];
        if (!string.IsNullOrEmpty(selectedConfigurationIndex))
        {
            // Int value
            this._SelectedConfigurationIndex = int.Parse(selectedConfigurationIndex, System.Globalization.CultureInfo.InvariantCulture);
        }
        else
        {
            // Null value
            this._SelectedConfigurationIndex = null;
        }

        switch (this._LastAct)
        {
            case "upload":
                // A new file has been uploaded
                if (File.Exists(this.UploadMonitorFilePath))
                {
                    XmlDocument doc = new XmlDocument();
                    using (StreamReader reader = File.OpenText(this.UploadMonitorFilePath))
                    {
                        // Load the document
                        doc.Load(reader);
                    }

                    XmlNodeList nodes = doc.GetElementsByTagName("uploadMonitor");
                    if (nodes.Count > 0)
                    {
                        XmlNode uploadMonitorNode = nodes[0];

                        XmlAttribute stateAttribute = uploadMonitorNode.Attributes["state"];
                        if (stateAttribute != null)
                        {
                            if (!string.IsNullOrEmpty(stateAttribute.Value))
                            {
                                this._UploadMonitorStatus = int.Parse(stateAttribute.Value);

                                if (this._UploadMonitorStatus.Value == 2)
                                {
                                    // Upload success
                                    // Get the file name

                                    this._SourceImageClientFileName = uploadMonitorNode.FirstChild.Value;
                                }

                                // Image upload (success / error)
                                return true;
                            }
                        }
                    }
                }
                break;
            case "edit":
                // Image edit
                return true;
            case "remove":
                // Image remove
                return true;
            case "configuration":
                // Selected confiugration index changed
                return true;
        }

        // No event
        return false;
    }

    void IPostBackDataHandler.RaisePostDataChangedEvent()
    {
        // Process the control events

        switch (this._LastAct)
        {
            case "upload":
                // Image upload (success / error)
                if (this._UploadMonitorStatus.HasValue)
                {
                    switch (this._UploadMonitorStatus.Value)
                    {
                        case 2:
                            // Upload success
                            this.ProcessUploadSuccess();
                            break;
                        case 3:
                            // Upload error
                            this.ProcessUploadError();
                            break;
                    }
                }
                break;
            case "edit":
                // Image edit
                this.ProcessEdit();
                break;
            case "remove":
                // Image remove
                this.ProcessRemove();
                break;
            case "configuration":
                // Selected confiugration index changed
                this.ProcessSelectedConfigurationIndexChanged();
                break;
        }
    }

    #endregion

    #region Properties

    #region Settings

    #region Appearance

    protected Unit _Width = Unit.Empty;
    /// <summary>
    /// Gets or sets the control width.</summary>
    public Unit Width
    {
        get
        {
            return this._Width;
        }
        set
        {
            this._Width = value;
        }
    }

    private Color _BackColor = ColorTranslator.FromHtml(DefaultValues_BackColor);
    /// <summary>
    /// Gets or sets the background color of the image upload control.</summary>
    [DefaultValue(typeof(Color), DefaultValues_BackColor)]
    [TypeConverter(typeof(WebColorConverter))]
    public Color BackColor
    {
        get
        {
            return this._BackColor;
        }
        set
        {
            this._BackColor = value;
        }
    }

    private Color _BorderColor = ColorTranslator.FromHtml(DefaultValues_BorderColor);
    /// <summary>
    /// Gets or sets the border color of the image upload control.</summary>
    [DefaultValue(typeof(Color), DefaultValues_BorderColor)]
    [TypeConverter(typeof(WebColorConverter))]
    public Color BorderColor
    {
        get
        {
            return this._BorderColor;
        }
        set
        {
            this._BorderColor = value;
        }
    }

    private BorderStyle _BorderStyle = DefaultValues_BorderStyle;
    /// <summary>
    /// Gets or sets the border style of the image upload control.</summary>
    public BorderStyle BorderStyle
    {
        get
        {
            return this._BorderStyle;
        }
        set
        {
            this._BorderStyle = value;
        }
    }

    private Unit _BorderWidth = DefaultValues_BorderWidth;
    /// <summary>
    /// Gets or sets the border width of the image upload control.</summary>
    [DefaultValue(typeof(Unit), "1px")]
    public Unit BorderWidth
    {
        get
        {
            return this._BorderWidth;
        }
        set
        {
            this._BorderWidth = value;
        }
    }

    private Color _ContentBackColor = ColorTranslator.FromHtml(DefaultValues_ContentBackColor);
    /// <summary>
    /// Gets or sets the background color of the content element.</summary>
    [DefaultValue(typeof(Color), DefaultValues_ContentBackColor)]
    [TypeConverter(typeof(WebColorConverter))]
    public Color ContentBackColor
    {
        get
        {
            return this._ContentBackColor;
        }
        set
        {
            this._ContentBackColor = value;
        }
    }

    private Color _ContentForeColor = ColorTranslator.FromHtml(DefaultValues_ContentForeColor);
    /// <summary>
    /// Gets or sets the foreground color of the content element.</summary>
    [DefaultValue(typeof(Color), DefaultValues_ContentForeColor)]
    [TypeConverter(typeof(WebColorConverter))]
    public Color ContentForeColor
    {
        get
        {
            return this._ContentForeColor;
        }
        set
        {
            this._ContentForeColor = value;
        }
    }

    private Color _ContentErrorForeColor = ColorTranslator.FromHtml(DefaultValues_ContentErrorForeColor);
    /// <summary>
    /// Gets or sets the foreground color of the content element when an error is displayed.</summary>
    [DefaultValue(typeof(Color), DefaultValues_ContentErrorForeColor)]
    [TypeConverter(typeof(WebColorConverter))]
    public Color ContentErrorForeColor
    {
        get
        {
            return this._ContentErrorForeColor;
        }
        set
        {
            this._ContentErrorForeColor = value;
        }
    }

    private Color _ContentBorderColor = ColorTranslator.FromHtml(DefaultValues_ContentBorderColor);
    /// <summary>
    /// Gets or sets the border color of the content element.</summary>
    [DefaultValue(typeof(Color), DefaultValues_ContentBorderColor)]
    [TypeConverter(typeof(WebColorConverter))]
    public Color ContentBorderColor
    {
        get
        {
            return this._ContentBorderColor;
        }
        set
        {
            this._ContentBorderColor = value;
        }
    }

    private BorderStyle _ContentBorderStyle = DefaultValues_ContentBorderStyle;
    /// <summary>
    /// Gets or sets the border style of the content element.</summary>
    public BorderStyle ContentBorderStyle
    {
        get
        {
            return this._ContentBorderStyle;
        }
        set
        {
            this._ContentBorderStyle = value;
        }
    }

    private Unit _ContentBorderWidth = DefaultValues_ContentBorderWidth;
    /// <summary>
    /// Gets or sets the border width of the content element.</summary>
    [DefaultValue(typeof(Unit), "1px")]
    public Unit ContentBorderWidth
    {
        get
        {
            return this._ContentBorderWidth;
        }
        set
        {
            this._ContentBorderWidth = value;
        }
    }

    private Color _PreviewBorderColor = ColorTranslator.FromHtml(DefaultValues_PreviewBorderColor);
    /// <summary>
    /// Gets or sets the border color of the preview image element.</summary>
    [DefaultValue(typeof(Color), DefaultValues_PreviewBorderColor)]
    [TypeConverter(typeof(WebColorConverter))]
    public Color PreviewBorderColor
    {
        get
        {
            return this._PreviewBorderColor;
        }
        set
        {
            this._PreviewBorderColor = value;
        }
    }

    private BorderStyle _PreviewBorderStyle = DefaultValues_PreviewBorderStyle;
    /// <summary>
    /// Gets or sets the border style of the preview image element.</summary>
    public BorderStyle PreviewBorderStyle
    {
        get
        {
            return this._PreviewBorderStyle;
        }
        set
        {
            this._PreviewBorderStyle = value;
        }
    }

    private Unit _PreviewBorderWidth = DefaultValues_PreviewBorderWidth;
    /// <summary>
    /// Gets or sets the border width of the preview image element.</summary>
    [DefaultValue(typeof(Unit), "1px")]
    public Unit PreviewBorderWidth
    {
        get
        {
            return this._PreviewBorderWidth;
        }
        set
        {
            this._PreviewBorderWidth = value;
        }
    }

    #endregion

    #region Misc

    protected float _OutputResolution = CommonData.DefaultResolution;
    /// <summary>
    /// Gets or sets the output resolution (DPI - default value = 96).</summary>
    public float OutputResolution
    {
        get
        {
            return this._OutputResolution;
        }
        set
        {
            if (this.HasImage)
            {
                throw new Exception("Cannot change the OutputResolution after an image has been loaded.");
            }

            // Validate the new resolution
            CodeCarvings.Piczard.Helpers.ImageHelper.ValidateResolution(value, true);

            this._OutputResolution = value;
        }
    }

    protected CropConstraint _CropConstraint = null;
    /// <summary>
    /// Gets or sets the crop constraint.</summary>
    public CropConstraint CropConstraint
    {
        get
        {
            return this._CropConstraint;
        }
        set
        {
            if (this.HasImage)
            {
                throw new Exception("Cannot change the CropConstraint after an image has been loaded.");
            }
            this._CropConstraint = value;
        }
    }

    protected ImageProcessingFilter _ImageUploadPreProcessingFilter = null;
    /// <summary>
    /// Gets or sets the filter(s) to apply after a new upload, before the image is loaded into the control.</summary>
    public ImageProcessingFilter ImageUploadPreProcessingFilter
    {
        get
        {
            return this._ImageUploadPreProcessingFilter;
        }
        set
        {
            this._ImageUploadPreProcessingFilter = value;
        }
    }

    protected ImageProcessingFilter _PostProcessingFilter = null;
    /// <summary>
    /// Gets or sets the filter(s) to apply to the image.</summary>
    public ImageProcessingFilter PostProcessingFilter
    {
        get
        {
            return this._PostProcessingFilter;
        }
        set
        {
            if (this.HasImage)
            {
                throw new Exception("Cannot change the PostProcessingFilter after an image has been loaded.");
            }
            this._PostProcessingFilter = value;
        }
    }

    protected SimpleImageUploadPostProcessingFilterApplyMode _PostProcessingFilterApplyMode = SimpleImageUploadPostProcessingFilterApplyMode.OnlyNewImages;
    /// <summary>
    /// Gets or sets a value indicating if and when ImageProcessingFilter must be applied.
    /// Default values is: "OnlyNewImages".</summary>
    public SimpleImageUploadPostProcessingFilterApplyMode PostProcessingFilterApplyMode
    {
        get
        {
            return this._PostProcessingFilterApplyMode;
        }
        set
        {
            if (this.HasImage)
            {
                throw new Exception("Cannot change PostProcessingFilterApplyMode after an image has been loaded.");
            }
            this._PostProcessingFilterApplyMode = value;
        }
    }

    protected ImageProcessingFilter _PreviewFilter = null;
    /// <summary>
    /// Gets or sets the filter(s) to apply to the preview image.</summary>
    public ImageProcessingFilter PreviewFilter
    {
        get
        {
            return this._PreviewFilter;
        }
        set
        {
            if (this.HasImage)
            {
                throw new Exception("Cannot change the PreviewFilter after an image has been loaded.");
            }

            this._PreviewFilter = value;
        }
    }
  
    /// <summary>
    /// Gets or sets the ResizeConstraint used to generate the preview image.
    /// This property is marked as obsolete since version 2.0.0 of the control and will be soon removed.
    /// Use 'PreviewFilter' instead.</summary>
    [Obsolete]
    public ResizeConstraint PreviewResizeConstraint
    {
        get
        {
            return (ResizeConstraint)this.PreviewFilter;
        }
        set
        {
            this.PreviewFilter = value;
        }
    }

    protected Size _ImageEditPopupSize = DefaultImageEditPopupSize;
    /// <summary>
    /// Gets or sets the image edit popup size.</summary>
    public Size ImageEditPopupSize
    {
        get
        {
            return this._ImageEditPopupSize;
        }
        set
        {
            this._ImageEditPopupSize = value;
        }
    }

    protected bool _AutoOpenImageEditPopupAfterUpload = false;
    /// <summary>
    /// Gets or sets a value indicating whether to automatically open the image edit popup after the upload process.</summary>
    public bool AutoOpenImageEditPopupAfterUpload
    {
        get
        {
            return this._AutoOpenImageEditPopupAfterUpload;
        }
        set
        {
            this._AutoOpenImageEditPopupAfterUpload = value;
        }
    }

    protected bool _AutoDisableImageEdit = true;
    /// <summary>
    /// Gets or sets a value indicating whether to automatically disable image edit feature if not available (e.g. Flash Player not installed).</summary>
    public bool AutoDisableImageEdit
    {
        get
        {
            return this._AutoDisableImageEdit;
        }
        set
        {
            this._AutoDisableImageEdit = value;
        }
    }

    protected Size _ButtonSize = DefaultButtonSize;
    /// <summary>
    /// Gets or sets the size of the buttons.</summary>
    public Size ButtonSize
    {
        get
        {
            return this._ButtonSize;
        }
        set
        {
            this._ButtonSize = value;
        }
    }

    protected string _CssClass = string.Empty;
    /// <summary>
    /// Gets or sets the Cascading Style Sheet (CSS) class rendered by the user control on the client.</summary>
    public string CssClass
    {
        get
        {
            return this._CssClass;
        }
        set
        {
            this._CssClass = value;
        }
    }

    protected string[] _Configurations = null;
    /// <summary>
    /// Gets or sets the available configuration names.</summary>
    public string[] Configurations
    {
        get
        {
            return this._Configurations;
        }
        set
        {
            this._Configurations = value;
        }
    }

    protected int? _SelectedConfigurationIndex = null;
    /// <summary>
    /// Gets or sets the index of the selected configuration.</summary>
    public int? SelectedConfigurationIndex
    {
        get
        {
            if (this._Configurations == null)
            {
                // No configuration available
                return null;
            }
            if (this._Configurations.Length == 0)
            {
                // No configuration available
                return null;
            }

            if (!this._SelectedConfigurationIndex.HasValue)
            {
                // First configuration selected by default
                return 0;
            }
            else
            {
                if (this._SelectedConfigurationIndex.Value >= this._Configurations.Length)
                {
                    // Use the last configuration available
                    return this._Configurations.Length - 1;
                }
            }

            return this._SelectedConfigurationIndex;
        }
        set
        {
            if (value.HasValue)
            {
                if (this._Configurations == null)
                {
                    // No configuration available
                    throw new Exception("Cannot set the SelectedConfigurationIndex because no configuration has been set yet.");
                }
                if (this._Configurations.Length == 0)
                {
                    // No configuration available
                    throw new Exception("Cannot set the SelectedConfigurationIndex because there is no configuration set.");
                }

                if (value.Value < 0)
                {
                    throw new Exception("SelectedConfigurationIndex cannot be < 0.");
                }
                if (value.Value >= this._Configurations.Length)
                {
                    throw new Exception("SelectedConfigurationIndex must be < Configurations.length.");
                }
            }

            this._SelectedConfigurationIndex = value;
        }
    }

    protected bool _DebugUploadProblems = false;
    /// <summary>
    /// Gets or sets a value indicating whether to show details when an upload error occurs.</summary>
    public bool DebugUploadProblems
    {
        get
        {
            return this._DebugUploadProblems;
        }
        set
        {
            this._DebugUploadProblems = value;
        }
    }

    protected PopupPictureTrimmerSettingsProvider _PictureTrimmerSettings = null;
    /// <summary>
    /// Gets an object that allows to customize settings of the PopupPictureTrimmer instance.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [NotifyParentProperty(true)]
    public PopupPictureTrimmerSettingsProvider PictureTrimmerSettings
    {
        get
        {
            if (this._PictureTrimmerSettings == null)
            {
                this._PictureTrimmerSettings = new PopupPictureTrimmerSettingsProvider(this.popupPictureTrimmer1);
            }
            return this._PictureTrimmerSettings;
        }
    }

    #endregion

    #region Globalization

    #region UI elements

    protected string _Text_EditButton = "Edit...";
    /// <summary>
    /// Gets or sets the text of the "Edit" button.</summary>
    public string Text_EditButton
    {
        get
        {
            return this._Text_EditButton;
        }
        set
        {
            this._Text_EditButton = value;
        }
    }

    protected string _Text_RemoveButton = "Remove";
    /// <summary>
    /// Gets or sets the text of the "Remove" button.</summary>
    public string Text_RemoveButton
    {
        get
        {
            return this._Text_RemoveButton;
        }
        set
        {
            this._Text_RemoveButton = value;
        }
    }

    protected string _Text_BrowseButton = "Browse...";
    /// <summary>
    /// Gets or sets the text of the "Browse" button.</summary>
    public string Text_BrowseButton
    {
        get
        {
            return this._Text_BrowseButton;
        }
        set
        {
            this._Text_BrowseButton = value;
        }
    }

    protected string _Text_CancelUploadButton = "Cancel upload";
    /// <summary>
    /// Gets or sets the text of the "Cancel Upload" button.</summary>
    public string Text_CancelUploadButton
    {
        get
        {
            return this._Text_CancelUploadButton;
        }
        set
        {
            this._Text_CancelUploadButton = value;
        }
    }

    protected string _Text_ConfigurationLabel = "Configuration:";
    /// <summary>
    /// Gets or sets the text of the "Configuration" label.</summary>
    public string Text_ConfigurationLabel
    {
        get
        {
            return this._Text_ConfigurationLabel;
        }
        set
        {
            this._Text_ConfigurationLabel = value;
        }
    }

    #endregion

    #region Status messages

    protected string _CurrentStatusMessage = null;
    protected string GetCurrentStatusMessage()
    {
        if (this._CurrentStatusMessage != null)
        {
            // Last status set
            return this._CurrentStatusMessage;
        }

        // By default return the "No image selected" text.
        return this.StatusMessage_NoImageSelected;
    }
    /// <summary>
    /// Sets the current status message.</summary>
    /// <param name="text">The message to display.</param>
    /// <param name="isError">If true, the message will be displayed as error message.</param>
    public void SetCurrentStatusMessage(string text, bool isError)
    {
        if (isError)
        {
            if (this._ContentErrorForeColor != Color.Empty)
            {
                text = "<span style=\"color:" + ColorTranslator.ToHtml(this._ContentErrorForeColor) + ";\">" + text + "</span>";
            }
        }
        this._CurrentStatusMessage = text;
    }
    /// <summary>
    /// Sets the current status message.</summary>
    /// <param name="text">The message to display.</param>
    public void SetCurrentStatusMessage(string text)
    {
        this.SetCurrentStatusMessage(text, false);
    }

    protected string _StatusMessage_NoImageSelected = "No image selected.";
    /// <summary>
    /// Gets or sets the text displayed when no image has been selected.</summary>
    public string StatusMessage_NoImageSelected
    {
        get
        {
            return this._StatusMessage_NoImageSelected;
        }
        set
        {
            this._StatusMessage_NoImageSelected = value;
        }
    }

    protected string _StatusMessage_UploadError = "A server error has occurred during the upload process.<br/>Please ensure that the file is smaller than {0} KBytes.";
    /// <summary>
    /// Gets or sets the text displayed when a (generic) upload error has occurred.</summary>
    public string StatusMessage_UploadError
    {
        get
        {
            return string.Format(this._StatusMessage_UploadError, this.MaxRequestLength);
        }
        set
        {
            this._StatusMessage_UploadError = value;
        }
    }

    protected string _StatusMessage_InvalidImage = "The uploaded file is not a valid image.";
    /// <summary>
    /// Gets or sets the text displayed when the uploaded image file is invalid.</summary>
    public string StatusMessage_InvalidImage
    {
        get
        {
            return this._StatusMessage_InvalidImage;
        }
        set
        {
            this._StatusMessage_InvalidImage = value;
        }
    }

    protected string _StatusMessage_InvalidImageSize = "The uploaded image is not valid (too small or too large).";
    /// <summary>
    /// Gets or sets the text displayed when the size of the uploaded image is too small or too large (please see: CodeCarvings.Piczard.Configuration.DrawingSettings.MaxImageSize).</summary>
    public string StatusMessage_InvalidImageSize
    {
        get
        {
            return this._StatusMessage_InvalidImageSize;
        }
        set
        {
            this._StatusMessage_InvalidImageSize = value;
        }
    }

    protected string _StatusMessage_Wait = "Please wait...";
    /// <summary>
    /// Gets or sets the text displayed when the user has to wait (e.g. a postback has been delayed).</summary>
    public string StatusMessage_Wait
    {
        get
        {
            return this._StatusMessage_Wait;
        }
        set
        {
            this._StatusMessage_Wait = value;
        }
    }

    #endregion

    #endregion

    #region Features

    protected bool _EnableEdit = true;
    /// <summary>
    /// Gets or sets a value indicating whether it's possible to edit the image.</summary>
    public bool EnableEdit
    {
        get
        {
            return this._EnableEdit;
        }
        set
        {
            this._EnableEdit = value;
        }
    }

    protected bool _EnableRemove = true;
    /// <summary>
    /// Gets or sets a value indicating whether it's possible to remove the image.</summary>
    public bool EnableRemove
    {
        get
        {
            return this._EnableRemove;
        }
        set
        {
            this._EnableRemove = value;
        }
    }

    protected bool _EnableUpload = true;
    /// <summary>
    /// Gets or sets a value indicating whether it's possible to upload an image.</summary>
    public bool EnableUpload
    {
        get
        {
            return this._EnableUpload;
        }
        set
        {
            this._EnableUpload = value;
        }
    }

    protected bool _EnableCancelUpload = true;
    /// <summary>
    /// Gets or sets a value indicating whether it's possible to cancel an upload in progess.</summary>
    public bool EnableCancelUpload
    {
        get
        {
            return this._EnableCancelUpload;
        }
        set
        {
            this._EnableCancelUpload = value;
        }
    }
    
    #endregion

    #endregion

    #region Current state

    protected string _TemporaryFileId = null;
    /// <summary>
    /// Gets or sets the current temporary file id.</summary>
    protected string TemporaryFileId
    {
        get
        {
            if (this._TemporaryFileId == null)
            {
                // Get e new temporary file id
                this._TemporaryFileId = TemporaryFileManager.GetNewTemporaryFileId();
            }

            return this._TemporaryFileId;
        }
        set
        {
            // Validate the value
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception("Invalid TemporaryFileId (null).");
            }
            if (!TemporaryFileManager.ValidateTemporaryFileId(value, false))
            {
                throw new Exception("Invalid TemporaryFileId.");
            }

            this._TemporaryFileId = value;
        }
    }

    protected string _RenderId = null;
    protected string RenderId
    {
        get
        {
            if (this._RenderId == null)
            {
                this._RenderId = Guid.NewGuid().ToString("N");
            }
            return this._RenderId;
        }
    }

    protected string InitFunctionName
    {
        get
        {
            return "CCPZ_SIU_" + this.RenderId + "_Init";
        }
    }

    protected string InitFunctionName2
    {
        get
        {
            return "CCPZ_SIU_" + this.RenderId + "_Init2";
        }
    }

    // If true, the control must regenerate the preview image (in the prerender method)
    protected bool _UpdatePreview = false;
    protected string _LastAct = "";
    protected int? _UploadMonitorStatus = null;
    protected bool _AutoOpenImageEditPopup = false;

    /// <summary>
    /// Gets a value indicating whether the control has an image.</summary>
    public bool HasImage
    {
        get
        {
            return this.popupPictureTrimmer1.ImageLoaded;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the control has an image uploaded or edited by the user.</summary>
    public bool HasNewImage
    {
        get
        {
            if (this.HasImage)
            {
                if (this.ImageUploaded)
                {
                    return true;
                }

                if (this.ImageEdited)
                {
                    return true;
                }
            }

            // Image not loaded or already persisted
            return false;
        }
    }

    protected bool _ImageUploaded = false;
    /// <summary>
    /// Gets a value indicating whether the control has an image uploaded by the user.</summary>
    public bool ImageUploaded
    {
        get
        {
            if (!this.HasImage)
            {
                return false;
            }

            return this._ImageUploaded;
        }
    }

    protected bool _ImageEdited = false;
    /// <summary>
    /// Gets a value indicating whether the control has an image edited by the user.</summary>
    public bool ImageEdited
    {
        get
        {
            if (!this.HasImage)
            {
                return false;
            }

            return this._ImageEdited;
        }
    }

    protected string _SourceImageClientFileName = null;
    /// <summary>
    /// Gets or sets the original image file name (before upload).</summary>
    public string SourceImageClientFileName
    {
        get
        {
            return this._SourceImageClientFileName;
        }
        set
        {
            if (value != null)
            {
                // Get the file name
                value = Path.GetFileName(value);
            }
            this._SourceImageClientFileName = value;
        }
    }

    #endregion

    #region PictureTrimmer properties

    /// <summary>
    /// Gets the current PictureTrimmerUserState of the PictureTrimmer control.</summary>
    public PictureTrimmerUserState UserState
    {
        get
        {
            if (!this.HasImage)
            {
                return null;
            }

            return this.popupPictureTrimmer1.UserState;
        }
    }

    /// <summary>
    /// Gets the current PictureTrimmerValue of the PictureTrimmer control.</summary>
    public PictureTrimmerValue Value
    {
        get
        {
            if (!this.HasImage)
            {
                return null;
            }

            return this.popupPictureTrimmer1.Value;
        }
    }

    /// <summary>
    /// Gets or sets the current culture.</summary>
    public string Culture
    {
        get
        {
            return this.popupPictureTrimmer1.Culture;
        }
        set
        {
            this.popupPictureTrimmer1.Culture = value;
        }
    }

    /// <summary>
    /// Gets or sets the CanvasColor used by the PictureTrimmer control.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [NotifyParentProperty(true)]
    public BackgroundColor CanvasColor
    {
        get
        {
            return this.popupPictureTrimmer1.CanvasColor;
        }
        set
        {
            this.popupPictureTrimmer1.CanvasColor = value;
        }
    }

    /// <summary>
    /// Gets or sets the ImageBackColor used by the PictureTrimmer control.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [NotifyParentProperty(true)]
    public BackgroundColor ImageBackColor
    {
        get
        {
            return this.popupPictureTrimmer1.ImageBackColor;
        }
        set
        {
            this.popupPictureTrimmer1.ImageBackColor = value;
        }
    }

    /// <summary>
    /// Gets or sets the ImageBackColorApplyMode property of the PictureTrimmer control.</summary>
    public PictureTrimmerImageBackColorApplyMode ImageBackColorApplyMode
    {
        get
        {
            return this.popupPictureTrimmer1.ImageBackColorApplyMode;
        }
        set
        {
            this.popupPictureTrimmer1.ImageBackColorApplyMode = value;
        }
    }

    /// <summary>
    /// Gets or sets the UIUnit used by the PictureTrimmer control.</summary>
    public GfxUnit UIUnit
    {
        get
        {
            return this.popupPictureTrimmer1.UIUnit;
        }
        set
        {
            this.popupPictureTrimmer1.UIUnit = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating how the PictureTrimmer GUI renders the cropping mask.
    /// This property is marked as obsolete since version 3.0.0 of the control and will be soon removed.
    /// Use 'PictureTrimmerSettings-CropShadowMode' instead.</summary>
    [Obsolete]
    public PictureTrimmerCropShadowMode CropShadowMode
    {
        get
        {
            return this.popupPictureTrimmer1.CropShadowMode;
        }
        set
        {
            this.popupPictureTrimmer1.CropShadowMode = value;
        }
    }    

    #region Read-only properties

    /// <summary>
    /// Gets the size (pixel) of the source image.</summary>
    public Size SourceImageSize
    {
        get
        {
            if (!this.HasImage)
            {
                throw new Exception("Image not loaded.");
            }

            return this.popupPictureTrimmer1.SourceImageSize;
        }
    }

    /// <summary>
    /// Gets the resolution (DPI) of the source image.</summary>
    public float SourceImageResolution
    {
        get
        {
            if (!this.HasImage)
            {
                throw new Exception("Image not loaded.");
            }
            return this.popupPictureTrimmer1.SourceImageResolution;
        }
    }

    /// <summary>
    /// Gets the format of the source image.</summary>
    public Guid SourceImageFormatId
    {
        get
        {
            if (!this.HasImage)
            {
                throw new Exception("Image not loaded.");
            }
            return this.popupPictureTrimmer1.SourceImageFormatId;
        }
    }

    #endregion

    #endregion

    #region File paths / urls

    /// <summary>
    /// Gets the path of the temporary file containing the source image.</summary>
    public string TemporarySourceImageFilePath
    {
        get
        {
            return TemporaryFileManager.GetTemporaryFilePath(this.TemporaryFileId, "_s.tmp");
        }
    }

    protected string UploadFilePath
    {
        get
        {
            return TemporaryFileManager.GetTemporaryFilePath(this.TemporaryFileId, "_u.tmp");
        }
    }

    protected string PreviewImageFilePath
    {
        get
        {
            return TemporaryFileManager.GetTemporaryFilePath(this.TemporaryFileId, "_p.jpg");
        }
    }

    protected string TemporaryWriteTestFilePath
    {
        get
        {
            return TemporaryFileManager.GetTemporaryFilePath(this.TemporaryFileId, "_wt.tmp");
        }
    }

    protected string UploadMonitorFilePath
    {
        get
        {
            return TemporaryFileManager.GetTemporaryFilePath(this.TemporaryFileId, "_um.xml");
        }
    }

    protected string PreviewImageUrl
    {
        get
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(this.ResolveUrl("preview.ashx"));
            sb.Append("?tfid=" + HttpUtility.UrlEncode(this.TemporaryFileId.ToString()));
            sb.Append("&k=" + HttpUtility.UrlEncode(this.GetQueryKey("")));
            sb.Append("&ts=" + HttpUtility.UrlEncode(DateTime.UtcNow.Ticks.ToString()));

            return sb.ToString();
        }
    }

    protected string UploadUrl
    {
        get
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(this.ResolveUrl("upload.aspx"));
            sb.Append("?tfid=" + HttpUtility.UrlEncode(this.TemporaryFileId.ToString()));
            string keyAdditionalData = "dup=" + (this._DebugUploadProblems ? "1" : "0");
            sb.Append("&" + keyAdditionalData);
            sb.Append("&k=" + HttpUtility.UrlEncode(this.GetQueryKey(keyAdditionalData)));
            sb.Append("&cid=" + HttpUtility.UrlEncode(this.ClientID));
            sb.Append("&bsw=" + HttpUtility.UrlEncode(this.ButtonSize.Width.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sb.Append("&bsh=" + HttpUtility.UrlEncode(this.ButtonSize.Height.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sb.Append("&ts=" + HttpUtility.UrlEncode(DateTime.UtcNow.Ticks.ToString()));

            return sb.ToString();
        }
    }

    protected string UploadMonitorUrl
    {
        get
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(this.ResolveUrl("uploadMonitor.ashx"));
            sb.Append("?tfid=" + HttpUtility.UrlEncode(this.TemporaryFileId.ToString()));
            sb.Append("&k=" + HttpUtility.UrlEncode(this.GetQueryKey("")));

            return sb.ToString();
        }
    }

    protected string MaxRequestLength
    {
        get
        {
            // Default value = 4 MB
            string result = "4096";

            try
            {
                System.Web.Configuration.HttpRuntimeSection section = (System.Web.Configuration.HttpRuntimeSection)System.Configuration.ConfigurationManager.GetSection("system.web/httpRuntime");
                if (section != null)
                {
                    result = section.MaxRequestLength.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                // An error has occurred (Medium trust ?)
                result = "?";
            }

            return result;
        }
    }

    #endregion

    #endregion

    #region Events

    /// <summary>
    /// Represents the method that handles the ImageUpload event.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The argument of type ImageUploadEventArgs that contains the event data.</param>
    public delegate void ImageUploadEventHandler(object sender, ImageUploadEventArgs args);

    /// <summary>
    /// Occurs when a new image is uploaded.</summary>
    public event ImageUploadEventHandler ImageUpload = null;
    protected void OnImageUpload(ImageUploadEventArgs e)
    {
        if (this.ImageUpload != null)
        {
            this.ImageUpload(this, e);
        }
    }

    /// <summary>
    /// Occurs when an upload process does not complete successfully.</summary>
    public event EventHandler UploadError = null;
    protected void OnUploadError(EventArgs e)
    {
        if (this.UploadError != null)
        {
            this.UploadError(this, e);
        }
    }

    /// <summary>
    /// Occurs when the image is edited.</summary>
    public event EventHandler ImageEdit = null;
    protected void OnImageEdit(EventArgs e)
    {
        if (this.ImageEdit != null)
        {
            this.ImageEdit(this, e);
        }
    }

    /// <summary>
    /// Occurs when the image is removed.</summary>
    public event EventHandler ImageRemove = null;
    protected void OnImageRemove(EventArgs e)
    {
        if (this.ImageRemove != null)
        {
            this.ImageRemove(this, e);
        }
    }

    /// <summary>
    /// Represents the method that handles the SelectedConfigurationIndexChanged event.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The argument of type SelectedConfigurationIndexChangedEventArgs that contains the event data.</param>
    public delegate void SelectedConfigurationIndexChangedEventHandler(object sender, SelectedConfigurationIndexChangedEventArgs args);

    /// <summary>
    /// Occurs when the ConfigurationIndex has been changed by the user.</summary>
    public event SelectedConfigurationIndexChangedEventHandler SelectedConfigurationIndexChanged = null;
    protected void OnSelectedConfigurationIndexChanged(SelectedConfigurationIndexChangedEventArgs e)
    {
        if (this.SelectedConfigurationIndexChanged != null)
        {
            this.SelectedConfigurationIndexChanged(this, e);
        }
    }

    #endregion

    #region Methods

    #region Load

    protected void LoadImageFromFileSystem_Internal(string sourceImageFilePath, System.Drawing.Image sourceImage, Size sourceImageSize, Guid sourceImageFormatId, float sourceImageResolution, bool disposeSourceImage, PictureTrimmerValue value)
    {
        // Calculate the client file name
        this.SourceImageClientFileName = "noname" + ImageArchiver.GetFormatEncoderParams(sourceImageFormatId).FileExtension;

        if (CodeCarvings.Piczard.Configuration.WebSettings.PictureTrimmer.UseTemporaryFiles)
        {
            // The picture trimmer can use temporary files -> Load the image now
            // This generates a new temporary files, however saves CPU and RAM
            this.popupPictureTrimmer1.LoadImage(sourceImage, this._OutputResolution, this._CropConstraint);
        }
        if (disposeSourceImage)
        {
            // The source image is no longer necessary
            sourceImage.Dispose();
            sourceImage = null;
        }

        if (!CodeCarvings.Piczard.Configuration.WebSettings.PictureTrimmer.UseTemporaryFiles)
        {
            // The picture trimmer cannot use temporary files -> Load the image now
            this.popupPictureTrimmer1.SetLoadImageData_ImageSize(sourceImageSize);
            this.popupPictureTrimmer1.SetLoadImageData_ImageResolution(sourceImageResolution);
            this.popupPictureTrimmer1.SetLoadImageData_ImageFormatId(sourceImageFormatId);

            this.popupPictureTrimmer1.LoadImageFromFileSystem(sourceImageFilePath, this._OutputResolution, this._CropConstraint);
        }

        if (value != null)
        {
            // Optional: Set the picture trimmer value
            this.popupPictureTrimmer1.Value = value;
        }

        // The new image has been loaded
        this._ImageUploaded = false;
        this._ImageEdited = false;

        // Update the preview
        this._UpdatePreview = true;
    }

    protected void LoadImageFromFileSystem_Internal(string sourceImageFilePath, PictureTrimmerValue value)
    {
        using (LoadedImage image = ImageArchiver.LoadImage(sourceImageFilePath))
        {
            this.LoadImageFromFileSystem_Internal(sourceImageFilePath, image.Image, image.Size, image.FormatId, image.Resolution, true, value);
        }
    }

    /// <summary>
    /// Loads an image stored in the file system and applies a specific PictureTrimmerValue.</summary>
    /// <param name="sourceImageFilePath">The path of the image to load.</param>
    /// <param name="value">The PictureTrimmerValue to apply.</param>
    public void LoadImageFromFileSystem(string sourceImageFilePath, PictureTrimmerValue value)
    {
        // Translate path to absolute
        sourceImageFilePath = CodeCarvings.Piczard.Helpers.IOHelper.TranslatePathToAbsolute(sourceImageFilePath);

        // Copy the source image into the temporary folder
        // So there is no problem il the original source image is deleted (e.g. when a record is updated...)
        System.IO.File.Copy(sourceImageFilePath, this.TemporarySourceImageFilePath, true);

        // Load the image
        this.LoadImageFromFileSystem_Internal(this.TemporarySourceImageFilePath, value);

        // Use the original file name as source client file name
        this.SourceImageClientFileName = sourceImageFilePath;
    }

    /// <summary>
    /// Loads an image stored in the file system and auto-calculates the PictureTrimmerValue to use.</summary>
    /// <param name="sourceImageFilePath">The path of the image to load.</param>
    public void LoadImageFromFileSystem(string sourceImageFilePath)
    {
        this.LoadImageFromFileSystem(sourceImageFilePath, null);
    }

    /// <summary>
    /// Loads an image from a Stream and applies a specific PictureTrimmerValue.</summary>
    /// <param name="sourceImageStream">The Stream containing the image to load.</param>
    /// <param name="value">The PictureTrimmerValue to apply.</param>
    public void LoadImageFromStream(Stream sourceImageStream, PictureTrimmerValue value)
    {
        // Save the stream
        using (Stream writer = File.Create(this.TemporarySourceImageFilePath))
        {
            if (sourceImageStream.Position != 0)
            {
                sourceImageStream.Seek(0, SeekOrigin.Begin);
            }

            byte[] buffer = new byte[4096];
            int readBytes;
            while (true)
            {
                readBytes = sourceImageStream.Read(buffer, 0, buffer.Length);
                if (readBytes <= 0)
                {
                    break;
                }
                writer.Write(buffer, 0, readBytes);
            }

            writer.Close();
        }

        // Load the image from the temporary file
        this.LoadImageFromFileSystem_Internal(this.TemporarySourceImageFilePath, value);
    }

    /// <summary>
    /// Loads an image from a Stream and auto-calculates the PictureTrimmerValue to use.</summary>
    /// <param name="sourceImageStream">The Stream containing the image to load.</param>
    public void LoadImageFromStream(Stream sourceImageStream)
    {
        this.LoadImageFromStream(sourceImageStream, null);
    }

    /// <summary>
    /// Loads an image from an array of bytes and applies a specific PictureTrimmerValue.</summary>
    /// <param name="sourceImageBytes">The array of bytes to load.</param>
    /// <param name="value">The PictureTrimmerValue to apply.</param>
    public void LoadImageFromByteArray(byte[] sourceImageBytes, PictureTrimmerValue value)
    {
        //Save the byte array
        using (Stream writer = File.Create(this.TemporarySourceImageFilePath))
        {
            writer.Write(sourceImageBytes, 0, sourceImageBytes.Length);
            writer.Close();
        }

        // Load the image from the temporary file
        this.LoadImageFromFileSystem_Internal(this.TemporarySourceImageFilePath, value);
    }

    /// <summary>
    /// Loads an image from an array of bytes and auto-calculates the PictureTrimmerValue to use.</summary>
    /// <param name="sourceImageBytes">The array of bytes to load.</param>
    public void LoadImageFromByteArray(byte[] sourceImageBytes)
    {
        this.LoadImageFromByteArray(sourceImageBytes, null);
    }

    /// <summary>
    /// Loads an image from an array of bytes and applies a specific PictureTrimmerValue.</summary>
    /// <param name="sourceImage">The source image to load.</param>
    /// <param name="value">The PictureTrimmerValue to apply.</param>
    public void LoadImage(LoadedImage sourceImage, PictureTrimmerValue value)
    {
        // Save the image - Use PNG as image format to preserve transparency
        ImageArchiver.SaveImageToFileSystem(sourceImage.Image, this.TemporarySourceImageFilePath, new PngFormatEncoderParams());

        // Load the image into the control
        this.LoadImageFromFileSystem_Internal(this.TemporarySourceImageFilePath, sourceImage.Image, sourceImage.Size, sourceImage.FormatId, sourceImage.Resolution, false, value);
    }

    /// <summary>
    /// Loads an image and auto-calculates the PictureTrimmerValue to use.</summary>
    /// <param name="sourceImage">The source image to load.</param>
    public void LoadImage(LoadedImage sourceImage)
    {
        this.LoadImage(sourceImage, null);
    }

    /// <summary>
    /// Loads an image from an array of bytes and applies a specific PictureTrimmerValue.</summary>
    /// <param name="sourceImage">The source image to load.</param>
    /// <param name="value">The PictureTrimmerValue to apply.</param>
    public void LoadImage(System.Drawing.Image sourceImage, PictureTrimmerValue value)
    {
        // Save the image - Use PNG as image format to preserve transparency
        ImageArchiver.SaveImageToFileSystem(sourceImage, this.TemporarySourceImageFilePath, new PngFormatEncoderParams());

        // Load the image into the control
        this.LoadImageFromFileSystem_Internal(this.TemporarySourceImageFilePath, sourceImage, sourceImage.Size, sourceImage.RawFormat.Guid, CodeCarvings.Piczard.Helpers.ImageHelper.GetImageResolution(sourceImage), false, value);
    }

    /// <summary>
    /// Loads an image and auto-calculates the PictureTrimmerValue to use.</summary>
    /// <param name="sourceImage">The source image to load.</param>
    public void LoadImage(System.Drawing.Image sourceImage)
    {
        this.LoadImage(sourceImage, null);
    }

    /// <summary>
    /// Unloads the current image.</summary>
    /// <param name="clearTemporaryFiles">If true, delete the temporary files.</param>
    protected void UnloadImage(bool clearTemporaryFiles)
    {
        this.popupPictureTrimmer1.UnloadImage();

        if (clearTemporaryFiles)
        {
            // Delete the temporary files
            this.ClearTemporaryFiles();
        }

        // No image
        this._ImageUploaded = false;
        this._ImageEdited = false;
        this._SourceImageClientFileName = null;
    }

    /// <summary>
    /// Unloads the current image and clears the temporary files.</summary>
    public void UnloadImage()
    {
        this.UnloadImage(true);
    }

    #endregion

    #region Image Processing

    /// <summary>
    /// Returns the ImageProcessingJob that can be used to process the source image.</summary>
    /// <returns>An ImageProcessingJob ready to be used to process imagess.</returns>
    public ImageProcessingJob GetImageProcessingJob()
    {
        ImageProcessingJob result = this.popupPictureTrimmer1.GetImageProcessingJob();
        if (this.PostProcessingFilter != null)
        {
            // Apply the post processing filter(s) is necessary
            bool addPostProcessingFilter = true;
            switch (this._PostProcessingFilterApplyMode)
            {
                case SimpleImageUploadPostProcessingFilterApplyMode.Never:
                    addPostProcessingFilter = false;
                    break;
                case SimpleImageUploadPostProcessingFilterApplyMode.OnlyNewImages:
                    addPostProcessingFilter = this.HasNewImage;
                    break;
                case SimpleImageUploadPostProcessingFilterApplyMode.Always:
                    addPostProcessingFilter = true;
                    break;
            }            
            if (addPostProcessingFilter)
            {
                result.Filters.Add(this.PostProcessingFilter);
            }
        }
        return result;
    }

    /// <summary>
    /// Returns the output image processed by the control.
    /// BackgroundColor and quantization are applied according to the specified FormatEncoderParams.</summary>
    /// <param name="hintFormatEncoderParams">The image format that will be used then to save image.</param>
    /// <returns>A Bitmap image processed by the control.</returns>
    public Bitmap GetProcessedImage(FormatEncoderParams hintFormatEncoderParams)
    {
        ImageProcessingJob job = this.GetImageProcessingJob();
        return job.GetProcessedImage(this.TemporarySourceImageFilePath, hintFormatEncoderParams);
    }

    /// <summary>
    /// Returns the output image processed by the control.</summary>
    /// <returns>A Bitmap image processed by the control.</returns>
    public Bitmap GetProcessedImage()
    {
        ImageProcessingJob job = this.GetImageProcessingJob();
        return job.GetProcessedImage(this.TemporarySourceImageFilePath);
    }

    /// <summary>
    /// Processes  the source image and saves the output in a Stream with a specific image format.</summary>
    /// <param name="destStream">The Stream in which the image will be saved.</param>
    /// <param name="formatEncoderParams">The image format of the saved image.</param>
    public void SaveProcessedImageToStream(Stream destStream, FormatEncoderParams formatEncoderParams)
    {
        ImageProcessingJob job = this.GetImageProcessingJob();
        job.SaveProcessedImageToStream(this.TemporarySourceImageFilePath, destStream, formatEncoderParams);
    }

    /// <summary>
    /// Processes the source image and saves the output in a Stream with the default image format.</summary>
    /// <param name="destStream">The Stream in which the image will be saved.</param>
    public void SaveProcessedImageToStream(Stream destStream)
    {
        ImageProcessingJob job = this.GetImageProcessingJob();
        job.SaveProcessedImageToStream(this.TemporarySourceImageFilePath, destStream);
    }

    /// <summary>
    /// Processes the source image and saves the output in the file system with a specific image format.</summary>
    /// <param name="destFilePath">The file path of the saved image.</param>
    /// <param name="formatEncoderParams">The image format of the saved image.</param>
    public void SaveProcessedImageToFileSystem(string destFilePath, FormatEncoderParams formatEncoderParams)
    {
        ImageProcessingJob job = this.GetImageProcessingJob();
        job.SaveProcessedImageToFileSystem(this.TemporarySourceImageFilePath, destFilePath, formatEncoderParams);
    }

    /// <summary>
    /// Processes the source image and save the output in the file system with the default image format.</summary>
    /// <param name="destFilePath">The file path of the saved image.</param>
    public void SaveProcessedImageToFileSystem(string destFilePath)
    {
        ImageProcessingJob job = this.GetImageProcessingJob();
        job.SaveProcessedImageToFileSystem(this.TemporarySourceImageFilePath, destFilePath);
    }

    /// <summary>
    /// Processes the source image and returns a byte array containing the processed image encoded with a specific image format.</summary>
    /// <param name="formatEncoderParams">The image format of the saved image.</param>
    /// <returns>An array of bytes containing the processed image.</returns>
    public byte[] SaveProcessedImageToByteArray(FormatEncoderParams formatEncoderParams)
    {
        ImageProcessingJob job = this.GetImageProcessingJob();
        return job.SaveProcessedImageToByteArray(this.TemporarySourceImageFilePath, formatEncoderParams);
    }

    /// <summary>
    /// Processes the source image and returns a byte array containing the processed image encoded with the default image format.</summary>
    /// <returns>An array of bytes containing the processed image.</returns>
    public byte[] SaveProcessedImageToByteArray()
    {
        ImageProcessingJob job = this.GetImageProcessingJob();
        return job.SaveProcessedImageToByteArray(this.TemporarySourceImageFilePath);
    }

    #endregion

    #region Misc

    /// <summary>
    /// Deletes the internal temporary files generated by the control.</summary>
    public void ClearTemporaryFiles()
    {
        this.ClearTemporaryFile(this.TemporarySourceImageFilePath);
        this.ClearTemporaryFile(this.UploadFilePath);
        this.ClearTemporaryFile(this.PreviewImageFilePath);
        this.ClearTemporaryFile(this.TemporaryWriteTestFilePath);
        this.ClearTemporaryFile(this.UploadMonitorFilePath);
    }

    /// <summary>
    /// Opens the image edit popup window.</summary>
    public void OpenImageEditPopup()
    {
        if (!this.HasImage)
        {
            throw new Exception("Image not loaded.");
        }

        // Open the image edit popup
        this._AutoOpenImageEditPopup = true;
    }

    #endregion

    #region Protected

    protected void ClearTemporaryFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    protected string GetSubElementId(string subId)
    {
        return this.ClientID + "_" + subId;
    }

    protected void TemporaryFolderWriteTest()
    {
        // Check if the application can write on the temporary folder
        if (!File.Exists(this.TemporaryWriteTestFilePath))
        {
            File.WriteAllText(this.TemporaryWriteTestFilePath, "write test", System.Text.Encoding.UTF8);
        }
    }

    protected string GetRenderStyle_container0()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(this.GetRenderStyleValue("width", this._Width));
        sb.Append(this.GetRenderStyleValue("background-color", this._BackColor));
        sb.Append(this.GetRenderStyleValue("border-color", this._BorderColor));
        sb.Append(this.GetRenderStyleValue("border-style", this._BorderStyle));
        sb.Append(this.GetRenderStyleValue("border-width", this._BorderWidth));
        return sb.ToString();
    }

    protected string GetRenderStyle_content()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(this.GetRenderStyleValue("background-color", this._ContentBackColor));
        sb.Append(this.GetRenderStyleValue("color", this._ContentForeColor));
        sb.Append(this.GetRenderStyleValue("border-color", this._ContentBorderColor));
        sb.Append(this.GetRenderStyleValue("border-style", this._ContentBorderStyle));
        sb.Append(this.GetRenderStyleValue("border-width", this._ContentBorderWidth));
        return sb.ToString();
    }

    protected string GetRenderStyleValue(string name, Unit value)
    {
        if (!value.IsEmpty)
        {
            return name + ":" + value.ToString(System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant() + ";";
        }
 
        // Do not render the value
        return "";
    }

    protected string GetRenderStyleValue(string name, Color value)
    {
        if (!value.IsEmpty)
        {
            return name + ":" + ColorTranslator.ToHtml(value) + ";";
        }

        // Do not render the value
        return "";
    }

    protected string GetRenderStyleValue(string name, BorderStyle value)
    {
        if (value != BorderStyle.NotSet)
        {
            return name + ":" + value.ToString().ToLowerInvariant() + ";";
        }

        // Do not render the value
        return "";
    }

    protected string GetQueryKey(string additionalData)
    {
        Random rnd = new Random();
        string keyLeft = Guid.NewGuid().ToString("N").Substring(0, rnd.Next(1, 10));
        string keyRight = Guid.NewGuid().ToString("N").Substring(0, rnd.Next(1, 10));
        long timeStamp = DateTime.Now.Ticks + Convert.ToInt64(rnd.Next(500, 500000));
        string key = keyLeft + "&" + timeStamp.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&" + additionalData + "&" + keyRight;
        string encodedKey = CodeCarvings.Piczard.Helpers.SecurityHelper.EncryptString(key);

        return encodedKey;
    }

    protected void ProcessUploadSuccess()
    {
        string sourceImageClientFileName = this._SourceImageClientFileName;

        if (this.HasImage)
        {
            // Unload the current image
            this.UnloadImage(false);
        }

        // Delete old files
        if (File.Exists(this.TemporarySourceImageFilePath))
        {
            File.Delete(this.TemporarySourceImageFilePath);
        }

        if (this._ImageUploadPreProcessingFilter == null)
        {
            // Just copy the source image
            File.Copy(this.UploadFilePath, this.TemporarySourceImageFilePath, true);
        }

        try
        {
            if (this._ImageUploadPreProcessingFilter != null)
            {
                // Pre-process the just uploaded image
                using (LoadedImage sourceImage = ImageArchiver.LoadImage(this.UploadFilePath))
                {
                    //  Use PNG to preserve transparency
                    FormatEncoderParams format = new PngFormatEncoderParams();
                    using (System.Drawing.Image tempImage = this._ImageUploadPreProcessingFilter.GetProcessedImage(sourceImage, sourceImage.Resolution, format))
                    {
                        ImageArchiver.SaveImageToFileSystem(tempImage, this.TemporarySourceImageFilePath, format);

                        // Optimization: save server resources...
                        this.popupPictureTrimmer1.SetLoadImageData_ImageSize(tempImage.Size);
                        this.popupPictureTrimmer1.SetLoadImageData_ImageResolution(sourceImage.Resolution);
                        this.popupPictureTrimmer1.SetLoadImageData_ImageFormatId(sourceImage.FormatId);
                    }
                }
            }

            // Load the image in the PictureTrimmer control
            this.popupPictureTrimmer1.LoadImageFromFileSystem(this.TemporarySourceImageFilePath, this._OutputResolution, this.CropConstraint);
        }
        catch (InvalidImageSizeException ex)
        {
            // Invalid image size
            ex.ToString();

            // Display the invalid image size message
            this.SetCurrentStatusMessage(this.StatusMessage_InvalidImageSize, true);

            // EVENT: Upload error (invalid image size)
            this.OnUploadError(EventArgs.Empty);
        }
        catch
        {
            // Invalid image

            // Display the invalid image message
            this.SetCurrentStatusMessage(this.StatusMessage_InvalidImage, true);

            // EVENT: Upload error (invalid image)
            this.OnUploadError(EventArgs.Empty);
        }

        if (this.HasImage)
        {
            // Restore the source image client file name (changed in the UnloadImage method)
            this._SourceImageClientFileName = sourceImageClientFileName;

            // The new image has been uploaded
            this._ImageUploaded = true;
            this._ImageEdited = false;

            // Update the preview
            this._UpdatePreview = true;

            if (this.ImageUpload != null)
            {
                // EVENT: Image upload
                string pictureTrimmerTID = this.popupPictureTrimmer1.TemporaryFileId;
                ImageUploadEventArgs args = new ImageUploadEventArgs(this._OutputResolution, this.CropConstraint, this.PostProcessingFilter, this.PreviewFilter);
                this.OnImageUpload(args);
                if (this.HasImage)
                {
                    if (this.popupPictureTrimmer1.TemporaryFileId != pictureTrimmerTID)
                    {
                        // The image has been reloeaded outside the control

                        if (this.AutoOpenImageEditPopupAfterUpload)
                        {
                            // Open the image edit popup if necessary
                            this.OpenImageEditPopup();
                        }

                        // Exit !!!
                        return;
                    }
                }
                else
                {
                    // The image has been unloaded, exit.
                    return;
                }                

                bool reloadImage = false;
                if (args.OutputResolutionChanged)
                {
                    this._OutputResolution = args.OutputResolution;
                    reloadImage = true;
                }
                if (args.CropConstraintChanged)
                {
                    this._CropConstraint = args.CropConstraint;
                    reloadImage = true;
                }
                if (args.PostProcessingFilterChanged)
                {
                    this._PostProcessingFilter = args.PostProcessingFilter;
                    // No need to reload if only the post processing filter has changed
                    // AND - the updatePreview is surely already TRUE
                }
                if (args.PreviewFilterChanged)
                {
                    this._PreviewFilter = args.PreviewFilter;
                    // No need to reload if only the preview filter has changed
                    // AND - the updatePreview is surely already TRUE
                }
                if (args.ReloadImageSet)
                {
                    // Forced to reload the source image
                    reloadImage = true;
                }

                if (reloadImage)
                {
                    // Reload the image
                    if (!args.ReloadImageSet)
                    {
                        // Standard reload, use the current source image size, resolutaion and format to save memory
                        this.popupPictureTrimmer1.SetLoadImageData_ImageSize(this.SourceImageSize);
                        this.popupPictureTrimmer1.SetLoadImageData_ImageResolution(this.SourceImageResolution);
                        this.popupPictureTrimmer1.SetLoadImageData_ImageFormatId(this.SourceImageFormatId);
                    }
                    this.popupPictureTrimmer1.LoadImageFromFileSystem(this.TemporarySourceImageFilePath, this._OutputResolution, this.CropConstraint);
                }
            }

            // Invoke the OpenImageEditPopup after the event, so the eventhandler may change
            // the AutoOpenImageEditPopupAfterUpload property
            if (this.AutoOpenImageEditPopupAfterUpload)
            {
                // Open the image edit popup
                this.OpenImageEditPopup();
            }
        }
    }

    protected void ProcessUploadError()
    {
        if (this.HasImage)
        {
            // Unload the current image so we can display the error message
            this.UnloadImage(false);
        }

        // Display the error message;
        this.SetCurrentStatusMessage(this.StatusMessage_UploadError, true);

        // EVENT: Upload error
        this.OnUploadError(EventArgs.Empty);
    }

    protected void ProcessEdit()
    {
        // The new image has been edited
        this._ImageUploaded = false;
        this._ImageEdited = true;

        // Update the preview
        this._UpdatePreview = true;

        // EVENT: Image edit
        this.OnImageEdit(EventArgs.Empty);
    }

    protected void ProcessRemove()
    {
        // Unload the image
        this.UnloadImage(true);

        // EVENT: Image removed
        this.OnImageRemove(EventArgs.Empty);
    }

    protected void ProcessSelectedConfigurationIndexChanged()
    {
        // The new image has been edited
        this._ImageUploaded = false;
        this._ImageEdited = true;

        // Update the preview
        this._UpdatePreview = true;

        // Open the image edit popup
        this._AutoOpenImageEditPopup = true;

        if (this.SelectedConfigurationIndexChanged != null)
        {
            string pictureTrimmerTID = this.popupPictureTrimmer1.TemporaryFileId;
            /// EVENT: Configuration index changed
            SelectedConfigurationIndexChangedEventArgs args = new SelectedConfigurationIndexChangedEventArgs(this._OutputResolution, this.CropConstraint, this.PostProcessingFilter, this.PreviewFilter);
            this.OnSelectedConfigurationIndexChanged(args);
            if (this.HasImage)
            {
                if (this.popupPictureTrimmer1.TemporaryFileId != pictureTrimmerTID)
                {
                    // The image has been reloeaded outside the control, exit.
                    return;
                }
            }
            else
            {
                // The image has been unloaded, exit.
                return;
            } 

            bool reloadImage = false;
            if (args.OutputResolutionChanged)
            {
                this._OutputResolution = args.OutputResolution;
                reloadImage = true;
            }
            if (args.CropConstraintChanged)
            {
                this._CropConstraint = args.CropConstraint;
                reloadImage = true;
            }
            if (args.PostProcessingFilterChanged)
            {
                this._PostProcessingFilter = args.PostProcessingFilter;
                // No need to reload if only the post filter has changed
                // AND - the updatePreview is surely already TRUE
            }
            if (args.PreviewFilterChanged)
            {
                this._PreviewFilter = args.PreviewFilter;
                // No need to reload if only the preview filter has changed
                // AND - the updatePreview is surely already TRUE
            }
            if (args.ReloadImageSet)
            {
                // Forced to reload the source image
                reloadImage = true;
            }

            if (reloadImage)
            {
                // Reload the image
                if (!args.ReloadImageSet)
                {
                    // Standard reload, use the current source image size to save memory
                    this.popupPictureTrimmer1.SetLoadImageData_ImageSize(this.SourceImageSize);
                    this.popupPictureTrimmer1.SetLoadImageData_ImageResolution(this.SourceImageResolution);
                    this.popupPictureTrimmer1.SetLoadImageData_ImageFormatId(this.SourceImageFormatId);
                }
                this.popupPictureTrimmer1.LoadImageFromFileSystem(this.TemporarySourceImageFilePath, this._OutputResolution, this.CropConstraint);
            }
        }
    }

    #endregion

    #endregion

    #region Sub-Classes

    /// <summary>
    /// Base class that provides data for the ImageUpload and the SelectedConfigurationIndexChanged events.</summary>
    public class ConfigurationEventArgs
        : EventArgs
    {

        /// <summary>
        /// Intializes new instace of the ConfigurationEventArgs class.</summary>
        /// <param name="outputResolution">The resolution (DPI) of the image that is generated by the control.</param>
        /// <param name="cropConstraint">The constraints that have to be satisfied by the cropped image.</param>
        /// <param name="postProcessingFilter">The filter(s) to apply to the image.</param>
        /// <param name="previewFilter">The filter(s) to apply to the preview image.</param>
        public ConfigurationEventArgs(float outputResolution, CropConstraint cropConstraint, ImageProcessingFilter postProcessingFilter, ImageProcessingFilter previewFilter)
        {
            this._OutputResolution = outputResolution;
            this._CropConstraint = cropConstraint;
            this._PostProcessingFilter = postProcessingFilter;
            this._PreviewFilter = previewFilter;

            this._OriginalOutputResolution = this._OutputResolution;
            this._OriginalCropConstraintString = JSONSerializer.SerializeToString(this._CropConstraint, true);
            this._OriginalPostProcessingFilterString = JSONSerializer.SerializeToString(this._PostProcessingFilter, true);
            this._OriginalPreviewFilterString = JSONSerializer.SerializeToString(this._PreviewFilter, true);

            this._ReloadImageSet = false;
        }

        private float _OutputResolution;
        /// <summary>
        /// Gets or sets the resolution (DPI) of the image that is generated by the control.</summary>        
        public float OutputResolution
        {
            get
            {
                return this._OutputResolution;
            }
            set
            {
                // Validate the resolution
                CodeCarvings.Piczard.Helpers.ImageHelper.ValidateResolution(value, true);

                this._OutputResolution = value;
            }
        }

        private CropConstraint _CropConstraint;
        /// <summary>
        /// Gets the constraints that have to be satisfied by the cropped image.</summary>        
        public CropConstraint CropConstraint
        {
            get
            {
                return this._CropConstraint;
            }
            set
            {
                this._CropConstraint = value;
            }
        }

        private ImageProcessingFilter _PostProcessingFilter;
        /// <summary>
        /// Gets or sets the filter(s) to apply to the image.</summary>
        public ImageProcessingFilter PostProcessingFilter
        {
            get
            {
                return this._PostProcessingFilter;
            }
            set
            {
                this._PostProcessingFilter = value;
            }
        }

        private ImageProcessingFilter _PreviewFilter;
        /// <summary>
        /// Gets or sets the filter(s) to apply to the preview image.</summary>
        public ImageProcessingFilter PreviewFilter
        {
            get
            {
                return this._PreviewFilter;
            }
            set
            {
                this._PreviewFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets the ResizeConstraint used to generate the preview image.
        /// This property is marked as obsolete since version 2.0.0 of the control and will be soon removed.
        /// Use 'PreviewFilter' instead.</summary>
        [Obsolete]
        public ResizeConstraint PreviewResizeConstraint
        {
            get
            {
                return (ResizeConstraint)this.PreviewFilter;
            }
            set
            {
                this.PreviewFilter = value;
            }
        }

        private float _OriginalOutputResolution;
        internal bool OutputResolutionChanged
        {
            get
            {
                return this._OutputResolution != this._OriginalOutputResolution;
            }
        }

        private string _OriginalCropConstraintString;
        internal bool CropConstraintChanged
        {
            get
            {
                return JSONSerializer.SerializeToString(this._CropConstraint, true) != this._OriginalCropConstraintString;
            }
        }

        private string _OriginalPostProcessingFilterString;
        internal bool PostProcessingFilterChanged
        {
            get
            {
                return JSONSerializer.SerializeToString(this.PostProcessingFilter, true) != this._OriginalPostProcessingFilterString;
            }
        }

        private string _OriginalPreviewFilterString;
        internal bool PreviewFilterChanged
        {
            get
            {
                return JSONSerializer.SerializeToString(this._PreviewFilter, true) != this._OriginalPreviewFilterString;
            }
        }

        private bool _ReloadImageSet;
        internal bool ReloadImageSet
        {
            get
            {
                return this._ReloadImageSet;
            }
        }
        /// <summary>
        /// Force the reloading of the source image.</summary>
        public void ReloadImage()
        {
            this._ReloadImageSet = true;
        }
    }

    /// <summary>
    /// Provides data for the ImageUpload event.</summary>
    public class ImageUploadEventArgs
        : ConfigurationEventArgs
    {
        /// <summary>
        /// Intializes new instace of the ImageUploadEventArgs class.</summary>
        /// <param name="outputResolution">The resolution (DPI) of the image that is generated by the control.</param>
        /// <param name="cropConstraint">The constraints that have to be satisfied by the cropped image.</param>
        /// <param name="postProcessingFilter">The filter(s) to apply to the image.</param>
        /// <param name="previewFilter">The filter(s) to apply to the preview image.</param>
        public ImageUploadEventArgs(float outputResolution, CropConstraint cropConstraint, ImageProcessingFilter postProcessingFilter, ImageProcessingFilter previewFilter)
            : base(outputResolution, cropConstraint, postProcessingFilter, previewFilter)
        {
        }
    }

    /// <summary>
    /// Provides data for the SelectedConfigurationIndexChanged event.</summary>
    public class SelectedConfigurationIndexChangedEventArgs
        : ConfigurationEventArgs
    {
        /// <summary>
        /// Intializes new instace of the SelectedConfigurationIndexChangedEventArgs class.</summary>
        /// <param name="outputResolution">The resolution (DPI) of the image that is generated by the control.</param>
        /// <param name="cropConstraint">The constraints that have to be satisfied by the cropped image.</param>
        /// <param name="postProcessingFilter">The filter(s) to apply to the image.</param>
        /// <param name="previewFilter">The filter(s) to apply to the preview image.</param>
        public SelectedConfigurationIndexChangedEventArgs(float outputResolution, CropConstraint cropConstraint, ImageProcessingFilter postProcessingFilter, ImageProcessingFilter previewFilter)
            : base(outputResolution, cropConstraint, postProcessingFilter, previewFilter)
        {
        }
    }

    /// <summary>
    /// Specifies if and when PostProcessingFilter must be applied.</summary>
    [Serializable]
    public enum SimpleImageUploadPostProcessingFilterApplyMode
        : int
    {
        Never = 0,
        OnlyNewImages = 1,
        Always = 2
    }

    /// <summary>
    /// Provides access to settings of a PopupPictureTrimmer instance.</summary>
    public class PopupPictureTrimmerSettingsProvider
    {

        public PopupPictureTrimmerSettingsProvider(PopupPictureTrimmer pictureTrimmer)
        {
            this._PictureTrimmer = pictureTrimmer;
        }

        private PopupPictureTrimmer _PictureTrimmer;
        
        /// <summary>
        /// Gets or sets a value indicating whether users can resize the source image through the GUI.</summary>
        public bool AllowResize
        {
            get
            {
                return this._PictureTrimmer.AllowResize;
            }
            set
            {
                this._PictureTrimmer.AllowResize = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control has to automatically freeze the GUI when the form is submitted.</summary>
        public bool AutoFreezeOnFormSubmit
        {
            get
            {
                return this._PictureTrimmer.AutoFreezeOnFormSubmit;
            }
            set
            {
                this._PictureTrimmer.AutoFreezeOnFormSubmit = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the PictureTrimmer instance automatically calculates the ZoomFactor.</summary>
        public PictureTrimmerAutoZoomMode AutoZoomMode
        {
            get
            {
                return this._PictureTrimmer.AutoZoomMode;
            }
            set
            {
                this._PictureTrimmer.AutoZoomMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the BackColor property of the PictureTrimmer control.</summary>
        public Color BackColor
        {
            get
            {
                return this._PictureTrimmer.BackColor;
            }
            set
            {
                this._PictureTrimmer.BackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the text of the "Cancel button".</summary>
        public string CancelButtonText
        {
            get
            {
                return this._PictureTrimmer.CancelButtonText;
            }
            set
            {
                this._PictureTrimmer.CancelButtonText = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the PictureTrimmer GUI renders the cropping mask.</summary>
        public PictureTrimmerCropShadowMode CropShadowMode
        {
            get
            {
                return this._PictureTrimmer.CropShadowMode;
            }
            set
            {
                this._PictureTrimmer.CropShadowMode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control has to automatically
        /// center the view after the user drags the crop area outside the visible area.</summary>
        public bool EnableAutoCenterView
        {
            get
            {
                return this._PictureTrimmer.EnableAutoCenterView;
            }
            set
            {
                this._PictureTrimmer.EnableAutoCenterView = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the crop area automatically snaps
        /// to the edge or the center of the image when the user moves the rectangle
        /// near those positions.</summary>
        public bool EnableSnapping
        {
            get
            {
                return this._PictureTrimmer.EnableSnapping;
            }
            set
            {
                this._PictureTrimmer.EnableSnapping = value;
            }
        }

        /// <summary>
        /// Gets or sets the Window Mode property of the Adobe Flash movie for transparency,
        /// layering, and positioning in the browser (it is strongly suggested to use
        /// the FlashWMode.Window setting).</summary>
        public FlashWMode FlashWMode
        {
            get
            {
                return this._PictureTrimmer.FlashWMode;
            }
            set
            {
                this._PictureTrimmer.FlashWMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the foreground color.</summary>
        public Color ForeColor
        {
            get
            {
                return this._PictureTrimmer.ForeColor;
            }
            set
            {
                this._PictureTrimmer.ForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the Cascading Style Sheet (CSS) class used to customize the
        /// style of the LightBox popup window.</summary>
        public string LightBoxCssClass
        {
            get
            {
                return this._PictureTrimmer.LightBoxCssClass;
            }
            set
            {
                this._PictureTrimmer.LightBoxCssClass = value;
            }
        }

        /// <summary>
        /// Gets or sets the text of the "Save button".</summary>
        public string SaveButtonText
        {
            get
            {
                return this._PictureTrimmer.SaveButtonText;
            }
            set
            {
                this._PictureTrimmer.SaveButtonText = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display the "Cancel button" in the popup window.</summary>
        public bool ShowCancelButton
        {
            get
            {
                return this._PictureTrimmer.ShowCancelButton;
            }
            set
            {
                this._PictureTrimmer.ShowCancelButton = value;
            }
        }   

        /// <summary>
        /// Gets or sets a value indicating whether to show lines that facilitate the
        /// alignment of the crop rectangle.</summary>
        public bool ShowCropAlignmentLines
        {
            get
            {
                return this._PictureTrimmer.ShowCropAlignmentLines;
            }
            set
            {
                this._PictureTrimmer.ShowCropAlignmentLines = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the "Details panel" in the
        /// component GUI.</summary>
        public bool ShowDetailsPanel
        {
            get
            {
                return this._PictureTrimmer.ShowDetailsPanel;
            }
            set
            {
                this._PictureTrimmer.ShowDetailsPanel = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the flip control in the "Rotate/Flip panel". 
        /// The flip control allows the user to flip the image horizontally and/or vertically.</summary>
        public bool ShowFlipPanel
        {
            get
            {
                return this._PictureTrimmer.ShowFlipPanel;
            }
            set
            {
                this._PictureTrimmer.ShowFlipPanel = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the "Adjustments panel" in
        /// the component GUI. The "Adjustments panel" allows the user to change Brightness,
        /// Contrast, Hue and/or Saturation of the Image.</summary>
        public bool ShowImageAdjustmentsPanel
        {
            get
            {
                return this._PictureTrimmer.ShowImageAdjustmentsPanel;
            }
            set
            {
                this._PictureTrimmer.ShowImageAdjustmentsPanel = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the "Resize panel" in the
        /// component GUI. The "Resize panel" allows the user to change the ResizeFactor
        /// applied to the source Image.</summary>
        public bool ShowResizePanel
        {
            get
            {
                return this._PictureTrimmer.ShowResizePanel;
            }
            set
            {
                this._PictureTrimmer.ShowResizePanel = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the rotate control in the
        /// "Rotate/Flip panel". The rotate control allows the user to rotate the image
        /// clockwise by 0, 90, 180 or 270 degrees.</summary>
        public bool ShowRotatePanel
        {
            get
            {
                return this._PictureTrimmer.ShowRotatePanel;
            }
            set
            {
                this._PictureTrimmer.ShowRotatePanel = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the rulers at the left and
        /// at the top of the working area.</summary>
        public bool ShowRulers
        {
            get
            {
                return this._PictureTrimmer.ShowRulers;
            }
            set
            {
                this._PictureTrimmer.ShowRulers = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the "Zoom panel" in the component
        /// GUI. The "Zoom panel" allows the user to magnify an area of the image.
        /// Please note that ZoomPanel is always invisible when the crop feature is disabled (CropConstraint = null).</summary>
        public bool ShowZoomPanel
        {
            get
            {
                return this._PictureTrimmer.ShowZoomPanel;
            }
            set
            {
                this._PictureTrimmer.ShowZoomPanel = value;
            }
        }     

    }

    #endregion

}
