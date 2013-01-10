<%@ WebHandler Language="VB" Class="SimpleImageUpload_uploadMonitor" %>
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

Option Strict On
Option Explicit On

Imports System
Imports System.Web
Imports System.IO

Imports CodeCarvings.Piczard.Web

Public Class SimpleImageUpload_uploadMonitor
    : Implements IHttpHandler
    
    ' STATES:
    ' 1 = File not found -> No error (upload in progress...)
    ' 2 = upload success
    ' 3 = upload error
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/xml"

        Dim temporaryFileId As String = context.Request("tfid")
        Dim key As String = context.Request("k")

        If (Not Me.ValidateTemporaryFileId(temporaryFileId)) Then
            Me.WriteEmptyState(context)
            Return
        End If
        If (Not Me.ValidateKey(key, "")) Then
            Me.WriteEmptyState(context)
            Return
        End If

        Dim uploadMonitorFilePath As String = Me.GetUploadMonitorFilePath(temporaryFileId)
        If (File.Exists(uploadMonitorFilePath)) Then
            ' File found -> An error has occurred
            Dim xml As String = File.ReadAllText(uploadMonitorFilePath, System.Text.Encoding.UTF8)
            context.Response.Write(xml)
        Else
            ' File not found -> No error
            Me.WriteState(context, 1)
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
    
    Protected Function GetUploadMonitorFilePath(ByVal temporaryFileId As String) As String
        Return TemporaryFileManager.GetTemporaryFilePath(temporaryFileId, "_um.xml")
    End Function
    
    Protected Sub WriteState(ByVal context As HttpContext, ByVal state As Integer)
        Dim xml As String = String.Format("<?xml version=""1.0""?><uploadMonitor state=""{0}""></uploadMonitor>", state)
        context.Response.Write(xml)
    End Sub
    
    Protected Sub WriteEmptyState(ByVal context As HttpContext)
        Me.WriteState(context, 0)
    End Sub

End Class