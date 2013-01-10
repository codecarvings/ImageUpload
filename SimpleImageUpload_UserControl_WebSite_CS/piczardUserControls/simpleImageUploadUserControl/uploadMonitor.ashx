<%@ WebHandler Language="C#" Class="SimpleImageUpload_uploadMonitor" %>
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
 
using System;
using System.Web;
using System.IO;

using CodeCarvings.Piczard.Web;

public class SimpleImageUpload_uploadMonitor 
    : IHttpHandler 
{
    
    // STATES:
    // 1 = File not found -> No error (upload in progress...)
    // 2 = upload success
    // 3 = upload error
    
    public void ProcessRequest (HttpContext context) 
    {
        context.Response.ContentType = "application/xml";

        string temporaryFileId = context.Request["tfid"];
        string key = context.Request["k"];

        if (!this.ValidateTemporaryFileId(temporaryFileId))
        {
            this.WriteEmptyState(context);
            return;
        }
        if (!this.ValidateKey(key, ""))
        {
            this.WriteEmptyState(context);
            return;
        }

        string uploadMonitorFilePath = this.GetUploadMonitorFilePath(temporaryFileId);
        if (File.Exists(uploadMonitorFilePath))
        {
            // File found -> An error has occurred
            string xml = File.ReadAllText(uploadMonitorFilePath, System.Text.Encoding.UTF8);
            context.Response.Write(xml);
        }
        else
        {
            // File not found -> No error
            this.WriteState(context, 1);
        }
    }
    
    public bool IsReusable 
    {
        get 
        {
            return false;
        }
    }

    protected bool ValidateTemporaryFileId(string temporaryFileId)
    {
        if (string.IsNullOrEmpty(temporaryFileId))
        {
            return false;
        }
        if (!TemporaryFileManager.ValidateTemporaryFileId(temporaryFileId, false))
        {
            return false;
        }

        return true;
    }
    protected bool ValidateKey(string key, string additionalData)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }
        string decodedKey = CodeCarvings.Piczard.Helpers.SecurityHelper.DecryptString(key);
        string[] keyVett = decodedKey.Split('&');
        if (keyVett.Length != 4)
        {
            return false;
        }
        DateTime timestamp = new DateTime(long.Parse(keyVett[1]));
        if (timestamp < DateTime.Now.AddDays(-1))
        {
            return false;
        }
        if (keyVett[2] != additionalData)
        {
            return false;
        }

        return true;
    }

    protected string GetUploadMonitorFilePath(string temporaryFileId)
    {
        return TemporaryFileManager.GetTemporaryFilePath(temporaryFileId, "_um.xml");
    }
    
    protected void WriteState(HttpContext context, int state)
    {
        string xml = string.Format("<?xml version=\"1.0\"?><uploadMonitor state=\"{0}\"></uploadMonitor>", state);
        context.Response.Write(xml);
    }

    protected void WriteEmptyState(HttpContext context)
    {
        this.WriteState(context, 0);
    }
}