<%@ WebHandler Language="VB" Class="SimpleImageUploadUserControl_preview" %>
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

Imports System
Imports System.Web
Imports System.IO

Imports CodeCarvings.Piczard
Imports CodeCarvings.Piczard.Web

Public Class SimpleImageUploadUserControl_preview
    : Implements IHttpHandler
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim temporaryFileId As String = context.Request("tfid")
        Dim key As String = context.Request("k")

        If (Not Me.ValidateTemporaryFileId(temporaryFileId)) Then
            Me.WriteEmpty(context)
            Return
        End If
        If (Not Me.ValidateKey(key, "")) Then
            Me.WriteEmpty(context)
            Return
        End If
        
        ' Transmit the image preview
        Dim previewImageFilePath As String = Me.GetPreviewImageFilePath(temporaryFileId)
        If (File.Exists(previewImageFilePath)) Then
            ' Preview image found
            ImageArchiver.TransmitImageFileToWebResponse(previewImageFilePath, context.Response)
        Else
            ' Preview image not found
            Me.WriteEmpty(context)
        End If
    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
    
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
        Dim keyVett As String() = decodedKey.Split("&")
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
    
    Protected Function GetPreviewImageFilePath(ByVal temporaryFileId As String) As String
        Return TemporaryFileManager.GetTemporaryFilePath(temporaryFileId, "_p.jpg")
    End Function
    
    Protected Sub WriteEmpty(ByVal context As HttpContext)
        context.Response.ContentType = "text/plain"
        context.Response.Write("no image")
    End Sub

End Class