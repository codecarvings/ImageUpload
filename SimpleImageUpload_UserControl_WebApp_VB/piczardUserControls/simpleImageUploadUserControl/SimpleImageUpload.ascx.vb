' -------------------------------------------------------
' Piczard | SimpleImageUpload User Control
' Author: Sergio Turolla
' <codecarvings.com>
' 
' Copyright (c) 2011-2013 Sergio Turolla
' All rights reserved.
' 
' Redistribution and use in source and binary forms, with or
' without modification, are permitted provided that the 
' following conditions are met:
' 
' - Redistributions of source code must retain the above 
'   copyright notice, this list of conditions and the 
'   following disclaimer.
' - Redistributions in binary form must reproduce the above
'   copyright notice, this list of conditions and the 
'   following disclaimer in the documentation and/or other
'   materials provided with the distribution.
' 
' THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
' CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
' INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
' MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
' DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
' CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
' SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
' BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
' SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
' INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
' WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
' NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
' OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
' SUCH DAMAGE.
' -------------------------------------------------------

' #########
' SimpleImageUpload Version 3.0.3
' #########

Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Xml
Imports System.IO
Imports System.ComponentModel

Imports CodeCarvings.Piczard
Imports CodeCarvings.Piczard.Serialization
Imports CodeCarvings.Piczard.Web
Imports CodeCarvings.Piczard.Web.Helpers

''' <summary>
''' A ready-to-use ASCX control that provides advanced image uploading features.</summary>
Partial Public Class SimpleImageUpload
    Inherits System.Web.UI.UserControl
    Implements IPostBackDataHandler

#Region "Consts"

    Protected Shared ReadOnly DefaultImageEditPopupSize As Size = New Size(800, 520)
    Protected Shared ReadOnly DefaultButtonSize As Size = New Size(110, 26)
    Protected Shared PerformTemporaryFolderWriteTestOnPageLoad As Boolean = True

    Protected Const DefaultValues_BackColor As String = "#eeeeee"
    Protected Const DefaultValues_BorderColor As String = "#cccccc"
    Protected Shared ReadOnly DefaultValues_BorderStyle As BorderStyle = BorderStyle.Solid
    Protected Shared ReadOnly DefaultValues_BorderWidth As Unit = Unit.Pixel(1)

    Protected Const DefaultValues_ContentBackColor As String = "#ffffff"
    Protected Const DefaultValues_ContentForeColor As String = "#000000"
    Protected Const DefaultValues_ContentErrorForeColor As String = "#cc0000"
    Protected Const DefaultValues_ContentBorderColor As String = "#cccccc"
    Protected Shared ReadOnly DefaultValues_ContentBorderStyle As BorderStyle = BorderStyle.Solid
    Protected Shared ReadOnly DefaultValues_ContentBorderWidth As Unit = Unit.Pixel(1)

    Protected Const DefaultValues_PreviewBorderColor As String = "#cccccc"
    Protected Shared ReadOnly DefaultValues_PreviewBorderStyle As BorderStyle = BorderStyle.Solid
    Protected Shared ReadOnly DefaultValues_PreviewBorderWidth As Unit = Unit.Pixel(1)

#End Region

#Region "Event Handlers"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If (PerformTemporaryFolderWriteTestOnPageLoad) Then
            ' Check if the application can write on the temporary folder
            Me.TemporaryFolderWriteTest()
        End If

        ' Load the JS file
        Dim t As Type = Me.Page.GetType()
        Dim scriptKey As String = "simpleImageUpload.js"
        If (Not Me.Page.ClientScript.IsClientScriptIncludeRegistered(t, scriptKey)) Then
            Me.Page.ClientScript.RegisterClientScriptInclude(t, scriptKey, Me.ResolveUrl("simpleImageUpload.js?v=5"))
        End If

        ' Reset the initialization function
        Me.popupPictureTrimmer1.OnClientControlLoadFunction = ""
    End Sub

#End Region

#Region "Overrides"

#Region "ControlState"

    Protected Overrides Function SaveControlState() As Object
        Dim values As List(Of Object) = New List(Of Object)()
        values.Add(MyBase.SaveControlState())

        values.Add(Me.TemporaryFileId)
        values.Add(Me._OutputResolution)
        values.Add(JSONSerializer.SerializeToString(Me._CropConstraint))
        values.Add(JSONSerializer.SerializeToString(Me._ImageUploadPreProcessingFilter, True)) ''Important: also serialize the type!
        values.Add(JSONSerializer.SerializeToString(Me._PostProcessingFilter, True)) ''Important: also serialize the type!
        values.Add(DirectCast(Me._PostProcessingFilterApplyMode, Integer))
        values.Add(JSONSerializer.SerializeToString(Me._PreviewFilter, True)) ''Important: also serialize the type!
        values.Add(Me._ImageUploaded)
        values.Add(Me._ImageEdited)
        values.Add(Me._SourceImageClientFileName)
        values.Add(Me._Configurations)

        ' These properties are important and are not saved in the viewsate! (EnableViewState=false in ASCX)
        values.Add(Me.imgPreview.ImageUrl)
        values.Add(Me.imgPreview.Width)
        values.Add(Me.imgPreview.Height)

        values.Add(Me._DebugUploadProblems)

        Return values.ToArray()
    End Function

    Protected Overrides Sub LoadControlState(ByVal savedState As Object)
        If ((savedState IsNot Nothing) And (TypeOf savedState Is Object())) Then
            Dim values As Object() = DirectCast(savedState, Object())
            Dim i As Integer = 0
            MyBase.LoadControlState(values(i))
            i += 1

            Me._TemporaryFileId = DirectCast(values(i), String)
            i += 1
            Me._OutputResolution = DirectCast(values(i), Single)
            i += 1
            Me._CropConstraint = CropConstraint.FromJSON(DirectCast(values(i), String))
            i += 1
            Me._ImageUploadPreProcessingFilter = DirectCast(JSONSerializer.Deserialize(DirectCast(values(i), String)), ImageProcessingFilter)
            i += 1
            Me._PostProcessingFilter = DirectCast(JSONSerializer.Deserialize(DirectCast(values(i), String)), ImageProcessingFilter)
            i += 1
            Me._PostProcessingFilterApplyMode = DirectCast(DirectCast(values(i), Integer), SimpleImageUploadPostProcessingFilterApplyMode)
            i += 1
            Me._PreviewFilter = DirectCast(JSONSerializer.Deserialize(DirectCast(values(i), String)), ImageProcessingFilter)
            i += 1
            Me._ImageUploaded = DirectCast(values(i), Boolean)
            i += 1
            Me._ImageEdited = DirectCast(values(i), Boolean)
            i += 1
            Me._SourceImageClientFileName = DirectCast(values(i), String)
            i += 1
            Me._Configurations = DirectCast(values(i), String())
            i += 1

            Me.imgPreview.ImageUrl = DirectCast(values(i), String)
            i += 1
            Me.imgPreview.Width = DirectCast(values(i), Unit)
            i += 1
            Me.imgPreview.Height = DirectCast(values(i), Unit)
            i += 1

            Me._DebugUploadProblems = DirectCast(values(i), Boolean)
            i += 1
        End If
    End Sub

#End Region

#Region "Viewstate"

    Protected Overrides Function SaveViewState() As Object
        Dim values As List(Of Object) = New List(Of Object)()
        values.Add(MyBase.SaveViewState())

        values.Add(Me._Width)

        values.Add(Me._BackColor)
        values.Add(Me._BorderColor)
        values.Add(Me._BorderStyle)
        values.Add(Me._BorderWidth)

        values.Add(Me._ContentBackColor)
        values.Add(Me._ContentForeColor)
        values.Add(Me._ContentErrorForeColor)
        values.Add(Me._ContentBorderColor)
        values.Add(Me._ContentBorderStyle)
        values.Add(Me._ContentBorderWidth)

        values.Add(Me._PreviewBorderColor)
        values.Add(Me._PreviewBorderStyle)
        values.Add(Me._PreviewBorderWidth)

        values.Add(Me._AutoOpenImageEditPopupAfterUpload)
        values.Add(Me._AutoDisableImageEdit)
        values.Add(Me._ImageEditPopupSize)
        values.Add(Me._ButtonSize)
        values.Add(Me._CssClass)

        values.Add(Me._EnableEdit)
        values.Add(Me._EnableRemove)
        values.Add(Me._EnableUpload)
        values.Add(Me._EnableCancelUpload)

        values.Add(Me._Text_EditButton)
        values.Add(Me._Text_RemoveButton)
        values.Add(Me._Text_BrowseButton)
        values.Add(Me._Text_CancelUploadButton)
        values.Add(Me._Text_ConfigurationLabel)
        values.Add(Me._StatusMessage_NoImageSelected)
        values.Add(Me._StatusMessage_UploadError)
        values.Add(Me._StatusMessage_InvalidImage)
        values.Add(Me._StatusMessage_InvalidImageSize)
        values.Add(Me._StatusMessage_Wait)

        Return values.ToArray()
    End Function

    Protected Overrides Sub LoadViewState(ByVal savedState As Object)
        If ((savedState IsNot Nothing) And (TypeOf savedState Is Object())) Then
            Dim values As Object() = DirectCast(savedState, Object())
            Dim i As Integer = 0
            MyBase.LoadViewState(values(i))
            i += 1

            Me._Width = DirectCast(values(i), Unit)
            i += 1

            Me._BackColor = DirectCast(values(i), Color)
            i += 1
            Me._BorderColor = DirectCast(values(i), Color)
            i += 1
            Me._BorderStyle = DirectCast(values(i), BorderStyle)
            i += 1
            Me._BorderWidth = DirectCast(values(i), Unit)
            i += 1

            Me._ContentBackColor = DirectCast(values(i), Color)
            i += 1
            Me._ContentForeColor = DirectCast(values(i), Color)
            i += 1
            Me._ContentErrorForeColor = DirectCast(values(i), Color)
            i += 1
            Me._ContentBorderColor = DirectCast(values(i), Color)
            i += 1
            Me._ContentBorderStyle = DirectCast(values(i), BorderStyle)
            i += 1
            Me._ContentBorderWidth = DirectCast(values(i), Unit)
            i += 1

            Me._PreviewBorderColor = DirectCast(values(i), Color)
            i += 1
            Me._PreviewBorderStyle = DirectCast(values(i), BorderStyle)
            i += 1
            Me._PreviewBorderWidth = DirectCast(values(i), Unit)
            i += 1

            Me._AutoOpenImageEditPopupAfterUpload = DirectCast(values(i), Boolean)
            i += 1
            Me._AutoDisableImageEdit = DirectCast(values(i), Boolean)
            i += 1
            Me._ImageEditPopupSize = DirectCast(values(i), Size)
            i += 1
            Me._ButtonSize = DirectCast(values(i), Size)
            i += 1
            Me._CssClass = DirectCast(values(i), String)
            i += 1

            Me._EnableEdit = DirectCast(values(i), Boolean)
            i += 1
            Me._EnableRemove = DirectCast(values(i), Boolean)
            i += 1
            Me._EnableUpload = DirectCast(values(i), Boolean)
            i += 1
            Me._EnableCancelUpload = DirectCast(values(i), Boolean)
            i += 1

            Me._Text_EditButton = DirectCast(values(i), String)
            i += 1
            Me._Text_RemoveButton = DirectCast(values(i), String)
            i += 1
            Me._Text_BrowseButton = DirectCast(values(i), String)
            i += 1
            Me._Text_CancelUploadButton = DirectCast(values(i), String)
            i += 1
            Me._Text_ConfigurationLabel = DirectCast(values(i), String)
            i += 1
            Me._StatusMessage_NoImageSelected = DirectCast(values(i), String)
            i += 1
            Me._StatusMessage_UploadError = DirectCast(values(i), String)
            i += 1
            Me._StatusMessage_InvalidImage = DirectCast(values(i), String)
            i += 1
            Me._StatusMessage_InvalidImageSize = DirectCast(values(i), String)
            i += 1
            Me._StatusMessage_Wait = DirectCast(values(i), String)
            i += 1
        End If
    End Sub

#End Region

