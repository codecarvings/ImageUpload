' -------------------------------------------------------
' Piczard | SimpleImageUpload User Control
' Author: Sergio Turolla
' <codecarvings.com>
' 
' Copyright (c) 2011-2012 Sergio Turolla
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

Option Strict On
Option Explicit On

Imports System.IO
Imports System.Xml
Imports System.Drawing

Imports CodeCarvings.Piczard.Web

Partial Public Class SimpleImageUpload_upload
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If (Not Me.IsPostBack) Then
            Me.TemporaryFileId = Request("tfid")
            Me.Key = Request("k")
            Me.DebugUploadProblems = (Request("dup") = "1")

            Me.SimpleImageUploadControlId = Request("cid")
            Dim bsw As String = Request("bsw")
            Dim bsh As String = Request("bsh")
            If ((Not String.IsNullOrEmpty(bsw)) And (Not String.IsNullOrEmpty(bsh))) Then
                Try
                    Me.ButtonSize = New Size(Integer.Parse(bsw, System.Globalization.CultureInfo.InvariantCulture), Integer.Parse(bsh, System.Globalization.CultureInfo.InvariantCulture))
                Catch

                End Try
            End If

            Me.phMainContainer.Visible = Me.CanUpload
        End If
    End Sub

    Protected Overrides Sub OnPreRender(ByVal e As System.EventArgs)
        ' Setup sizes
        Dim fontSize As Integer = DirectCast(If(Me.ButtonSize.Width > Me.ButtonSize.Height, Me.ButtonSize.Width, Me.ButtonSize.Height), Integer)
        Me.fuFile.Style(HtmlTextWriterStyle.FontSize) = fontSize.ToString(System.Globalization.CultureInfo.InvariantCulture) + "px"

        MyBase.OnPreRender(e)
    End Sub

