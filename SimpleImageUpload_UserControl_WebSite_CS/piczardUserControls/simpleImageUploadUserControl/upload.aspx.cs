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
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;
using System.Drawing;

using CodeCarvings.Piczard.Web;

public partial class SimpleImageUpload_upload 
    : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            this.TemporaryFileId = Request["tfid"];
            this.Key = Request["k"];
            this.DebugUploadProblems = (Request["dup"] == "1");

            this.SimpleImageUploadControlId = Request["cid"];
            string bsw = Request["bsw"];
            string bsh = Request["bsh"];
            if ((!string.IsNullOrEmpty(bsw)) && (!string.IsNullOrEmpty(bsh)))
            {
                try
                {

                    this.ButtonSize = new Size(int.Parse(bsw, System.Globalization.CultureInfo.InvariantCulture), int.Parse(bsh, System.Globalization.CultureInfo.InvariantCulture));
                }
                catch
                {
                }
            }

            this.phMainContainer.Visible = this.CanUpload;
        }
    }

    protected override void OnPreRender(EventArgs e)
    {
        // Setup sizes
        int fontSize = this.ButtonSize.Width > this.ButtonSize.Height ? this.ButtonSize.Width : this.ButtonSize.Height;
        this.fuFile.Style[HtmlTextWriterStyle.FontSize] = fontSize.ToString(System.Globalization.CultureInfo.InvariantCulture) + "px";

        base.OnPreRender(e);
    }

    #region Upload monitor

    protected void Page_Error(object sender, EventArgs e)
    {
        // Write the error info into a temporary file
        string temporaryFileId = Request["tfid"];
        string key = Request["k"];
        bool debugUploadProblems = (Request["dup"] == "1");

        // Validate the parameters
        if (!this.ValidateTemporaryFileId(temporaryFileId))
        {
            return;
        }
        if (!this.ValidateKey(key, this.GetKeyAdditionalData(debugUploadProblems)))
        {
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if (debugUploadProblems)
        {
            // Write also the error details
            Exception ex = Server.GetLastError();
            sb.Append("<strong>Error in: </strong>" + Request.Url.ToString() + "<br />\r\n");
            sb.Append("<br />\r\n");
            sb.Append("<strong>Error Message: </strong>" + ex.Message.ToString() + "<br />\r\n");
            sb.Append("<br />\r\n");
            sb.Append("<strong>Stack Trace:</strong><br />\r\n");
            sb.Append(Server.GetLastError().ToString());
        }

        // State 3 = upload error
        this.WriteUploadMonitorFile(temporaryFileId, 3, sb.ToString());
    }

    protected void WriteUploadMonitorFile(string temporaryFileId, int state, string message)
    {
        // Write the file that contains information about the current upload process
        XmlDocument doc = new XmlDocument();

        XmlNode declaration = doc.CreateNode(XmlNodeType.XmlDeclaration, null, null);
        doc.AppendChild(declaration);

        XmlElement uploadMonitor = doc.CreateElement("uploadMonitor");
        doc.AppendChild(uploadMonitor);
        uploadMonitor.SetAttribute("state", state.ToString(System.Globalization.CultureInfo.InvariantCulture));
        uploadMonitor.InnerText = message;

        string uploadMonitorFilePath = TemporaryFileManager.GetTemporaryFilePath(temporaryFileId, "_um.xml");
        File.WriteAllText(uploadMonitorFilePath, doc.OuterXml, System.Text.Encoding.UTF8);
    }

    #endregion

    protected string SimpleImageUploadControlId
    {
        get
        {
            if (this.ViewState["SimpleImageUploadControlId"] != null)
            {
                return (string)this.ViewState["SimpleImageUploadControlId"];
            }
            else
            {
                return "";
            }
        }
        set
        {
            this.ViewState["SimpleImageUploadControlId"] = value;
        }
    }

    protected string TemporaryFileId
    {
        get
        {
            if (this.ViewState["TemporaryFileId"] != null)
            {
                return (string)this.ViewState["TemporaryFileId"];
            }
            else
            {
                return "";
            }
        }
        set
        {
            this.ViewState["TemporaryFileId"] = value;
        }
    }

    protected bool DebugUploadProblems
    {
        get
        {
            if (this.ViewState["DebugUploadProblems"] != null)
            {
                return (bool)this.ViewState["DebugUploadProblems"];
            }
            else
            {
                return false;
            }
        }
        set
        {
            this.ViewState["DebugUploadProblems"] = value;
        }
    }

    protected string Key
    {
        get
        {
            if (this.ViewState["Key"] != null)
            {
                return (string)this.ViewState["Key"];
            }
            else
            {
                return "";
            }
        }
        set
        {
            this.ViewState["Key"] = value;
        }
    }

    protected Size ButtonSize
    {
        get
        {
            if (this.ViewState["ButtonSize"] != null)
            {
                return (Size)this.ViewState["ButtonSize"];
            }
            else
            {
                // Default value
                return new Size(100, 26);
            }
        }
        set
        {
            this.ViewState["ButtonSize"] = value;
        }
    }

    protected string UploadFilePath
    {
        get
        {
            return TemporaryFileManager.GetTemporaryFilePath(this.TemporaryFileId, "_u.tmp");
        }
    }

    protected void btnUpload_Click(object sender, EventArgs e)
    {
        if (!this.CanUpload)
        {
            return;
        }

        if (!this.fuFile.HasFile)
        {
            return;
        }

        // Save the image file
        this.fuFile.SaveAs(this.UploadFilePath);

        // State 2 = upload success
        // Write the client file name into the XML file 
        this.WriteUploadMonitorFile(this.TemporaryFileId, 2, Path.GetFileName(this.fuFile.FileName));

        // Debug / test
        //System.Threading.Thread.Sleep(4000);

        // Launch the upload success client script
        this.ClientScript.RegisterStartupScript(this.GetType(), "onUploadSuccess", "onUploadSuccess();", true);
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

    protected string GetKeyAdditionalData(bool debugUploadProblems)
    {
        string result = "dup=" + (debugUploadProblems ? "1" : "0");
        return result;
    }

    protected bool CanUpload
    {
        get
        {
            try
            {
                if (!this.ValidateTemporaryFileId(this.TemporaryFileId))
                {
                    return false;
                }
                if (!this.ValidateKey(this.Key, this.GetKeyAdditionalData(this.DebugUploadProblems)))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