#Region "Render"

    Protected Overrides Sub OnInit(ByVal e As EventArgs)
        Me.Page.RegisterRequiresControlState(Me)
        Me.Page.RegisterRequiresPostBack(Me)

        MyBase.OnInit(e)
    End Sub

    Protected Overrides Sub OnPreRender(ByVal e As EventArgs)

        ' ### Dynamic load CSS and JS files ######
        Dim crlf As String = "" 'ControlChars.CrLf

        Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder()
        sb.Append("<script type=""text/javascript"">" + ControlChars.CrLf)
        ' Google Chrome & Safari Ajax Bug
        Dim isInAjaxPostBack As Boolean = AjaxHelper.IsInAjaxPostBack(Me.Page)
        If (Not isInAjaxPostBack) Then
            sb.Append("//<![CDATA[" + ControlChars.CrLf)
        End If

        ' JS function executed after the JS library is loaded
        sb.Append("function " + Me.InitFunctionName2 + "()" + crlf)
        sb.Append("{" + crlf)
        sb.Append("var loadData={" + crlf)
        sb.Append("popupPictureTrimmerClientId:""" + JSHelper.EncodeString(Me.popupPictureTrimmer1.ClientID) + """" + crlf)
        sb.Append(",btnEditClientId:""" + JSHelper.EncodeString(Me.btnEdit.ClientID) + """" + crlf)
        sb.Append(",btnRemoveClientId:""" + JSHelper.EncodeString(Me.btnRemove.ClientID) + """" + crlf)
        sb.Append(",btnBrowseDisabledClientId:""" + JSHelper.EncodeString(Me.btnBrowseDisabled.ClientID) + """" + crlf)
        sb.Append(",btnCancelUploadClientId:""" + JSHelper.EncodeString(Me.btnCancelUpload.ClientID) + """" + crlf)
        sb.Append(",btnBrowseClientId:""" + JSHelper.EncodeString(Me.btnBrowse.ClientID) + """" + crlf)
        sb.Append(",hfActClientId:""" + JSHelper.EncodeString(Me.hfAct.ClientID) + """" + crlf)
        sb.Append(",ddlConfigurationsClientId:""" + JSHelper.EncodeString(Me.ddlConfigurations.ClientID) + """" + crlf)
        sb.Append(",hlPictureImageEditId:""" + JSHelper.EncodeString(Me.hlPictureImageEdit.ClientID) + """" + crlf)

        sb.Append(",uploadUrl:""" + JSHelper.EncodeString(Me.UploadUrl) + """" + crlf)
        sb.Append(",uploadMonitorUrl:""" + JSHelper.EncodeString(Me.UploadMonitorUrl) + """" + crlf)
        sb.Append(",btnPostBack_PostBackEventReference:""" + JSHelper.EncodeString(Me.Page.ClientScript.GetPostBackEventReference(Me.btnPostBack, "")) + """" + crlf)
        sb.Append(",imageEditPopupSize_width:" + Me.ImageEditPopupSize.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) + crlf)
        sb.Append(",imageEditPopupSize_height:" + Me.ImageEditPopupSize.Height.ToString(System.Globalization.CultureInfo.InvariantCulture) + crlf)
        sb.Append(",autoOpenImageEditPopup:" + JSHelper.EncodeBool(Me._AutoOpenImageEditPopup) + crlf)
        sb.Append(",autoDisableImageEdit:" + JSHelper.EncodeBool(Me._AutoDisableImageEdit) + crlf)
        sb.Append(",buttonSize_width:" + Me.ButtonSize.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) + crlf)
        sb.Append(",buttonSize_height:" + Me.ButtonSize.Height.ToString(System.Globalization.CultureInfo.InvariantCulture) + crlf)
        sb.Append(",enableEdit:" + JSHelper.EncodeBool(Me._EnableEdit) + crlf)
        sb.Append(",enableRemove:" + JSHelper.EncodeBool(Me._EnableRemove) + crlf)
        sb.Append(",enableCancelUpload:" + JSHelper.EncodeBool(Me._EnableCancelUpload) + crlf)
        sb.Append(",dup:" + JSHelper.EncodeBool(Me._DebugUploadProblems) + crlf)
        sb.Append(",statusMessage_Wait:""" + JSHelper.EncodeString(Me.StatusMessage_Wait) + """" + crlf)
        sb.Append("};" + crlf)
        sb.Append("var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.loadControl(""" + JSHelper.EncodeString(Me.ClientID) + """, loadData);")
        sb.Append("}" + crlf)

        ' Dynamic load JS / CSS (required for Ajax)
        sb.Append("function " + Me.InitFunctionName + "()" + crlf)
        sb.Append("{" + crlf)
        sb.Append("if (typeof(window.__ccpz_siu_lt) === ""undefined"")" + crlf)
        sb.Append("{" + crlf)
        ' The variable (window.__ccpz_siu_lt) (configured in simpleImageUpload.js) is undefined...
        sb.Append(JSHelper.GetLoadScript(Me.ResolveUrl("simpleImageUpload.js?v=5"), Me.InitFunctionName + "_load_js", Me.InitFunctionName2 + "();") + crlf)
        sb.Append("}" + crlf)
        sb.Append("else" + crlf)
        sb.Append("{" + crlf)
        sb.Append(Me.InitFunctionName2 + "();" + crlf)
        sb.Append("}" + crlf)
        sb.Append("}" + crlf)

        If (Not isInAjaxPostBack) Then
            sb.Append(ControlChars.CrLf + "//]]>" + ControlChars.CrLf)
        End If
        sb.Append("</script>")

        Dim scriptToRegister As String = sb.ToString()
        If (Not AjaxHelper.RegisterClientScriptBlockInAjaxPostBack(Me.Page, "CCPZ_SIU_DAI_" + Me.ClientID, scriptToRegister, False)) Then
            Me.litScript.Text = scriptToRegister
        Else
            Me.litScript.Text = ""
        End If

        ' Setup the initialization function
        If (Me.Visible) Then
            Me.popupPictureTrimmer1.OnClientControlLoadFunction = Me.InitFunctionName
        End If

        ' Hide design-time elements
        Me.phDesignTimeStart.Visible = False
        Me.phDesignTimeEnd.Visible = False

        ' Update the layout
        Me.btnEdit.Visible = Me.EnableEdit
        Me.btnEdit.Width = Me.ButtonSize.Width
        Me.btnEdit.Height = Me.ButtonSize.Height

        Me.btnRemove.Visible = Me.EnableRemove
        Me.btnRemove.Width = Me.ButtonSize.Width
        Me.btnRemove.Height = Me.ButtonSize.Height

        Me.btnBrowse.Width = Me.ButtonSize.Width
        Me.btnBrowseDisabled.Width = Me.ButtonSize.Width

        Me.btnBrowse.Height = Me.ButtonSize.Height
        Me.btnBrowseDisabled.Height = Me.ButtonSize.Height

        Me.btnCancelUpload.OnClientClick = "CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.cancelUpload(""" + JSHelper.EncodeString(Me.ClientID) + """); return false;"

        Me.btnCancelUpload.Width = Me.ButtonSize.Width
        Me.btnCancelUpload.Height = Me.ButtonSize.Height

        Me.phEditCommands.Visible = Me.EnableEdit Or Me.EnableRemove

        Me.phUploadCommands.Visible = Me.EnableUpload
        Me.btnCancelUpload.Visible = Me.EnableCancelUpload

        ' Update the texts
        Me.litStatusMessage.Text = Me.GetCurrentStatusMessage()

        Me.btnEdit.Text = Me.Text_EditButton
        Me.btnRemove.Text = Me.Text_RemoveButton
        Me.btnBrowse.Text = Me.Text_BrowseButton
        Me.btnBrowseDisabled.Text = Me.Text_BrowseButton
        Me.btnCancelUpload.Text = Me.Text_CancelUploadButton

        If (Me.HasImage) Then
            If (Not File.Exists(Me.PreviewImageFilePath)) Then
                ' The preview file does not exists -> create it
                Me._UpdatePreview = True
            End If

            If (Me._UpdatePreview) Then
                ' Get the processing job (Default resolution = 96DPI)
                Dim job As ImageProcessingJob = Me.GetImageProcessingJob()
                job.OutputResolution = CommonData.DefaultResolution

                ' Add the preview filter
                If (Me.PreviewFilter IsNot Nothing) Then
                    job.Filters.Add(Me.PreviewFilter)
                End If

                ' Save the preview image
                Me.imgPreview.ImageUrl = Nothing
                If (File.Exists(Me.TemporarySourceImageFilePath)) Then
                    ' job.SaveProcessedImageToFileSystem(Me.TemporarySourceImageFilePath, Me.PreviewImageFilePath, New JpegFormatEncoderParams())

                    ' Jpeg images does not allow transparent images - Apply the right back color!
                    Dim format As FormatEncoderParams = New JpegFormatEncoderParams()
                    Using previewImage As System.Drawing.Image = job.GetProcessedImage(Me.TemporarySourceImageFilePath, format)
                        ImageArchiver.SaveImageToFileSystem(previewImage, Me.PreviewImageFilePath, format)

                        ' Force the reload of the preview
                        Me.imgPreview.ImageUrl = Me.PreviewImageUrl
                        Me.imgPreview.Width = Unit.Pixel(previewImage.Size.Width)
                        Me.imgPreview.Height = Unit.Pixel(previewImage.Size.Height)
                    End Using
                End If

                ' Force the reload of the preivew
                Me.imgPreview.ImageUrl = Me.PreviewImageUrl
            End If
        End If

        If (String.IsNullOrEmpty(Me.imgPreview.ImageUrl)) Then
            ' Set a dummy image (for xhtml compliance)
            Me.imgPreview.ImageUrl = Me.ResolveUrl("blank.gif")
            Me.imgPreview.Width = Unit.Pixel(1)
            Me.imgPreview.Height = Unit.Pixel(1)
        End If

        Me.imgPreview.BorderColor = Me._PreviewBorderColor
        Me.imgPreview.BorderStyle = Me._PreviewBorderStyle
        Me.imgPreview.BorderWidth = Me._PreviewBorderWidth

        If (Me._CropConstraint Is Nothing) Then
            ' Crop disabled
            Me.popupPictureTrimmer1.ShowZoomPanel = False
        End If

        ' Update the configuration UI
        Me.litSelectConfiguration.Text = Me.Text_ConfigurationLabel
        Me.ddlConfigurations.Items.Clear()
        Dim configurations As String() = Me.Configurations
        If (Not (configurations Is Nothing)) Then
            For i As Integer = 0 To configurations.Length - 1
                Me.ddlConfigurations.Items.Add(New ListItem(configurations(i), i.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            Next
            Me.ddlConfigurations.SelectedIndex = Me.SelectedConfigurationIndex.Value
        End If
        Me.ddlConfigurations.Attributes("onchange") = "CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.onConfigurationChange(""" + JSHelper.EncodeString(Me.ClientID) + """);"

        ' Hide the SELECT element to mantain compliance with XHTML specs (SELECT cannot be emtpy...)
        If (Me.ddlConfigurations.Items.Count > 0) Then
            Me.ddlConfigurations.Visible = True
        Else
            Me.ddlConfigurations.Visible = False
        End If

        If (Not String.IsNullOrEmpty(Me._CssClass)) Then
            Dim classes As String() = Me._CssClass.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
            If (Array.IndexOf(Of String)(classes, "ccpz_fobr") >= 0) Then
                Me.popupPictureTrimmer1.CssClass = If(String.IsNullOrEmpty(Me.popupPictureTrimmer1.CssClass), "ccpz_fobr", Me.popupPictureTrimmer1.CssClass + " ccpz_fobr")
            End If
        End If

        MyBase.OnPreRender(e)
    End Sub

#End Region

#End Region

#Region "IPostBackDataHandler Members"

    Protected Function LoadPostData(ByVal postDataKey As String, ByVal postCollection As System.Collections.Specialized.NameValueCollection) As Boolean Implements System.Web.UI.IPostBackDataHandler.LoadPostData
        Me._LastAct = Me.hfAct.Value
        ' Reset the action
        Me.hfAct.Value = ""

        Dim selectedConfigurationIndex As String = postCollection(Me.ddlConfigurations.UniqueID)
        If (Not String.IsNullOrEmpty(selectedConfigurationIndex)) Then
            ' Int value
            Me._SelectedConfigurationIndex = Integer.Parse(selectedConfigurationIndex, System.Globalization.CultureInfo.InvariantCulture)
        Else
            ' Null value
            Me._SelectedConfigurationIndex = Nothing
        End If


        Select Case Me._LastAct
            Case "upload"
                ' A new file has been uploaded
                If (File.Exists(Me.UploadMonitorFilePath)) Then
                    Dim doc As XmlDocument = New XmlDocument()
                    Using reader As StreamReader = File.OpenText(Me.UploadMonitorFilePath)
                        ' Load the document
                        doc.Load(reader)
                    End Using

                    Dim nodes As XmlNodeList = doc.GetElementsByTagName("uploadMonitor")
                    If (nodes.Count > 0) Then
                        Dim uploadMonitorNode As XmlNode = nodes(0)

                        Dim stateAttribute As XmlAttribute = uploadMonitorNode.Attributes("state")
                        If (stateAttribute IsNot Nothing) Then
                            If (Not String.IsNullOrEmpty(stateAttribute.Value)) Then
                                Me._UploadMonitorStatus = Integer.Parse(stateAttribute.Value)

                                If (Me._UploadMonitorStatus.Value = 2) Then
                                    ' Upload success
                                    ' Get the file name

                                    Me._SourceImageClientFileName = uploadMonitorNode.FirstChild.Value
                                End If

                                ' Image upload (success / error)
                                Return True
                            End If
                        End If
                    End If
                End If
            Case "edit"
                ' Image edit
                Return True
            Case "remove"
                ' Image remove
                Return True
            Case "configuration"
                ' Selected confiugration index changed
                Return True
        End Select

        ' No event
        Return False
    End Function

    Protected Sub RaisePostDataChangedEvent() Implements System.Web.UI.IPostBackDataHandler.RaisePostDataChangedEvent
        ' Process the control events

        Select Case Me._LastAct
            Case "upload"
                ' Image upload (success / error)
                If (Me._UploadMonitorStatus.HasValue) Then
                    Select Case Me._UploadMonitorStatus.Value
                        Case 2
                            ' Upload success
                            Me.ProcessUploadSuccess()
                        Case 3
                            ' Upload error
                            Me.ProcessUploadError()
                    End Select
                End If
            Case "edit"
                ' Image edit
                Me.ProcessEdit()
            Case "remove"
                ' Image remove
                Me.ProcessRemove()
            Case "configuration"
                ' Selected confiugration index changed
                Me.ProcessSelectedConfigurationIndexChanged()
        End Select
    End Sub

#End Region

#Region "Properties"

#Region "Settings"

#Region "Appearance"

    Protected _Width As Unit = Unit.Empty
    ''' <summary>
    ''' Gets or sets the control width.</summary>
    Public Property Width() As Unit
        Get
            Return Me._Width
        End Get
        Set(ByVal value As Unit)
            Me._Width = value
        End Set
    End Property

    Protected _BackColor As Color = ColorTranslator.FromHtml(DefaultValues_BackColor)
    ''' <summary>
    ''' Gets or sets the background color of the image upload control.</summary>
    <DefaultValue(GetType(Color), DefaultValues_BackColor)> _
    <TypeConverter(GetType(WebColorConverter))> Public Property BackColor() As Color
        Get
            Return Me._BackColor
        End Get
        Set(ByVal value As Color)
            Me._BackColor = value
        End Set
    End Property

    Protected _BorderColor As Color = ColorTranslator.FromHtml(DefaultValues_BorderColor)
    ''' <summary>
    ''' Gets or sets the border color of the image upload control.</summary>
    <DefaultValue(GetType(Color), DefaultValues_BorderColor)> _
    <TypeConverter(GetType(WebColorConverter))> Public Property BorderColor() As Color
        Get
            Return Me._BorderColor
        End Get
        Set(ByVal value As Color)
            Me._BorderColor = value
        End Set
    End Property

    Protected _BorderStyle As BorderStyle = DefaultValues_BorderStyle
    ''' <summary>
    ''' Gets or sets the border style of the image upload control.</summary>
    Public Property BorderStyle() As BorderStyle
        Get
            Return Me._BorderStyle
        End Get
        Set(ByVal value As BorderStyle)
            Me._BorderStyle = value
        End Set
    End Property

    Protected _BorderWidth As Unit = DefaultValues_BorderWidth
    ''' <summary>
    ''' Gets or sets the border width of the image upload control.</summary>
    <DefaultValue(GetType(Unit), "1px")> Public Property BorderWidth() As Unit
        Get
            Return Me._BorderWidth
        End Get
        Set(ByVal value As Unit)
            Me._BorderWidth = value
        End Set
    End Property

    Protected _ContentBackColor As Color = ColorTranslator.FromHtml(DefaultValues_ContentBackColor)
    ''' <summary>
    ''' Gets or sets the background color of the content element.</summary>
    <DefaultValue(GetType(Color), DefaultValues_ContentBackColor)> _
    <TypeConverter(GetType(WebColorConverter))> Public Property ContentBackColor() As Color
        Get
            Return Me._ContentBackColor
        End Get
        Set(ByVal value As Color)
            Me._ContentBackColor = value
        End Set
    End Property

    Protected _ContentForeColor As Color = ColorTranslator.FromHtml(DefaultValues_ContentForeColor)
    ''' <summary>
    ''' Gets or sets the foreground color of the content element.</summary>
    <DefaultValue(GetType(Color), DefaultValues_ContentForeColor)> _
    <TypeConverter(GetType(WebColorConverter))> Public Property ContentForeColor() As Color
        Get
            Return Me._ContentForeColor
        End Get
        Set(ByVal value As Color)
            Me._ContentForeColor = value
        End Set
    End Property

    Protected _ContentErrorForeColor As Color = ColorTranslator.FromHtml(DefaultValues_ContentErrorForeColor)
    ''' <summary>
    ''' Gets or sets the foreground color of the content element when an error is displayed.</summary>
    <DefaultValue(GetType(Color), DefaultValues_ContentErrorForeColor)> _
    <TypeConverter(GetType(WebColorConverter))> Public Property ContentErrorForeColor() As Color
        Get
            Return Me._ContentErrorForeColor
        End Get
        Set(ByVal value As Color)
            Me._ContentErrorForeColor = value
        End Set
    End Property

    Protected _ContentBorderColor As Color = ColorTranslator.FromHtml(DefaultValues_ContentBorderColor)
    ''' <summary>
    ''' Gets or sets the border color of the content element.</summary>
    <DefaultValue(GetType(Color), DefaultValues_ContentBorderColor)> _
    <TypeConverter(GetType(WebColorConverter))> Public Property ContentBorderColor() As Color
        Get
            Return Me._ContentBorderColor
        End Get
        Set(ByVal value As Color)
            Me._ContentBorderColor = value
        End Set
    End Property

    Protected _ContentBorderStyle As BorderStyle = DefaultValues_ContentBorderStyle
    ''' <summary>
    ''' Gets or sets the border style of the content element.</summary>
    Public Property ContentBorderStyle() As BorderStyle
        Get
            Return Me._ContentBorderStyle
        End Get
        Set(ByVal value As BorderStyle)
            Me._ContentBorderStyle = value
        End Set
    End Property

    Protected _ContentBorderWidth As Unit = DefaultValues_ContentBorderWidth
    ''' <summary>
    ''' Gets or sets the border width of the content element.</summary>
    <DefaultValue(GetType(Unit), "1px")> Public Property ContentBorderWidth() As Unit
        Get
            Return Me._ContentBorderWidth
        End Get
        Set(ByVal value As Unit)
            Me._ContentBorderWidth = value
        End Set
    End Property

    Protected _PreviewBorderColor As Color = ColorTranslator.FromHtml(DefaultValues_PreviewBorderColor)
    ''' <summary>
    ''' Gets or sets the border color of the preview image element.</summary>
    <DefaultValue(GetType(Color), DefaultValues_PreviewBorderColor)> _
    <TypeConverter(GetType(WebColorConverter))> Public Property PreviewBorderColor() As Color
        Get
            Return Me._PreviewBorderColor
        End Get
        Set(ByVal value As Color)
            Me._PreviewBorderColor = value
        End Set
    End Property

    Protected _PreviewBorderStyle As BorderStyle = DefaultValues_PreviewBorderStyle
    ''' <summary>
    ''' Gets or sets the border style of the preview image element.</summary>
    Public Property PreviewBorderStyle() As BorderStyle
        Get
            Return Me._PreviewBorderStyle
        End Get
        Set(ByVal value As BorderStyle)
            Me._PreviewBorderStyle = value
        End Set
    End Property

    Protected _PreviewBorderWidth As Unit = DefaultValues_PreviewBorderWidth
    ''' <summary>
    ''' Gets or sets the border width of the preview image element.</summary>
    <DefaultValue(GetType(Unit), "1px")> Public Property PreviewBorderWidth() As Unit
        Get
            Return Me._PreviewBorderWidth
        End Get
        Set(ByVal value As Unit)
            Me._PreviewBorderWidth = value
        End Set
    End Property

#End Region

#Region "Misc"

    Protected _OutputResolution As Single = CommonData.DefaultResolution
    ''' <summary>
    ''' Gets or sets the output resolution (DPI - default value = 96).</summary>
    Public Property OutputResolution() As Single
        Get
            Return Me._OutputResolution
        End Get
        Set(ByVal value As Single)
            If (Me.HasImage) Then
                Throw New Exception("Cannot change the OutputResolution after an image has been loaded.")
            End If

            ' Validate the new resolution
            CodeCarvings.Piczard.Helpers.ImageHelper.ValidateResolution(value, True)

            Me._OutputResolution = value
        End Set
    End Property

    Protected _CropConstraint As CropConstraint = Nothing
    ''' <summary>
    ''' Gets or sets the crop constraint.</summary>
    Public Property CropConstraint() As CropConstraint
        Get
            Return Me._CropConstraint
        End Get
        Set(ByVal value As CropConstraint)
            If (Me.HasImage) Then
                Throw New Exception("Cannot change the CropConstraint after an image has been loaded.")
            End If
            Me._CropConstraint = value
        End Set
    End Property

    Protected _ImageUploadPreProcessingFilter As ImageProcessingFilter = Nothing
    ''' <summary>
    ''' Gets or sets the filter(s) to apply after a new upload, before the image is loaded into the control.</summary>
    Public Property ImageUploadPreProcessingFilter() As ImageProcessingFilter
        Get
            Return Me._ImageUploadPreProcessingFilter
        End Get
        Set(ByVal value As ImageProcessingFilter)
            Me._ImageUploadPreProcessingFilter = value
        End Set
    End Property

    Protected _PostProcessingFilter As ImageProcessingFilter = Nothing
    ''' <summary>
    ''' Gets or sets the filter(s) to apply to the image.</summary>
    Public Property PostProcessingFilter() As ImageProcessingFilter
        Get
            Return Me._PostProcessingFilter
        End Get
        Set(ByVal value As ImageProcessingFilter)
            If (Me.HasImage) Then
                Throw New Exception("Cannot change the PostProcessingFilter after an image has been loaded.")
            End If
            Me._PostProcessingFilter = value
        End Set
    End Property

    Protected _PostProcessingFilterApplyMode As SimpleImageUploadPostProcessingFilterApplyMode = SimpleImageUploadPostProcessingFilterApplyMode.OnlyNewImages
    ''' <summary>
    ''' Gets or sets a value indicating if and when ImageProcessingFilter must be applied.
    ''' Default values is: "OnlyNewImages".</summary>
    Public Property PostProcessingFilterApplyMode() As SimpleImageUploadPostProcessingFilterApplyMode
        Get
            Return Me._PostProcessingFilterApplyMode
        End Get
        Set(ByVal value As SimpleImageUploadPostProcessingFilterApplyMode)
            If (Me.HasImage) Then
                Throw New Exception("Cannot change PostProcessingFilterApplyMode after an image has been loaded.")
            End If
            Me._PostProcessingFilterApplyMode = value
        End Set
    End Property

    Protected _PreviewFilter As ImageProcessingFilter = Nothing
    ''' <summary>
    ''' Gets or sets the filter(s) to apply to the preview image.</summary>
    Public Property PreviewFilter() As ImageProcessingFilter
        Get
            Return Me._PreviewFilter
        End Get
        Set(ByVal value As ImageProcessingFilter)
            If (Me.HasImage) Then
                Throw New Exception("Cannot change the PreviewFilter after an image has been loaded.")
            End If
            Me._PreviewFilter = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the ResizeConstraint used to generate the preview image.
    ''' This property is marked as obsolete since version 2.0.0 of the control and will be soon removed.
    ''' Use 'PreviewFilter' instead.</summary>
    <Obsolete()> _
    Public Property PreviewResizeConstraint() As ResizeConstraint
        Get
            Return DirectCast(Me.PreviewFilter, ResizeConstraint)
        End Get
        Set(ByVal value As ResizeConstraint)
            Me.PreviewFilter = value
        End Set
    End Property

    Protected _ImageEditPopupSize As Size = DefaultImageEditPopupSize
    ''' <summary>
    ''' Gets or sets the image edit popup size.</summary>
    Public Property ImageEditPopupSize() As Size
        Get
            Return Me._ImageEditPopupSize
        End Get
        Set(ByVal value As Size)
            Me._ImageEditPopupSize = value
        End Set
    End Property

    Protected _AutoOpenImageEditPopupAfterUpload As Boolean = False
    ''' <summary>
    ''' Gets or sets a value indicating whether to automatically open the image edit popup after the upload process.</summary>
    Public Property AutoOpenImageEditPopupAfterUpload() As Boolean
        Get
            Return Me._AutoOpenImageEditPopupAfterUpload
        End Get
        Set(ByVal value As Boolean)
            Me._AutoOpenImageEditPopupAfterUpload = value
        End Set
    End Property

    Protected _AutoDisableImageEdit As Boolean = True
    ''' <summary>
    ''' Gets or sets a value indicating whether to automatically disable image edit feature if not available (e.g. Flash Player not installed).</summary>
    Public Property AutoDisableImageEdit() As Boolean
        Get
            Return Me._AutoDisableImageEdit
        End Get
        Set(ByVal value As Boolean)
            Me._AutoDisableImageEdit = value
        End Set
    End Property

    Protected _ButtonSize As Size = DefaultButtonSize
    ''' <summary>
    ''' Gets or sets the size of the buttons.</summary>
    Public Property ButtonSize() As Size
        Get
            Return Me._ButtonSize
        End Get
        Set(ByVal value As Size)
            Me._ButtonSize = value
        End Set
    End Property

    Protected _CssClass As String = String.Empty
    ''' <summary>
    ''' Gets or sets the Cascading Style Sheet (CSS) class rendered by the user control on the client.</summary>
    Public Property CssClass() As String
        Get
            Return Me._CssClass
        End Get
        Set(ByVal value As String)
            Me._CssClass = value
        End Set
    End Property

    Protected _Configurations As String() = Nothing
    ''' <summary>
    ''' Gets or sets the available configuration names.</summary>
    Public Property Configurations() As String()
        Get
            Return Me._Configurations
        End Get
        Set(ByVal value As String())
            Me._Configurations = value
        End Set
    End Property

    Protected _SelectedConfigurationIndex As Integer? = Nothing
    ''' <summary>
    ''' Gets or sets the index of the selected configuration.</summary>
    Public Property SelectedConfigurationIndex() As Integer?
        Get
            If (Me._Configurations Is Nothing) Then
                ' No configuration available
                Return Nothing
            End If
            If (Me._Configurations.Length = 0) Then
                ' No configuration available
                Return Nothing
            End If

            If (Not Me._SelectedConfigurationIndex.HasValue) Then
                ' First configuration selected by default
                Return 0
            Else
                If (Me._SelectedConfigurationIndex.Value >= Me._Configurations.Length) Then
                    ' Use the last configuration available
                    Return Me._Configurations.Length - 1
                End If
            End If

            Return Me._SelectedConfigurationIndex
        End Get
        Set(ByVal value As Integer?)
            If (value.HasValue) Then
                If (Me._Configurations Is Nothing) Then
                    ' No configuration available
                    Throw New Exception("Cannot set the SelectedConfigurationIndex because no configuration has been set yet.")
                End If
                If (Me._Configurations.Length = 0) Then
                    ' No configuration available
                    Throw New Exception("Cannot set the SelectedConfigurationIndex because there is no configuration set.")
                End If

                If (value.Value < 0) Then
                    Throw New Exception("SelectedConfigurationIndex cannot be < 0.")
                End If
                If (value.Value >= Me._Configurations.Length) Then
                    Throw New Exception("SelectedConfigurationIndex must be < Configurations.length.")
                End If
            End If

            Me._SelectedConfigurationIndex = value
        End Set
    End Property

    Protected _DebugUploadProblems As Boolean = False
    ''' <summary>
    ''' Gets or sets a value indicating whether to show details when an upload error occurs.</summary>
    Public Property DebugUploadProblems() As Boolean
        Get
            Return Me._DebugUploadProblems
        End Get
        Set(ByVal value As Boolean)
            Me._DebugUploadProblems = value
        End Set
    End Property

    Protected _PictureTrimmerSettings As PopupPictureTrimmerSettingsProvider = Nothing
    ''' <summary>
    '''  Gets an object that allows to customize settings of the PopupPictureTrimmer instance.</summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    <NotifyParentProperty(True)> Public ReadOnly Property PictureTrimmerSettings() As PopupPictureTrimmerSettingsProvider
        Get
            If (Me._PictureTrimmerSettings Is Nothing) Then
                Me._PictureTrimmerSettings = New PopupPictureTrimmerSettingsProvider(Me.popupPictureTrimmer1)
            End If
            Return Me._PictureTrimmerSettings
        End Get
    End Property

#End Region

#Region "Globalization"

#Region "UI elements"

    Protected _Text_EditButton As String = "Edit..."
    ''' <summary>
    ''' Gets or sets the text of the "Edit" button.</summary>
    Public Property Text_EditButton() As String
        Get
            Return Me._Text_EditButton
        End Get
        Set(ByVal value As String)
            Me._Text_EditButton = value
        End Set
    End Property

    Protected _Text_RemoveButton As String = "Remove"
    ''' <summary>
    ''' Gets or sets the text of the "Remove" button.</summary>
    Public Property Text_RemoveButton() As String
        Get
            Return Me._Text_RemoveButton
        End Get
        Set(ByVal value As String)
            Me._Text_RemoveButton = value
        End Set
    End Property

    Protected _Text_BrowseButton As String = "Browse..."
    ''' <summary>
    ''' Gets or sets the text of the "Browse" button.</summary>
    Public Property Text_BrowseButton() As String
        Get
            Return Me._Text_BrowseButton
        End Get
        Set(ByVal value As String)
            Me._Text_BrowseButton = value
        End Set
    End Property

    Protected _Text_CancelUploadButton As String = "Cancel upload"
    ''' <summary>
    ''' Gets or sets the text of the "Cancel Upload" button.</summary>
    Public Property Text_CancelUploadButton() As String
        Get
            Return Me._Text_CancelUploadButton
        End Get
        Set(ByVal value As String)
            Me._Text_CancelUploadButton = value
        End Set
    End Property

    Protected _Text_ConfigurationLabel As String = "Configuration:"
    ''' <summary>
    ''' Gets or sets the text of the "Configuration" label.</summary>
    Public Property Text_ConfigurationLabel() As String
        Get
            Return Me._Text_ConfigurationLabel
        End Get
        Set(ByVal value As String)
            Me._Text_ConfigurationLabel = value
        End Set
    End Property

#End Region

#Region "Status messages"

    Protected _CurrentStatusMessage As String = Nothing
    Protected Function GetCurrentStatusMessage() As String
        If (Me._CurrentStatusMessage IsNot Nothing) Then
            ' Last status set
            Return Me._CurrentStatusMessage
        End If

        ' By default return the "No image selected" text.
        Return Me.StatusMessage_NoImageSelected
    End Function

    ''' <summary>
    ''' Sets the current status message.</summary>
    ''' <param name="text">The message to display.</param>
    ''' <param name="isError">If true, the message will be displayed as error message.</param>
    Public Sub SetCurrentStatusMessage(ByVal text As String, ByVal isError As Boolean)
        If (isError) Then
            If (Me._ContentErrorForeColor <> Color.Empty) Then
                text = "<span style=""color:" + ColorTranslator.ToHtml(Me._ContentErrorForeColor) + ";"">" + text + "</span>"
            End If
        End If
        Me._CurrentStatusMessage = text
    End Sub

    ''' <summary>
    ''' Sets the current status message.</summary>
    ''' <param name="text">The message to display.</param>
    Public Sub SetCurrentStatusMessage(ByVal text As String)
        Me.SetCurrentStatusMessage(text, False)
    End Sub

    Protected _StatusMessage_NoImageSelected As String = "No image selected."
    ''' <summary>
    ''' Gets or sets the text displayed when no image has been selected.</summary>
    Public Property StatusMessage_NoImageSelected() As String
        Get
            Return Me._StatusMessage_NoImageSelected
        End Get
        Set(ByVal value As String)
            Me._StatusMessage_NoImageSelected = value
        End Set
    End Property

    Protected _StatusMessage_UploadError As String = "A server error has occurred during the upload process.<br/>Please ensure that the file is smaller than {0} KBytes."
    ''' <summary>
    ''' Gets or sets the text displayed when a (generic) upload error has occurred.</summary>
    Public Property StatusMessage_UploadError() As String
        Get
            Return String.Format(Me._StatusMessage_UploadError, Me.MaxRequestLength)
        End Get
        Set(ByVal value As String)
            Me._StatusMessage_UploadError = value
        End Set
    End Property

    Protected _StatusMessage_InvalidImage As String = "The uploaded file is not a valid image."
    ''' <summary>
    ''' Gets or sets the text displayed when the uploaded image file is invalid.</summary>
    Public Property StatusMessage_InvalidImage() As String
        Get
            Return Me._StatusMessage_InvalidImage
        End Get
        Set(ByVal value As String)
            Me._StatusMessage_InvalidImage = value
        End Set
    End Property

    Protected _StatusMessage_InvalidImageSize As String = "The uploaded image is not valid (too small or too large)."
    ''' <summary>
    ''' Gets or sets the text displayed when the size of the uploaded image is too small or too large (please see: CodeCarvings.Piczard.Configuration.DrawingSettings.MaxImageSize).</summary>
    Public Property StatusMessage_InvalidImageSize() As String
        Get
            Return Me._StatusMessage_InvalidImageSize
        End Get
        Set(ByVal value As String)
            Me._StatusMessage_InvalidImageSize = value
        End Set
    End Property

    Protected _StatusMessage_Wait As String = "Please wait..."
    ''' <summary>
    ''' Gets or sets the text displayed when the user has to wait (e.g. a postback has been delayed).</summary>
    Public Property StatusMessage_Wait() As String
        Get
            Return Me._StatusMessage_Wait
        End Get
        Set(ByVal value As String)
            Me._StatusMessage_Wait = value
        End Set
    End Property

#End Region

#End Region

#Region "Features"

    Protected _EnableEdit As Boolean = True
    ''' <summary>
    ''' Gets or sets a value indicating whether it's possible to edit the image.</summary>
    Public Property EnableEdit() As Boolean
        Get
            Return Me._EnableEdit
        End Get
        Set(ByVal value As Boolean)
            Me._EnableEdit = value
        End Set
    End Property

    Protected _EnableRemove As Boolean = True
    ''' <summary>
    ''' Gets or sets a value indicating whether it's possible to remove the image.</summary>
    Public Property EnableRemove() As Boolean
        Get
            Return Me._EnableRemove
        End Get
        Set(ByVal value As Boolean)
            Me._EnableRemove = value
        End Set
    End Property

    Protected _EnableUpload As Boolean = True
    ''' <summary>
    ''' Gets or sets a value indicating whether it's possible to upload an image.</summary>
    Public Property EnableUpload() As Boolean
        Get
            Return Me._EnableUpload
        End Get
        Set(ByVal value As Boolean)
            Me._EnableUpload = value
        End Set
    End Property

    Protected _EnableCancelUpload As Boolean = True
    ''' <summary>
    ''' Gets or sets a value indicating whether it's possible to cancel an upload in progess.</summary>
    Public Property EnableCancelUpload() As Boolean
        Get
            Return Me._EnableCancelUpload
        End Get
        Set(ByVal value As Boolean)
            Me._EnableCancelUpload = value
        End Set
    End Property

#End Region

#End Region

#Region "Current state"

    Protected _TemporaryFileId As String = Nothing
    ''' <summary>
    ''' Gets or sets the current temporary file id.</summary>
    Protected Property TemporaryFileId() As String
        Get
            If (Me._TemporaryFileId Is Nothing) Then
                ' Get e new temporary file id
                Me._TemporaryFileId = TemporaryFileManager.GetNewTemporaryFileId()
            End If

            Return Me._TemporaryFileId
        End Get
        Set(ByVal value As String)
            ' Validate the value
            If (String.IsNullOrEmpty(value)) Then
                Throw New Exception("Invalid TemporaryFileId (null).")
            End If
            If (Not TemporaryFileManager.ValidateTemporaryFileId(value, False)) Then
                Throw New Exception("Invalid TemporaryFileId.")
            End If

            Me._TemporaryFileId = value
        End Set
    End Property

    Protected _RenderId As String = Nothing
    Protected ReadOnly Property RenderId() As String
        Get
            If (Me._RenderId Is Nothing) Then
                Me._RenderId = Guid.NewGuid().ToString("N")
            End If
            Return Me._RenderId
        End Get
    End Property

    Protected ReadOnly Property InitFunctionName() As String
        Get
            Return "CCPZ_SIU_" + Me.RenderId + "_Init"
        End Get
    End Property

    Protected ReadOnly Property InitFunctionName2() As String
        Get
            Return "CCPZ_SIU_" + Me.RenderId + "_Init2"
        End Get
    End Property

    ' If true, the control must regenerate the preview image (in the prerender method)
    Protected _UpdatePreview As Boolean = False
    Protected _LastAct As String = ""
    Protected _UploadMonitorStatus As Integer? = Nothing
    Protected _AutoOpenImageEditPopup As Boolean = False

    ''' <summary>
    ''' Gets a value indicating whether the control has an image.</summary>
    Public ReadOnly Property HasImage() As Boolean
        Get
            Return Me.popupPictureTrimmer1.ImageLoaded
        End Get
    End Property

    ''' <summary>
    ''' Gets a value indicating whether the control has an image uploaded or edited by the user.</summary>
    Public ReadOnly Property HasNewImage() As Boolean
        Get
            If (Me.HasImage) Then
                If (Me.ImageUploaded) Then
                    Return True
                End If

                If (Me.ImageEdited) Then
                    Return True
                End If
            End If

            ' Image not loaded or already persisted
            Return False
        End Get
    End Property

    Protected _ImageUploaded As Boolean = False
    ''' <summary>
    ''' Gets a value indicating whether the control has an image uploaded by the user.</summary>
    Public ReadOnly Property ImageUploaded() As Boolean
        Get
            If (Not Me.HasImage) Then
                Return False
            End If

            Return Me._ImageUploaded
        End Get
    End Property

    Protected _ImageEdited As Boolean = False
    ''' <summary>
    ''' Gets a value indicating whether the control has an image edited by the user.</summary>
    Public ReadOnly Property ImageEdited() As Boolean
        Get
            If (Not Me.HasImage) Then
                Return False
            End If

            Return Me._ImageEdited
        End Get
    End Property

    Protected _SourceImageClientFileName As String = Nothing
    ''' <summary>
    ''' Gets or sets the original image file name (before upload).</summary>
    Public Property SourceImageClientFileName() As String
        Get
            Return Me._SourceImageClientFileName
        End Get
        Set(ByVal value As String)
            If (value IsNot Nothing) Then
                ' Get the file name
                value = Path.GetFileName(value)
            End If
            Me._SourceImageClientFileName = value
        End Set
    End Property

#End Region

#Region "PictureTrimmer properties"

    ''' <summary>
    ''' Gets the current PictureTrimmerUserState of the PictureTrimmer control.</summary>
    Public ReadOnly Property UserState() As PictureTrimmerUserState
        Get
            If (Not Me.HasImage) Then
                Return Nothing
            End If

            Return Me.popupPictureTrimmer1.UserState
        End Get
    End Property

    ''' <summary>
    ''' Gets the current PictureTrimmerValue of the PictureTrimmer control.</summary>
    Public ReadOnly Property Value() As PictureTrimmerValue
        Get
            If (Not Me.HasImage) Then
                Return Nothing
            End If

            Return Me.popupPictureTrimmer1.Value
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the current culture.</summary>
    Public Property Culture() As String
        Get
            Return Me.popupPictureTrimmer1.Culture
        End Get
        Set(ByVal value As String)
            Me.popupPictureTrimmer1.Culture = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the CanvasColor used by the PictureTrimmer" control.</summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    <NotifyParentProperty(True)> Public Property CanvasColor() As BackgroundColor
        Get
            Return Me.popupPictureTrimmer1.CanvasColor
        End Get
        Set(ByVal value As BackgroundColor)
            Me.popupPictureTrimmer1.CanvasColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the ImageBackColor used by the PictureTrimmer" control.</summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    <NotifyParentProperty(True)> Public Property ImageBackColor() As BackgroundColor
        Get
            Return Me.popupPictureTrimmer1.ImageBackColor
        End Get
        Set(ByVal value As BackgroundColor)
            Me.popupPictureTrimmer1.ImageBackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the ImageBackColorApplyMode property of the PictureTrimmer control.</summary>
    Public Property ImageBackColorApplyMode() As PictureTrimmerImageBackColorApplyMode
        Get
            Return Me.popupPictureTrimmer1.ImageBackColorApplyMode
        End Get
        Set(ByVal value As PictureTrimmerImageBackColorApplyMode)
            Me.popupPictureTrimmer1.ImageBackColorApplyMode = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the UIUnit used by the PictureTrimmer control.</summary>
    Public Property UIUnit() As GfxUnit
        Get
            Return Me.popupPictureTrimmer1.UIUnit
        End Get
        Set(ByVal value As GfxUnit)
            Me.popupPictureTrimmer1.UIUnit = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a value indicating how the PictureTrimmer GUI renders the cropping mask.
    ''' This property is marked as obsolete since version 3.0.0 of the control and will be soon removed.
    ''' Use 'PictureTrimmerSettings-CropShadowMode' instead.</summary>
    <Obsolete()> Public Property CropShadowMode() As PictureTrimmerCropShadowMode
        Get
            Return Me.popupPictureTrimmer1.CropShadowMode
        End Get
        Set(ByVal value As PictureTrimmerCropShadowMode)
            Me.popupPictureTrimmer1.CropShadowMode = value
        End Set
    End Property

#Region "Read-only properties"

    ''' <summary>
    ''' Gets the size (pixel) of the source image.</summary>
    Public ReadOnly Property SourceImageSize() As Size
        Get
            If (Not Me.HasImage) Then
                Throw New Exception("Image not loaded.")
            End If

            Return Me.popupPictureTrimmer1.SourceImageSize
        End Get
    End Property

    ''' <summary>
    ''' Gets the resolution (DPI) of the source image.</summary>
    Public ReadOnly Property SourceImageResolution() As Single
        Get
            If (Not Me.HasImage) Then
                Throw New Exception("Image not loaded.")
            End If

            Return Me.popupPictureTrimmer1.SourceImageResolution
        End Get
    End Property

    ''' <summary>
    ''' Gets the format of the source image</summary>
    Public ReadOnly Property SourceImageFormatId() As Guid
        Get
            If (Not Me.HasImage) Then
                Throw New Exception("Image not loaded.")
            End If

            Return Me.popupPictureTrimmer1.SourceImageFormatId
        End Get
    End Property

#End Region

#End Region

#Region "File paths / urls"

    ''' <summary>
    ''' Gets the path of the temporary file containing the source image.</summary>
    Public ReadOnly Property TemporarySourceImageFilePath() As String
        Get
            Return TemporaryFileManager.GetTemporaryFilePath(Me.TemporaryFileId, "_s.tmp")
        End Get
    End Property

    Protected ReadOnly Property UploadFilePath() As String
        Get
            Return TemporaryFileManager.GetTemporaryFilePath(Me.TemporaryFileId, "_u.tmp")
        End Get
    End Property

    Protected ReadOnly Property PreviewImageFilePath() As String
        Get
            Return TemporaryFileManager.GetTemporaryFilePath(Me.TemporaryFileId, "_p.jpg")
        End Get
    End Property

    Protected ReadOnly Property TemporaryWriteTestFilePath() As String
        Get
            Return TemporaryFileManager.GetTemporaryFilePath(Me.TemporaryFileId, "_wt.tmp")
        End Get
    End Property

    Protected ReadOnly Property UploadMonitorFilePath() As String
        Get
            Return TemporaryFileManager.GetTemporaryFilePath(Me.TemporaryFileId, "_um.xml")
        End Get
    End Property

    Protected ReadOnly Property PreviewImageUrl() As String
        Get
            Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder()
            sb.Append(Me.ResolveUrl("preview.ashx"))
            sb.Append("?tfid=" + HttpUtility.UrlEncode(Me.TemporaryFileId.ToString()))
            sb.Append("&k=" + HttpUtility.UrlEncode(Me.GetQueryKey("")))
            sb.Append("&ts=" + HttpUtility.UrlEncode(DateTime.UtcNow.Ticks.ToString()))

            Return sb.ToString()
        End Get
    End Property

    Protected ReadOnly Property UploadUrl() As String
        Get
            Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder()
            sb.Append(Me.ResolveUrl("upload.aspx"))
            sb.Append("?tfid=" + HttpUtility.UrlEncode(Me.TemporaryFileId.ToString()))
            Dim keyAdditionalData As String = "dup=" + If(Me._DebugUploadProblems, "1", "0")
            sb.Append("&" + keyAdditionalData)
            sb.Append("&k=" + HttpUtility.UrlEncode(Me.GetQueryKey(keyAdditionalData)))
            sb.Append("&cid=" + HttpUtility.UrlEncode(Me.ClientID))
            sb.Append("&bsw=" + HttpUtility.UrlEncode(Me.ButtonSize.Width.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            sb.Append("&bsh=" + HttpUtility.UrlEncode(Me.ButtonSize.Height.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            sb.Append("&ts=" + HttpUtility.UrlEncode(DateTime.UtcNow.Ticks.ToString()))

            Return sb.ToString()
        End Get
    End Property

    Protected ReadOnly Property UploadMonitorUrl() As String
        Get
            Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder()
            sb.Append(Me.ResolveUrl("uploadMonitor.ashx"))
            sb.Append("?tfid=" + HttpUtility.UrlEncode(Me.TemporaryFileId.ToString()))
            sb.Append("&k=" + HttpUtility.UrlEncode(Me.GetQueryKey("")))

            Return sb.ToString()
        End Get
    End Property

    Protected ReadOnly Property MaxRequestLength() As String
        Get
            ' Default value = 4 MB
            Dim result As String = "4096"

            Try
                Dim section As System.Web.Configuration.HttpRuntimeSection = DirectCast(System.Configuration.ConfigurationManager.GetSection("system.web/httpRuntime"), System.Web.Configuration.HttpRuntimeSection)
                If (section IsNot Nothing) Then
                    result = section.MaxRequestLength.ToString(System.Globalization.CultureInfo.InvariantCulture)
                End If
            Catch
                ' An error has occurred (Medium trust ?)
                result = "?"
            End Try

            Return result
        End Get
    End Property

#End Region

#End Region

#Region "Events"

    ''' <summary>
    ''' Represents the method that handles the ImageUpload event.</summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="args">The argument of type ImageUploadEventArgs that contains the event data.</param>    
    Public Delegate Sub ImageUploadEventHandler(ByVal sender As Object, ByVal args As ImageUploadEventArgs)

    ''' <summary>
    ''' Occurs when a new image is uploaded.</summary>
    Public Event ImageUpload As ImageUploadEventHandler
    Protected Sub OnImageUpload(ByVal e As ImageUploadEventArgs)
        RaiseEvent ImageUpload(Me, e)
    End Sub

    ''' <summary>
    ''' Occurs when an upload process does not complete successfully.</summary>
    Public Event UploadError As EventHandler
    Protected Sub OnUploadError(ByVal e As EventArgs)
        RaiseEvent UploadError(Me, e)
    End Sub

    ''' <summary>
    ''' Occurs when the image is edited.</summary>
    Public Event ImageEdit As EventHandler
    Protected Sub OnImageEdit(ByVal e As EventArgs)
        RaiseEvent ImageEdit(Me, e)
    End Sub

    ''' <summary>
    ''' Occurs when the image is removed.</summary>
    Public Event ImageRemove As EventHandler
    Protected Sub OnImageRemove(ByVal e As EventArgs)
        RaiseEvent ImageRemove(Me, e)
    End Sub

    ''' <summary>
    ''' Represents the method that handles the SelectedConfigurationIndexChanged event.</summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="args">The argument of type SelectedConfigurationIndexChangedEventArgs that contains the event data.</param>
    Public Delegate Sub SelectedConfigurationIndexChangedEventHandler(ByVal sender As Object, ByVal args As SelectedConfigurationIndexChangedEventArgs)

    ''' <summary>
    ''' Occurs when the ConfigurationIndex has been changed by the user.</summary>
    Public Event SelectedConfigurationIndexChanged As SelectedConfigurationIndexChangedEventHandler
    Protected Sub OnSelectedConfigurationIndexChanged(ByVal e As SelectedConfigurationIndexChangedEventArgs)
        RaiseEvent SelectedConfigurationIndexChanged(Me, e)
    End Sub

#End Region

#Region "Methods"

#Region "Load"

    Protected Sub LoadImageFromFileSystem_Internal(ByVal sourceImageFilePath As String, ByVal sourceImage As System.Drawing.Image, ByVal sourceImageSize As Size, ByVal sourceImageFormatId As Guid, ByVal sourceImageResolution As Single, ByVal disposeSourceImage As Boolean, ByVal value As PictureTrimmerValue)
        ' Calculate the client file name
        Me.SourceImageClientFileName = "noname" + ImageArchiver.GetFormatEncoderParams(sourceImageFormatId).FileExtension

        If (CodeCarvings.Piczard.Configuration.WebSettings.PictureTrimmer.UseTemporaryFiles) Then
            ' The picture trimmer can use temporary files -> Load the image now
            ' This generates a new temporary files, however saves CPU and RAM
            Me.popupPictureTrimmer1.LoadImage(sourceImage, Me._OutputResolution, Me._CropConstraint)
        End If
        If (disposeSourceImage) Then
            ' The source image is no longer necessary
            sourceImage.Dispose()
            sourceImage = Nothing
        End If

        If (Not CodeCarvings.Piczard.Configuration.WebSettings.PictureTrimmer.UseTemporaryFiles) Then
            ' The picture trimmer cannot use temporary files -> Load the image now
            Me.popupPictureTrimmer1.SetLoadImageData_ImageSize(sourceImageSize)
            Me.popupPictureTrimmer1.SetLoadImageData_ImageResolution(sourceImageResolution)
            Me.popupPictureTrimmer1.SetLoadImageData_ImageFormatId(sourceImageFormatId)

            Me.popupPictureTrimmer1.LoadImageFromFileSystem(sourceImageFilePath, Me._OutputResolution, Me._CropConstraint)
        End If

        If (value IsNot Nothing) Then
            ' Optional: Set the picture trimmer value
            Me.popupPictureTrimmer1.Value = value
        End If

        ' The new image has been loaded
        Me._ImageUploaded = False
        Me._ImageEdited = False

        ' Update the preview
        Me._UpdatePreview = True
    End Sub

    Protected Sub LoadImageFromFileSystem_Internal(ByVal sourceImageFilePath As String, ByVal value As PictureTrimmerValue)
        Using image As LoadedImage = ImageArchiver.LoadImage(sourceImageFilePath)
            Me.LoadImageFromFileSystem_Internal(sourceImageFilePath, image.Image, image.Size, image.FormatId, image.Resolution, True, value)
        End Using
    End Sub

    ''' <summary>
    ''' Loads an image stored in the file system and applies a specific PictureTrimmerValue.</summary>
    ''' <param name="sourceImageFilePath">The path of the image to load.</param>
    ''' <param name="value">The PictureTrimmerValue to apply.</param>
    Public Sub LoadImageFromFileSystem(ByVal sourceImageFilePath As String, ByVal value As PictureTrimmerValue)
        ' Translate path to absolute
        sourceImageFilePath = CodeCarvings.Piczard.Helpers.IOHelper.TranslatePathToAbsolute(sourceImageFilePath)

        ' Copy the source image into the temporary folder
        ' So there is no problem il the original source image is deleted (e.g. when a record is updated...)
        System.IO.File.Copy(sourceImageFilePath, Me.TemporarySourceImageFilePath, True)

        ' Load the image
        Me.LoadImageFromFileSystem_Internal(Me.TemporarySourceImageFilePath, value)

        ' Use the original file name as source client file name
        Me.SourceImageClientFileName = sourceImageFilePath
    End Sub

    ''' <summary>
    ''' Loads an image stored in the file system and auto-calculates the PictureTrimmerValue to use.</summary>
    ''' <param name="sourceImageFilePath">The path of the image to load.</param>
    Public Sub LoadImageFromFileSystem(ByVal sourceImageFilePath As String)
        Me.LoadImageFromFileSystem(sourceImageFilePath, Nothing)
    End Sub

    ''' <summary>
    ''' Loads an image from a Stream and applies a specific PictureTrimmerValue.</summary>
    ''' <param name="sourceImageStream">The Stream containing the image to load.</param>
    ''' <param name="value">The PictureTrimmerValue to apply.</param>
    Public Sub LoadImageFromStream(ByVal sourceImageStream As Stream, ByVal value As PictureTrimmerValue)
        ' Save the stream
        Using writer As Stream = File.Create(Me.TemporarySourceImageFilePath)
            If (sourceImageStream.Position <> 0) Then
                sourceImageStream.Seek(0, SeekOrigin.Begin)
            End If

            Dim buffer(4095) As Byte
            Dim readBytes As Integer
            While (True)
                readBytes = sourceImageStream.Read(buffer, 0, buffer.Length)
                If (readBytes <= 0) Then
                    Exit While
                End If
                writer.Write(buffer, 0, readBytes)
            End While

            writer.Close()
        End Using

        ' Load the image from the temporary file
        Me.LoadImageFromFileSystem_Internal(Me.TemporarySourceImageFilePath, value)
    End Sub

    ''' <summary>
    ''' Loads an image from a Stream and auto-calculates the PictureTrimmerValue to use.</summary>
    ''' <param name="sourceImageStream">The Stream containing the image to load.</param>    
    Public Sub LoadImageFromStream(ByVal sourceImageStream As Stream)
        Me.LoadImageFromStream(sourceImageStream, Nothing)
    End Sub

    ''' <summary>
    ''' Loads an image from an array of bytes and applies a specific PictureTrimmerValue.</summary>
    ''' <param name="sourceImageBytes">The array of bytes to load.</param>
    ''' <param name="value">The PictureTrimmerValue to apply.</param>
    Public Sub LoadImageFromByteArray(ByVal sourceImageBytes As Byte(), ByVal value As PictureTrimmerValue)
        ' Save the byte array
        Using writer As Stream = File.Create(Me.TemporarySourceImageFilePath)
            writer.Write(sourceImageBytes, 0, sourceImageBytes.Length)
            writer.Close()
        End Using

        ' Load the image from the temporary file
        Me.LoadImageFromFileSystem_Internal(Me.TemporarySourceImageFilePath, value)
    End Sub

    ''' <summary>
    ''' Loads an image from an array of bytes and auto-calculates the PictureTrimmerValue to use.</summary>
    ''' <param name="sourceImageBytes">The array of bytes to load.</param>
    Public Sub LoadImageFromByteArray(ByVal sourceImageBytes As Byte())
        Me.LoadImageFromByteArray(sourceImageBytes, Nothing)
    End Sub

    ''' <summary>
    ''' Loads an image from an array of bytes and applies a specific PictureTrimmerValue.</summary>
    ''' <param name="sourceImage">The source image to load.</param>
    ''' <param name="value">The PictureTrimmerValue to apply.</param>
    Public Sub LoadImage(ByVal sourceImage As LoadedImage, ByVal value As PictureTrimmerValue)
        ' Save the image - Use PNG as image format to preserve transparency
        ImageArchiver.SaveImageToFileSystem(sourceImage.Image, Me.TemporarySourceImageFilePath, New PngFormatEncoderParams())

        ' Load the image into the control
        Me.LoadImageFromFileSystem_Internal(Me.TemporarySourceImageFilePath, sourceImage.Image, sourceImage.Size, sourceImage.FormatId, sourceImage.Resolution, False, value)
    End Sub

    ''' <summary>
    ''' Loads an image and auto-calculates the PictureTrimmerValue to use.</summary>
    ''' <param name="sourceImage">The source image to load.</param>
    Public Sub LoadImage(ByVal sourceImage As LoadedImage)
        Me.LoadImage(sourceImage, Nothing)
    End Sub

    ''' <summary>
    ''' Loads an image from an array of bytes and applies a specific PictureTrimmerValue.</summary>
    ''' <param name="sourceImage">The source image to load.</param>
    ''' <param name="value">The PictureTrimmerValue to apply.</param>
    Public Sub LoadImage(ByVal sourceImage As System.Drawing.Image, ByVal value As PictureTrimmerValue)
        ' Save the image - Use PNG as image format to preserve transparency
        ImageArchiver.SaveImageToFileSystem(sourceImage, Me.TemporarySourceImageFilePath, New PngFormatEncoderParams())

        ' Load the image into the control
        Me.LoadImageFromFileSystem_Internal(Me.TemporarySourceImageFilePath, sourceImage, sourceImage.Size, sourceImage.RawFormat.Guid, CodeCarvings.Piczard.Helpers.ImageHelper.GetImageResolution(sourceImage), False, value)
    End Sub

    ''' <summary>
    ''' Loads an image and auto-calculates the PictureTrimmerValue to use.</summary>
    ''' <param name="sourceImage">The source image to load.</param>
    Public Sub LoadImage(ByVal sourceImage As System.Drawing.Image)
        Me.LoadImage(sourceImage, Nothing)
    End Sub

    ''' <summary>
    ''' Unloads the current image.</summary>
    ''' <param name="clearTemporaryFiles">If true, delete the temporary files.</param>
    Public Sub UnloadImage(ByVal clearTemporaryFiles As Boolean)
        Me.popupPictureTrimmer1.UnloadImage()

        If (clearTemporaryFiles) Then
            ' Delete the temporary files
            Me.ClearTemporaryFiles()
        End If

        ' No image
        Me._ImageUploaded = False
        Me._ImageEdited = False
        Me._SourceImageClientFileName = Nothing
    End Sub

    ''' <summary>
    ''' Unloads the current image and clears the temporary files.</summary>
    Public Sub UnloadImage()
        Me.UnloadImage(True)
    End Sub

#End Region

#Region "Image Processing"

    ''' <summary>
    ''' Returns the ImageProcessingJob that can be used to process the source image.</summary>
    ''' <returns>An ImageProcessingJob ready to be used to process imagess.</returns>
    Public Function GetImageProcessingJob() As ImageProcessingJob
        Dim result As ImageProcessingJob = Me.popupPictureTrimmer1.GetImageProcessingJob()
        If (Not (Me.PostProcessingFilter Is Nothing)) Then
            ' Apply the post processing filter(s) is necessary
            Dim addPostProcessingFilter As Boolean = True
            Select Case Me._PostProcessingFilterApplyMode
                Case SimpleImageUploadPostProcessingFilterApplyMode.Never
                    addPostProcessingFilter = False
                Case SimpleImageUploadPostProcessingFilterApplyMode.OnlyNewImages
                    addPostProcessingFilter = Me.HasNewImage
                Case SimpleImageUploadPostProcessingFilterApplyMode.Always
                    addPostProcessingFilter = True
            End Select
            If (addPostProcessingFilter) Then
                result.Filters.Add(Me.PostProcessingFilter)
            End If
        End If
        Return result
    End Function

    ''' <summary>
    ''' Returns the output image processed by the control.
    ''' BackgroundColor and quantization are applied according to the specified FormatEncoderParams.</summary>
    ''' <param name="hintFormatEncoderParams">The image format that will be used then to save image.</param>
    ''' <returns>A Bitmap image processed by the control.</returns>
    Public Function GetProcessedImage(ByVal hintFormatEncoderParams As FormatEncoderParams) As Bitmap
        Dim job As ImageProcessingJob = Me.GetImageProcessingJob()
        Return job.GetProcessedImage(Me.TemporarySourceImageFilePath, hintFormatEncoderParams)
    End Function

    ''' <summary>
    ''' Returns the output image processed by the control.</summary>
    ''' <returns>A Bitmap image processed by the control.</returns>
    Public Function GetProcessedImage() As Bitmap
        Dim job As ImageProcessingJob = Me.GetImageProcessingJob()
        Return job.GetProcessedImage(Me.TemporarySourceImageFilePath)
    End Function

    ''' <summary>
    ''' Processes  the source image and saves the output in a Stream with a specific image format.</summary>
    ''' <param name="destStream">The Stream in which the image will be saved.</param>
    ''' <param name="formatEncoderParams">The image format of the saved image.</param>
    Public Sub SaveProcessedImageToStream(ByVal destStream As Stream, ByVal formatEncoderParams As FormatEncoderParams)
        Dim job As ImageProcessingJob = Me.GetImageProcessingJob()
        job.SaveProcessedImageToStream(Me.TemporarySourceImageFilePath, destStream, formatEncoderParams)
    End Sub

    ''' <summary>
    ''' Processes the source image and saves the output in a Stream with the default image format.</summary>
    ''' <param name="destStream">The Stream in which the image will be saved.</param>
    Public Sub SaveProcessedImageToStream(ByVal destStream As Stream)
        Dim job As ImageProcessingJob = Me.GetImageProcessingJob()
        job.SaveProcessedImageToStream(Me.TemporarySourceImageFilePath, destStream)
    End Sub

    ''' <summary>
    ''' Processes the source image and saves the output in the file system with a specific image format.</summary>
    ''' <param name="destFilePath">The file path of the saved image.</param>
    ''' <param name="formatEncoderParams">The image format of the saved image.</param>
    Public Sub SaveProcessedImageToFileSystem(ByVal destFilePath As String, ByVal formatEncoderParams As FormatEncoderParams)
        Dim job As ImageProcessingJob = Me.GetImageProcessingJob()
        job.SaveProcessedImageToFileSystem(Me.TemporarySourceImageFilePath, destFilePath, formatEncoderParams)
    End Sub

    ''' <summary>
    ''' Processes the source image and save the output in the file system with the default image format.</summary>
    ''' <param name="destFilePath">The file path of the saved image.</param>
    Public Sub SaveProcessedImageToFileSystem(ByVal destFilePath As String)
        Dim job As ImageProcessingJob = Me.GetImageProcessingJob()
        job.SaveProcessedImageToFileSystem(Me.TemporarySourceImageFilePath, destFilePath)
    End Sub

    ''' <summary>
    ''' Processes the source image and returns a byte array containing the processed image encoded with a specific image format.</summary>
    ''' <param name="formatEncoderParams">The image format of the saved image.</param>
    ''' <returns>An array of bytes containing the processed image.</returns>
    Public Function SaveProcessedImageToByteArray(ByVal formatEncoderParams As FormatEncoderParams) As Byte()
        Dim job As ImageProcessingJob = Me.GetImageProcessingJob()
        Return job.SaveProcessedImageToByteArray(Me.TemporarySourceImageFilePath, formatEncoderParams)
    End Function

    ''' <summary>
    ''' Processes the source image and returns a byte array containing the processed image encoded with the default image format.</summary>
    ''' <returns>An array of bytes containing the processed image.</returns>
    Public Function SaveProcessedImageToByteArray() As Byte()
        Dim job As ImageProcessingJob = Me.GetImageProcessingJob()
        Return job.SaveProcessedImageToByteArray(Me.TemporarySourceImageFilePath)
    End Function

#End Region

#Region "Misc"

    ''' <summary>
    ''' Deletes the internal temporary files generated by the control.</summary>
    Public Sub ClearTemporaryFiles()
        Me.ClearTemporaryFile(Me.TemporarySourceImageFilePath)
        Me.ClearTemporaryFile(Me.UploadFilePath)
        Me.ClearTemporaryFile(Me.PreviewImageFilePath)
        Me.ClearTemporaryFile(Me.TemporaryWriteTestFilePath)
        Me.ClearTemporaryFile(Me.UploadMonitorFilePath)
    End Sub

    ''' <summary>
    ''' Opens the image edit popup window.</summary>
    Public Sub OpenImageEditPopup()
        If (Not Me.HasImage) Then
            Throw New Exception("Image not loaded.")
        End If

        ' Open the image edit popup
        Me._AutoOpenImageEditPopup = True
    End Sub

#End Region

#Region "Protected"

    Protected Sub ClearTemporaryFile(ByVal filePath As String)
        If (String.IsNullOrEmpty(filePath)) Then
            Return
        End If

        If (File.Exists(filePath)) Then
            File.Delete(filePath)
        End If
    End Sub

    Protected Function GetSubElementId(ByVal subId As String) As String
        Return Me.ClientID + "_" + subId
    End Function

    Protected Sub TemporaryFolderWriteTest()
        ' Check if the application can write on the temporary folder
        If (Not File.Exists(Me.TemporaryWriteTestFilePath)) Then
            File.WriteAllText(Me.TemporaryWriteTestFilePath, "write test", System.Text.Encoding.UTF8)
        End If
    End Sub

    Protected Function GetRenderStyle_container0() As String
        Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder()
        sb.Append(Me.GetRenderStyleValue("width", Me._Width))
        sb.Append(Me.GetRenderStyleValue("background-color", Me._BackColor))
        sb.Append(Me.GetRenderStyleValue("border-color", Me._BorderColor))
        sb.Append(Me.GetRenderStyleValue("border-style", Me._BorderStyle))
        sb.Append(Me.GetRenderStyleValue("border-width", Me._BorderWidth))
        Return sb.ToString()
    End Function

    Protected Function GetRenderStyle_content() As String
        Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder()
        sb.Append(Me.GetRenderStyleValue("background-color", Me._ContentBackColor))
        sb.Append(Me.GetRenderStyleValue("color", Me._ContentForeColor))
        sb.Append(Me.GetRenderStyleValue("border-color", Me._ContentBorderColor))
        sb.Append(Me.GetRenderStyleValue("border-style", Me._ContentBorderStyle))
        sb.Append(Me.GetRenderStyleValue("border-width", Me._ContentBorderWidth))
        Return sb.ToString()
    End Function

    Protected Function GetRenderStyleValue(ByVal name As String, ByVal value As Unit) As String
        If (Not value.IsEmpty) Then
            Return name + ":" + value.ToString(System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant() + ";"
        End If

        ' Do not render the value
        Return ""
    End Function

    Protected Function GetRenderStyleValue(ByVal name As String, ByVal value As Color) As String
        If (Not value.IsEmpty) Then
            Return name + ":" + ColorTranslator.ToHtml(value) + ";"
        End If

        ' Do not render the value
        Return ""
    End Function

    Protected Function GetRenderStyleValue(ByVal name As String, ByVal value As BorderStyle) As String
        If (value <> BorderStyle.NotSet) Then
            Return name + ":" + value.ToString().ToLowerInvariant() + ";"
        End If

        ' Do not render the value
        Return ""
    End Function

    Protected Function GetQueryKey(ByVal additionalData As String) As String
        Dim rnd As Random = New Random()
        Dim keyLeft As String = Guid.NewGuid().ToString("N").Substring(0, rnd.Next(1, 10))
        Dim keyRight As String = Guid.NewGuid().ToString("N").Substring(0, rnd.Next(1, 10))
        Dim timeStamp As Long = DateTime.Now.Ticks + Convert.ToInt64(rnd.Next(500, 500000))
        Dim key As String = keyLeft + "&" + timeStamp.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&" + additionalData + "&" + keyRight
        Dim encodedKey As String = CodeCarvings.Piczard.Helpers.SecurityHelper.EncryptString(key)

        Return encodedKey
    End Function

    Protected Sub ProcessUploadSuccess()
        Dim sourceImageClientFileName As String = Me._SourceImageClientFileName

        If (Me.HasImage) Then
            ' Unload the current image
            Me.UnloadImage(False)
        End If

        ' Delete old files
        If (File.Exists(Me.TemporarySourceImageFilePath)) Then
            File.Delete(Me.TemporarySourceImageFilePath)
        End If

        If (Me._ImageUploadPreProcessingFilter Is Nothing) Then
            ' Just copy the source image
            File.Copy(Me.UploadFilePath, Me.TemporarySourceImageFilePath, True)
        End If

        Try
            If (Me._ImageUploadPreProcessingFilter IsNot Nothing) Then
                ' Pre-process the just uploaded image
                Using sourceImage As LoadedImage = ImageArchiver.LoadImage(Me.UploadFilePath)
                    '  Use PNG to preserve transparency
                    Dim format As FormatEncoderParams = New PngFormatEncoderParams()
                    Using tempImage As System.Drawing.Image = Me._ImageUploadPreProcessingFilter.GetProcessedImage(sourceImage, sourceImage.Resolution, format)
                        ImageArchiver.SaveImageToFileSystem(tempImage, Me.TemporarySourceImageFilePath, format)

                        ' Optimization: save server resources...
                        Me.popupPictureTrimmer1.SetLoadImageData_ImageSize(tempImage.Size)
                        Me.popupPictureTrimmer1.SetLoadImageData_ImageResolution(sourceImage.Resolution)
                        Me.popupPictureTrimmer1.SetLoadImageData_ImageFormatId(sourceImage.FormatId)
                    End Using
                End Using
            End If

            ' Load the image in the PictureTrimmer control
            Me.popupPictureTrimmer1.LoadImageFromFileSystem(Me.TemporarySourceImageFilePath, Me._OutputResolution, Me.CropConstraint)
        Catch ex As InvalidImageSizeException
            ' Invalid image size
            ex.ToString()

            ' Display the invalid image size message
            Me.SetCurrentStatusMessage(Me.StatusMessage_InvalidImageSize, True)

            ' EVENT: Upload error (invalid image size)
            Me.OnUploadError(EventArgs.Empty)
        Catch
            ' Invalid image

            ' Display the invalid image message
            Me.SetCurrentStatusMessage(Me.StatusMessage_InvalidImage, True)

            ' EVENT: Upload error (invalid image)
            Me.OnUploadError(EventArgs.Empty)
        End Try

        If (Me.HasImage) Then
            ' Restore the source image client file name (changed in the UnloadImage method)
            Me._SourceImageClientFileName = sourceImageClientFileName

            ' The new image has been uploaded
            Me._ImageUploaded = True
            Me._ImageEdited = False

            ' Update the preview
            Me._UpdatePreview = True

            '--- If (Not (Me.ImageUploadEvent Is Nothing)) Then
            ' EVENT: Image upload
            Dim pictureTrimmerTID As String = Me.popupPictureTrimmer1.TemporaryFileId
            Dim args As ImageUploadEventArgs = New ImageUploadEventArgs(Me._OutputResolution, Me.CropConstraint, Me.PostProcessingFilter, Me.PreviewFilter)
            Me.OnImageUpload(args)
            If (Me.HasImage) Then
                If (Me.popupPictureTrimmer1.TemporaryFileId <> pictureTrimmerTID) Then
                    ' The image has been reloaded outside the control

                    If (Me.AutoOpenImageEditPopupAfterUpload) Then
                        ' Open the image edit popup
                        Me.OpenImageEditPopup()
                    End If

                    ' Exit !!!
                    Return
                End If
            Else
                ' The image has been unloaded, exit.
                Return
            End If

            Dim reloadImage As Boolean = False
            If (args.OutputResolutionChanged) Then
                Me._OutputResolution = args.OutputResolution
                reloadImage = True
            End If
            If (args.CropConstraintChanged) Then
                Me._CropConstraint = args.CropConstraint
                reloadImage = True
            End If
            If (args.PostProcessingFilterChanged) Then
                Me._PostProcessingFilter = args.PostProcessingFilter
                ' No need to reload if only the post processing filter has changed
                ' AND - the updatePreview is surely already TRUE
            End If
            If (args.PreviewFilterChanged) Then
                Me._PreviewFilter = args.PreviewFilter
                ' No need to reload if only the preview filter has changed
                ' AND - the updatePreview is surely already TRUE
            End If
            If (args.ReloadImageSet) Then
                ' Forced to reload the source image
                reloadImage = True
            End If

            If (reloadImage) Then
                ' Reload the image
                If (Not args.ReloadImageSet) Then
                    ' Standard reload, use the current source image size, resolutaion and format to save memory
                    Me.popupPictureTrimmer1.SetLoadImageData_ImageSize(Me.SourceImageSize)
                    Me.popupPictureTrimmer1.SetLoadImageData_ImageResolution(Me.SourceImageResolution)
                    Me.popupPictureTrimmer1.SetLoadImageData_ImageFormatId(Me.SourceImageFormatId)
                End If
                Me.popupPictureTrimmer1.LoadImageFromFileSystem(Me.TemporarySourceImageFilePath, Me._OutputResolution, Me.CropConstraint)
            End If
            '--- End If

            ' Invoke the OpenImageEditPopup after the event, so the eventhandler may change
            ' the AutoOpenImageEditPopupAfterUpload property
            If (Me.AutoOpenImageEditPopupAfterUpload) Then
                ' Open the image edit popup
                Me.OpenImageEditPopup()
            End If
        End If
    End Sub

    Protected Sub ProcessUploadError()
        If (Me.HasImage) Then
            ' Unload the current image so we can display the error message
            Me.UnloadImage(False)
        End If

        ' Display the error message;
        Me.SetCurrentStatusMessage(Me.StatusMessage_UploadError, True)

        ' EVENT: Upload error
        Me.OnUploadError(EventArgs.Empty)
    End Sub

    Protected Sub ProcessEdit()
        ' The new image has been edited
        Me._ImageUploaded = False
        Me._ImageEdited = True

        ' Update the preview
        Me._UpdatePreview = True

        ' EVENT: Image edit
        Me.OnImageEdit(EventArgs.Empty)
    End Sub

    Protected Sub ProcessRemove()
        ' Unload the image
        Me.UnloadImage(True)

        ' EVENT: Image removed
        Me.OnImageRemove(EventArgs.Empty)
    End Sub

    Protected Sub ProcessSelectedConfigurationIndexChanged()
        ' The new image has been edited
        Me._ImageUploaded = False
        Me._ImageEdited = True

        ' Update the preview
        Me._UpdatePreview = True

        ' Open the image edit popup
        Me._AutoOpenImageEditPopup = True

        '--- If (Not (Me.SelectedConfigurationIndexChangedEvent Is Nothing)) Then
        Dim pictureTrimmerTID As String = Me.popupPictureTrimmer1.TemporaryFileId
        ' EVENT: Configuration index changed
        Dim args As SelectedConfigurationIndexChangedEventArgs = New SelectedConfigurationIndexChangedEventArgs(Me._OutputResolution, Me.CropConstraint, Me.PostProcessingFilter, Me.PreviewFilter)
        Me.OnSelectedConfigurationIndexChanged(args)
        If (Me.HasImage) Then
            If (Me.popupPictureTrimmer1.TemporaryFileId <> pictureTrimmerTID) Then
                ' The image has been reloeaded outside the control, exit.
                Return
            End If
        Else
            ' The image has been unloaded, exit.
            Return
        End If

        Dim reloadImage As Boolean = False
        If (args.OutputResolutionChanged) Then
            Me._OutputResolution = args.OutputResolution
            reloadImage = True
        End If
        If (args.CropConstraintChanged) Then
            Me._CropConstraint = args.CropConstraint
            reloadImage = True
        End If
        If (args.PostProcessingFilterChanged) Then
            Me._PostProcessingFilter = args.PostProcessingFilter
            ' No need to reload if only the post processing filter has changed
            ' AND - the updatePreview is surely already TRUE
        End If
        If (args.PreviewFilterChanged) Then
            Me._PreviewFilter = args.PreviewFilter
            ' No need to reload if only the preview filter has changed
            ' AND - the updatePreview is surely already TRUE
        End If
        If (args.ReloadImageSet) Then
            ' Forced to reload the source image
            reloadImage = True
        End If

        If (reloadImage) Then
            ' Reload the image
            If (Not args.ReloadImageSet) Then
                ' Standard reload, use the current source image size, resolutaion and format to save memory
                Me.popupPictureTrimmer1.SetLoadImageData_ImageSize(Me.SourceImageSize)
                Me.popupPictureTrimmer1.SetLoadImageData_ImageResolution(Me.SourceImageResolution)
                Me.popupPictureTrimmer1.SetLoadImageData_ImageFormatId(Me.SourceImageFormatId)
            End If
            Me.popupPictureTrimmer1.LoadImageFromFileSystem(Me.TemporarySourceImageFilePath, Me._OutputResolution, Me.CropConstraint)
        End If
        '--- End If
    End Sub

#End Region

#End Region

#Region "Sub-Classes"

    ''' <summary>
    ''' Base class that provides data for the ImageUpload and the SelectedConfigurationIndexChanged events.</summary>
    Public Class ConfigurationEventArgs
        Inherits EventArgs

        ''' <summary>
        ''' Intializes new instace of the ConfigurationEventArgs class.</summary>
        ''' <param name="outputResolution">The resolution (DPI) of the image that is generated by the control.</param>
        ''' <param name="cropConstraint">The constraints that have to be satisfied by the cropped image.</param>
        ''' <param name="postProcessingFilter">The filter(s) to apply to the image.</param>
        ''' <param name="previewFilter">The filter(s) to apply to the preview image.</param>
        Public Sub New(ByVal outputResolution As Single, ByVal cropConstraint As CropConstraint, ByVal postProcessingFilter As ImageProcessingFilter, ByVal previewFilter As ImageProcessingFilter)
            MyBase.New()

            Me._OutputResolution = outputResolution
            Me._CropConstraint = cropConstraint
            Me._PostProcessingFilter = postProcessingFilter
            Me._PreviewFilter = previewFilter

            Me._OriginalOutputResolution = Me._OutputResolution
            Me._OriginalCropConstraintString = JSONSerializer.SerializeToString(Me._CropConstraint, True)
            Me._OriginalPostProcessingFilterString = JSONSerializer.SerializeToString(Me._PostProcessingFilter, True)
            Me._OriginalPreviewFilterString = JSONSerializer.SerializeToString(Me._PreviewFilter, True)

            Me._ReloadImageSet = False
        End Sub

        Private _OutputResolution As Single
        ''' <summary>
        ''' Gets or sets the resolution (DPI) of the image that is generated by the control.</summary>
        Public Property OutputResolution() As Single
            Get
                Return Me._OutputResolution
            End Get
            Set(ByVal value As Single)
                ' Validate the resolution
                CodeCarvings.Piczard.Helpers.ImageHelper.ValidateResolution(value, True)

                Me._OutputResolution = value
            End Set
        End Property

        Private _CropConstraint As CropConstraint
        ''' <summary>
        ''' Gets the constraints that have to be satisfied by the cropped image.</summary>
        Public Property CropConstraint() As CropConstraint
            Get
                Return Me._CropConstraint
            End Get
            Set(ByVal value As CropConstraint)
                Me._CropConstraint = value
            End Set
        End Property

        Private _PostProcessingFilter As ImageProcessingFilter
        ''' <summary>
        ''' Gets or sets the filter(s) to apply to the image.</summary>
        Public Property PostProcessingFilter() As ImageProcessingFilter
            Get
                Return Me._PostProcessingFilter
            End Get
            Set(ByVal value As ImageProcessingFilter)
                Me._PostProcessingFilter = value
            End Set
        End Property

        Private _PreviewFilter As ImageProcessingFilter
        ''' <summary>
        ''' Gets or sets the filter(s) to apply to the preview image.</summary>
        Public Property PreviewFilter() As ImageProcessingFilter
            Get
                Return Me._PreviewFilter
            End Get
            Set(ByVal value As ImageProcessingFilter)
                Me._PreviewFilter = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the ResizeConstraint used to generate the preview image.
        ''' This property is marked as obsolete since version 2.0.0 of the control and will be soon removed.
        ''' Use 'PreviewFilter' instead.</summary>
        <Obsolete()> _
        Public Property PreviewResizeConstraint() As ResizeConstraint
            Get
                Return DirectCast(Me.PreviewFilter, ResizeConstraint)
            End Get
            Set(ByVal value As ResizeConstraint)
                Me.PreviewFilter = value
            End Set
        End Property

        Private _OriginalOutputResolution As Single
        Friend ReadOnly Property OutputResolutionChanged() As Boolean
            Get
                Return Me._OutputResolution <> Me._OriginalOutputResolution
            End Get
        End Property

        Private _OriginalCropConstraintString As String
        Friend ReadOnly Property CropConstraintChanged() As Boolean
            Get
                Return JSONSerializer.SerializeToString(Me._CropConstraint, True) <> Me._OriginalCropConstraintString
            End Get
        End Property

        Private _OriginalPostProcessingFilterString As String
        Friend ReadOnly Property PostProcessingFilterChanged() As Boolean
            Get
                Return JSONSerializer.SerializeToString(Me._PostProcessingFilter, True) <> Me._OriginalPostProcessingFilterString
            End Get
        End Property

        Private _OriginalPreviewFilterString As String
        Friend ReadOnly Property PreviewFilterChanged() As Boolean
            Get
                Return JSONSerializer.SerializeToString(Me._PreviewFilter, True) <> Me._OriginalPreviewFilterString
            End Get
        End Property

        Private _ReloadImageSet As Boolean
        Friend ReadOnly Property ReloadImageSet() As Boolean
            Get
                Return Me._ReloadImageSet
            End Get
        End Property
        ''' <summary>
        ''' Force the reloading of the source image.</summary>
        Public Sub ReloadImage()
            Me._ReloadImageSet = True
        End Sub

    End Class

    ''' <summary>
    ''' Provides data for the ImageUpload event.</summary>
    Public Class ImageUploadEventArgs
        Inherits ConfigurationEventArgs

        ''' <summary>
        ''' Intializes new instace of the ImageUploadEventArgs class.</summary>
        ''' <param name="outputResolution">The resolution (DPI) of the image that is generated by the control.</param>
        ''' <param name="cropConstraint">The constraints that have to be satisfied by the cropped image.</param>
        ''' <param name="postProcessingFilter">The filter(s) to apply to the image.</param>
        ''' <param name="previewFilter">The filter(s) to apply to the preview image.</param>
        Public Sub New(ByVal outputResolution As Single, ByVal cropConstraint As CropConstraint, ByVal postProcessingFilter As ImageProcessingFilter, ByVal previewFilter As ImageProcessingFilter)
            MyBase.New(outputResolution, cropConstraint, postProcessingFilter, previewFilter)
        End Sub
    End Class

    ''' <summary>
    ''' Provides data for the SelectedConfigurationIndexChanged event.</summary>
    Public Class SelectedConfigurationIndexChangedEventArgs
        Inherits ConfigurationEventArgs

        ''' <summary>
        ''' Intializes new instace of the SelectedConfigurationIndexChangedEventArgs class.</summary>
        ''' <param name="outputResolution">The resolution (DPI) of the image that is generated by the control.</param>
        ''' <param name="cropConstraint">The constraints that have to be satisfied by the cropped image.</param>
        ''' <param name="postProcessingFilter">The filter(s) to apply to the image.</param>
        ''' <param name="previewFilter">The filter(s) to apply to the preview image.</param>
        Public Sub New(ByVal outputResolution As Single, ByVal cropConstraint As CropConstraint, ByVal postProcessingFilter As ImageProcessingFilter, ByVal previewFilter As ImageProcessingFilter)
            MyBase.New(outputResolution, cropConstraint, postProcessingFilter, previewFilter)
        End Sub
    End Class

    ''' <summary>
    ''' Specifies if and when PostProcessingFilter must be applied.</summary>
    <Serializable()> _
    Public Enum SimpleImageUploadPostProcessingFilterApplyMode As Integer
        Never = 0
        OnlyNewImages = 1
        Always = 2
    End Enum

    ''' <summary>
    ''' Provides access to settings of a PopupPictureTrimmer instance.</summary>
    Public Class PopupPictureTrimmerSettingsProvider
        Public Sub New(ByVal pictureTrimmer As PopupPictureTrimmer)
            Me._PictureTrimmer = pictureTrimmer
        End Sub

        Private _PictureTrimmer As PopupPictureTrimmer

        ''' <summary>
        ''' Gets or sets a value indicating whether users can resize the source image through the GUI.</summary>
        Public Property AllowResize() As Boolean
            Get
                Return Me._PictureTrimmer.AllowResize
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.AllowResize = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the control has to automatically freeze the GUI when the form is submitted.</summary>
        Public Property AutoFreezeOnFormSubmit() As Boolean
            Get
                Return Me._PictureTrimmer.AutoFreezeOnFormSubmit
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.AutoFreezeOnFormSubmit = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating how the PictureTrimmer instance automatically calculates the ZoomFactor.</summary>
        Public Property AutoZoomMode() As PictureTrimmerAutoZoomMode
            Get
                Return Me._PictureTrimmer.AutoZoomMode
            End Get
            Set(ByVal value As PictureTrimmerAutoZoomMode)
                Me._PictureTrimmer.AutoZoomMode = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the BackColor property of the PictureTrimmer control.</summary>
        Public Property BackColor() As Color
            Get
                Return Me._PictureTrimmer.BackColor
            End Get
            Set(ByVal value As Color)
                Me._PictureTrimmer.BackColor = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the text of the "Cancel button".</summary>
        Public Property CancelButtonText() As String
            Get
                Return Me._PictureTrimmer.CancelButtonText
            End Get
            Set(ByVal value As String)
                Me._PictureTrimmer.CancelButtonText = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating how the PictureTrimmer GUI renders the cropping mask.</summary>
        Public Property CropShadowMode() As PictureTrimmerCropShadowMode
            Get
                Return Me._PictureTrimmer.CropShadowMode
            End Get
            Set(ByVal value As PictureTrimmerCropShadowMode)
                Me._PictureTrimmer.CropShadowMode = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the control has to automatically
        ''' center the view after the user drags the crop area outside the visible area.</summary>
        Public Property EnableAutoCenterView() As Boolean
            Get
                Return Me._PictureTrimmer.EnableAutoCenterView
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.EnableAutoCenterView = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the crop area automatically snaps
        ''' to the edge or the center of the image when the user moves the rectangle
        ''' near those positions.</summary>
        Public Property EnableSnapping() As Boolean
            Get
                Return Me._PictureTrimmer.EnableSnapping
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.EnableSnapping = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the Window Mode property of the Adobe Flash movie for transparency,
        ''' layering, and positioning in the browser (it is strongly suggested to use
        ''' the FlashWMode.Window setting).</summary>
        Public Property FlashWMode() As FlashWMode
            Get
                Return Me._PictureTrimmer.FlashWMode
            End Get
            Set(ByVal value As FlashWMode)
                Me._PictureTrimmer.FlashWMode = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the foreground color.</summary>
        Public Property ForeColor() As Color
            Get
                Return Me._PictureTrimmer.ForeColor
            End Get
            Set(ByVal value As Color)
                Me._PictureTrimmer.ForeColor = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the Cascading Style Sheet (CSS) class used to customize the
        ''' style of the LightBox popup window.</summary>
        Public Property LightBoxCssClass() As String
            Get
                Return Me._PictureTrimmer.LightBoxCssClass
            End Get
            Set(ByVal value As String)
                Me._PictureTrimmer.LightBoxCssClass = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the text of the "Save button".</summary>
        Public Property SaveButtonText() As String
            Get
                Return Me._PictureTrimmer.SaveButtonText
            End Get
            Set(ByVal value As String)
                Me._PictureTrimmer.SaveButtonText = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether to display the "Cancel button" in the popup window.</summary>
        Public Property ShowCancelButton() As Boolean
            Get
                Return Me._PictureTrimmer.ShowCancelButton
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.ShowCancelButton = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether to show lines that facilitate the
        ''' alignment of the crop rectangle.</summary>
        Public Property ShowCropAlignmentLines() As Boolean
            Get
                Return Me._PictureTrimmer.ShowCropAlignmentLines
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.ShowCropAlignmentLines = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether to show the "Details panel" in the
        ''' component GUI.</summary>
        Public Property ShowDetailsPanel() As Boolean
            Get
                Return Me._PictureTrimmer.ShowDetailsPanel
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.ShowDetailsPanel = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether to show the flip control in the "Rotate/Flip panel". 
        ''' The flip control allows the user to flip the image horizontally and/or vertically.</summary>
        Public Property ShowFlipPanel() As Boolean
            Get
                Return Me._PictureTrimmer.ShowFlipPanel
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.ShowFlipPanel = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether to show the "Adjustments panel" in
        ''' the component GUI. The "Adjustments panel" allows the user to change Brightness,
        ''' Contrast, Hue and/or Saturation of the Image.</summary>
        Public Property ShowImageAdjustmentsPanel() As Boolean
            Get
                Return Me._PictureTrimmer.ShowImageAdjustmentsPanel
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.ShowImageAdjustmentsPanel = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether to show the "Resize panel" in the
        ''' component GUI. The "Resize panel" allows the user to change the ResizeFactor
        ''' applied to the source Image.</summary>
        Public Property ShowResizePanel() As Boolean
            Get
                Return Me._PictureTrimmer.ShowResizePanel
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.ShowResizePanel = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether to show the rotate control in the
        ''' "Rotate/Flip panel". The rotate control allows the user to rotate the image
        ''' clockwise by 0, 90, 180 or 270 degrees.</summary>
        Public Property ShowRotatePanel() As Boolean
            Get
                Return Me._PictureTrimmer.ShowRotatePanel
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.ShowRotatePanel = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether to show the rulers at the left and
        ''' at the top of the working area.</summary>
        Public Property ShowRulers() As Boolean
            Get
                Return Me._PictureTrimmer.ShowRulers
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.ShowRulers = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether to show the "Zoom panel" in the component
        ''' GUI. The "Zoom panel" allows the user to magnify an area of the image.
        ''' Please note that ZoomPanel is always invisible when the crop feature is disabled (CropConstraint = null).</summary>
        Public Property ShowZoomPanel() As Boolean
            Get
                Return Me._PictureTrimmer.ShowZoomPanel
            End Get
            Set(ByVal value As Boolean)
                Me._PictureTrimmer.ShowZoomPanel = value
            End Set
        End Property

    End Class

#End Region

End Class