#Region "Upload monitor"

    Protected Sub Page_Error(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Error
        ' Write the error info into a temporary file
        Dim temporaryFileId As String = Request("tfid")
        Dim key As String = Request("k")
        Dim debugUploadProblems As Boolean = (Request("dup") = "1")

        ' Validate the parameters
        If (Not Me.ValidateTemporaryFileId(temporaryFileId)) Then
            Return
        End If
        If (Not Me.ValidateKey(key, Me.GetKeyAdditionalData(debugUploadProblems))) Then
            Return
        End If

        Dim sb As System.Text.StringBuilder = New System.Text.StringBuilder()
        If (debugUploadProblems) Then
            ' Write also the error details
            Dim ex As Exception = Server.GetLastError()
            sb.Append("<strong>Error in: </strong>" + Request.Url.ToString() + "<br />" + ControlChars.CrLf)
            sb.Append("<br />" + ControlChars.CrLf)
            sb.Append("<strong>Error Message: </strong>" + ex.Message.ToString() + "<br />" + ControlChars.CrLf)
            sb.Append("<br />" + ControlChars.CrLf)
            sb.Append("<strong>Stack Trace:</strong><br />" + ControlChars.CrLf)
            sb.Append(Server.GetLastError().ToString())
        End If

        ' State 3 = upload error
        Me.WriteUploadMonitorFile(temporaryFileId, 3, sb.ToString())
    End Sub

    Protected Sub WriteUploadMonitorFile(ByVal temporaryFileId As String, ByVal state As Integer, ByVal message As String)
        ' Write the file that contains information about the current upload process
        Dim doc As XmlDocument = New XmlDocument()

        Dim declaration As XmlNode = doc.CreateNode(XmlNodeType.XmlDeclaration, Nothing, Nothing)
        doc.AppendChild(declaration)

        Dim uploadMonitor As XmlElement = doc.CreateElement("uploadMonitor")
        doc.AppendChild(uploadMonitor)
        uploadMonitor.SetAttribute("state", state.ToString(System.Globalization.CultureInfo.InvariantCulture))
        uploadMonitor.InnerText = message

        Dim uploadMonitorFilePath As String = TemporaryFileManager.GetTemporaryFilePath(temporaryFileId, "_um.xml")
        File.WriteAllText(uploadMonitorFilePath, doc.OuterXml, System.Text.Encoding.UTF8)
    End Sub

#End Region

    Protected Property SimpleImageUploadControlId() As String
        Get
            If (Me.ViewState("SimpleImageUploadControlId") IsNot Nothing) Then
                Return DirectCast(Me.ViewState("SimpleImageUploadControlId"), String)
            Else
                Return ""
            End If
        End Get
        Set(ByVal value As String)
            Me.ViewState("SimpleImageUploadControlId") = value
        End Set
    End Property

    Protected Property TemporaryFileId() As String
        Get
            If (Me.ViewState("TemporaryFileId") IsNot Nothing) Then
                Return DirectCast(Me.ViewState("TemporaryFileId"), String)
            Else
                Return ""
            End If
        End Get
        Set(ByVal value As String)
            Me.ViewState("TemporaryFileId") = value
        End Set
    End Property

    Protected Property DebugUploadProblems() As Boolean
        Get
            If (Me.ViewState("DebugUploadProblems") IsNot Nothing) Then
                Return DirectCast(Me.ViewState("DebugUploadProblems"), Boolean)
            Else
                Return False
            End If
        End Get
        Set(ByVal value As Boolean)
            Me.ViewState("DebugUploadProblems") = value
        End Set
    End Property

    Protected Property Key() As String
        Get
            If (Me.ViewState("Key") IsNot Nothing) Then
                Return DirectCast(Me.ViewState("Key"), String)
            Else
                Return ""
            End If
        End Get
        Set(ByVal value As String)
            Me.ViewState("Key") = value
        End Set
    End Property

    Protected Property ButtonSize() As Size
        Get
            If (Me.ViewState("ButtonSize") IsNot Nothing) Then
                Return DirectCast(Me.ViewState("ButtonSize"), Size)
            Else
                ' Default value
                Return New Size(100, 26)
            End If
        End Get
        Set(ByVal value As Size)
            Me.ViewState("ButtonSize") = value
        End Set
    End Property

    Protected ReadOnly Property UploadFilePath() As String
        Get
            Return TemporaryFileManager.GetTemporaryFilePath(Me.TemporaryFileId, "_u.tmp")
        End Get
    End Property

    Protected Sub btnUpload_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUpload.Click
        If (Not Me.CanUpload) Then
            Return
        End If

        If (Not Me.fuFile.HasFile) Then
            Return
        End If

        ' Save the image file
        Me.fuFile.SaveAs(Me.UploadFilePath)

        ' State 2 = upload success
        ' Write the client file name into the XML file 
        Me.WriteUploadMonitorFile(Me.TemporaryFileId, 2, Path.GetFileName(Me.fuFile.FileName))

        ' Debug / test
        ' System.Threading.Thread.Sleep(4000);

        ' Launch the upload success client script
        Me.ClientScript.RegisterStartupScript(Me.GetType(), "onUploadSuccess", "onUploadSuccess();", True)
    End Sub

    Protected Function ValidateTemporaryFileId(ByVal temporaryFileId As String) As Boolean
        If (String.IsNullOrEmpty(temporaryFileId)) Then
            Return False
        End If
        If (Not TemporaryFileManager.ValidateTemporaryFileId(temporaryFileId, False)) Then
            Return False
        End If

        Return True
    End Function

    Protected Function ValidateKey(ByVal key As String, ByVal additionalData As String) As Boolean
        If (String.IsNullOrEmpty(key)) Then
            Return False
        End If
        Dim decodedKey As String = CodeCarvings.Piczard.Helpers.SecurityHelper.DecryptString(key)
        Dim keyVett As String() = decodedKey.Split("&"c)
        If (keyVett.Length <> 4) Then
            Return False
        End If
        Dim timestamp As DateTime = New DateTime(Long.Parse(keyVett(1)))
        If (timestamp < DateTime.Now.AddDays(-1)) Then
            Return False
        End If
        If (keyVett(2) <> additionalData) Then
            Return False
        End If

        Return True
    End Function

    Protected Function GetKeyAdditionalData(ByVal debugUploadProblems As Boolean) As String
        Dim result As String = "dup=" + If(debugUploadProblems, "1", "0")
        Return result
    End Function

    Protected ReadOnly Property CanUpload() As Boolean
        Get
            Try
                If (Not Me.ValidateTemporaryFileId(Me.TemporaryFileId)) Then
                    Return False
                End If
                If (Not Me.ValidateKey(Me.Key, Me.GetKeyAdditionalData(Me.DebugUploadProblems))) Then
                    Return False
                End If

                Return True
            Catch
                Return False
            End Try
        End Get
    End Property

End Class
