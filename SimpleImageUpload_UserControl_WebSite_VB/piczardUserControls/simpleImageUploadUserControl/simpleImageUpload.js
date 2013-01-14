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

// Setup the variable that indicates that the javascript library has been loaded
window.__ccpz_siu_lt = true;

// --- Namespaces ---

if (typeof (window.CodeCarvings) === "undefined") {
    window.CodeCarvings = function () {
    }
}
if (typeof (CodeCarvings.Wcs) === "undefined") {
    CodeCarvings.Wcs = function () {
    }
}
if (typeof (CodeCarvings.Wcs.Piczard) === "undefined") {
    CodeCarvings.Wcs.Piczard = function () {
    }
}
if (typeof (CodeCarvings.Wcs.Piczard.Upload) === "undefined") {
    CodeCarvings.Wcs.Piczard.Upload = function () {
    }
}

// --- Classes ---

if (typeof (CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload) === "undefined") {
    // Do not overwrite the controls array if already defined...

    CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload = function (id) {
        this.id = id;
    }

    CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.controls = [];

    // Check if Flash player is available
    CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.hasFlashPlayer = null;
    CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.get_hasFlashPlayer = function (forceUpdate) {
        if (CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.hasFlashPlayer !== null) {
            if (!forceUpdate) {
                return CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.hasFlashPlayer;
            }
        }

        var ex;
        try {
            CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.hasFlashPlayer = CodeCarvings.Wcs.Piczard.Shared.Externals.swfobject_v2_2.swfobject.hasFlashPlayerVersion("8") ? true : false;
        }
        catch (ex) {
        }
        return CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.hasFlashPlayer;
    }
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.prototype =
{
    className: "CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload",

    id: "",
    loadData: null,
    xmlHttp: null,
    uploadMonitorTimeoutId: 0,
    uploadMonitorDelay: 5000,
    cancelUploadEnableDelay: 2000,
    uploadErrorLightbox: null,
    uploadInProgress: false,
    wait: false,
    autoSetPageBlockSubmit: true,
    statusMessageVisible: true,
    displayConfigurations: false,
    configurationChange: false,

    getSubElementId: function (subId) {
        if (subId) {
            return this.id + "_" + subId;
        }
        else {
            return this.id;
        }
    },

    getSubElement: function (subId) {
        return document.getElementById(this.getSubElementId(subId));
    },

    load: function (loadData) {
        this.loadData = loadData;
    },

    initializeUI: function () {
        var popupPictureTrimmer = CodeCarvings.Wcs.Piczard.PictureTrimmer.getControl(this.loadData.popupPictureTrimmerClientId);
        if (popupPictureTrimmer) {
            // Initialize the parent control
            popupPictureTrimmer.parentControl = this;
        }

        // Get the frame element
        var oFrId = this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_ifr);
        var oFr = this.getSubElement(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_ifr);
        if (!oFr) {
            // Not found -> Create it
            var oParent = this.getSubElement(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadPlaceHolder);
            if (oParent) {
                var oFr = document.createElement("ifr" + "ame");
                oFr.setAttribute("fr" + "ameBorder", "0");
                oFr.setAttribute("id", oFrId);
                oFr.setAttribute("name", oFrId);
                oFr.style.width = this.loadData.buttonSize_width + "px";
                oFr.style.height = this.loadData.buttonSize_height + "px";

                oParent.appendChild(oFr);
                oFr.setAttribute("src", this.loadData.uploadUrl);
            }
        }

        // Initialize some values
        this.stopUploadMonitor();
        this.uploadInProgress = false;
        this.wait = false;
        this.statusMessageVisible = !this.get_hasImage();
        this.updateUI();

        if (this.loadData.autoOpenImageEditPopup) {
            // Auto open the image edit popup
            this.loadData.autoOpenImageEditPopup = false;

            var otherPopup = CodeCarvings.Wcs.Piczard.PictureTrimmer.popup_getOpen();
            if (otherPopup == null) {
                // No popup is open

                var enableEdit = true;
                if (this.loadData.autoDisableImageEdit) {
                    var hasFlashPlayer = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.get_hasFlashPlayer();
                    if (!hasFlashPlayer) {
                        enableEdit = false;
                    }
                }

                if (enableEdit) {
                    this.openImageEditPopup();
                }
            }
        }
    },

    updateUI: function () {
        if (this.statusMessageVisible) {
            this.setElementDisplay(this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_content_statusMessage), "inline");
            this.setElementDisplay(this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_content_preview), "none");
        }
        else {
            this.setElementDisplay(this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_content_statusMessage), "none");
            this.setElementDisplay(this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_content_preview), "inline");
        }

        if (this.wait) {
            this.setElementDisabled(this.loadData.btnEditClientId, true);
            this.setElementDisabled(this.loadData.btnRemoveClientId, true);

            this.setElementDisplay(this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadContainer_0), "block");
            this.setElementDisplay(this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadContainer_1), "none");
        }
        else {
            var me = this;
            var hasImage = this.get_hasImage();
            var enableEdit = this.loadData.enableEdit;
            var enableRemove = this.loadData.enableRemove;

            if (!hasImage) {
                enableEdit = false;
                enableRemove = false;
            }

            if (enableEdit) {
                if (this.loadData.autoDisableImageEdit) {
                    var hasFlashPlayer = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.get_hasFlashPlayer();
                    if (!hasFlashPlayer) {
                        enableEdit = false;
                    }
                }
            }

            this.setElementDisabled(this.loadData.btnEditClientId, !enableEdit);
            var oBtnEdit = document.getElementById(this.loadData.btnEditClientId);
            if (oBtnEdit) {
                oBtnEdit.onclick = function () {
                    if (enableEdit) {
                        me.openImageEditPopup();
                    }
                    return false;
                }
            }
            var oHlPictureImageEdit = document.getElementById(this.loadData.hlPictureImageEditId);
            if (oHlPictureImageEdit) {
                oHlPictureImageEdit.onclick = function () {
                    if (enableEdit) {
                        me.openImageEditPopup();
                    }
                    return false;
                }
                oHlPictureImageEdit.style.cursor = enableEdit ? "pointer" : "default";
            }

            this.setElementDisabled(this.loadData.btnRemoveClientId, !hasImage);
            var oBtnRemove = document.getElementById(this.loadData.btnRemoveClientId);
            if (oBtnRemove) {
                oBtnRemove.onclick = function () {
                    if (enableRemove) {
                        me.removeImage();
                    }
                    return false;
                }
            }

            this.setElementDisplay(this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadContainer_0), "none");
            this.setElementDisplay(this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadContainer_1), "block");
        }

        this.setElementDisplay(this.loadData.btnBrowseDisabledClientId, "inline");
        this.setElementDisplay(this.loadData.btnCancelUploadClientId, "none");
    },

    setElementDisplay: function (id, value) {
        var oElement = document.getElementById(id);
        if (oElement) {
            oElement.style.display = value;
        }
    },

    setElementDisabled: function (id, value) {
        var oElement = document.getElementById(id);
        if (oElement) {
            oElement.disabled = value;
        }
    },

    displayStatusMessage: function (value) {
        this.statusMessageVisible = true;

        var oStatusMessage = this.getSubElement(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_content_statusMessage);
        if (oStatusMessage) {
            oStatusMessage.innerHTML = "<div style=\"padding:5px;\">" + value + "</div>";
        }

        this.updateUI();
    },

    setWaitMode: function () {
        this.wait = true;
        this.displayStatusMessage(this.loadData.statusMessage_Wait);
    },

    get_hasImage: function () {
        var popupPictureTrimmer = CodeCarvings.Wcs.Piczard.PictureTrimmer.getControl(this.loadData.popupPictureTrimmerClientId);
        if (popupPictureTrimmer) {
            return popupPictureTrimmer.get_imageLoaded();
        }
        else {
            return false;
        }
    },

    get_uploadInProgress: function () {
        return this.uploadInProgress;
    },

    get_autoSetPageBlockSubmit: function () {
        return this.autoSetPageBlockSubmit;
    },
    set_autoSetPageBlockSubmit: function (value) {
        if (value) {
            this.autoSetPageBlockSubmit = true;
        }
        else {
            this.autoSetPageBlockSubmit = false;
        }
    },

    onUploadReady: function () {
        this.setElementDisabled(this.loadData.btnBrowseClientId, false);
    },

    onUploadStart: function () {
        this.startUploadMonitor();
        this.uploadInProgress = true;
        this.wait = true;
        this.updateUI();

        // Update the cancel upload interface
        if (this.loadData.enableCancelUpload) {
            // The user can cancel the current upload
            this.setElementDisabled(this.loadData.btnCancelUploadClientId, true);

            this.setElementDisplay(this.loadData.btnBrowseDisabledClientId, "none");
            this.setElementDisplay(this.loadData.btnCancelUploadClientId, "inline");

            var me = this;
            window.setTimeout(function () {
                me.setElementDisabled(me.loadData.btnCancelUploadClientId, false);
            }, this.cancelUploadEnableDelay);
        }
    },

    onUploadEnd: function () {
        this.stopUploadMonitor();
        this.uploadInProgress = false;
    },

    onUploadSuccess: function () {
        this.onUploadEnd();

        this.executeUploadAction();
    },

    onUploadError: function (message) {
        this.onUploadEnd();

        if (this.loadData.dup) {
            // Display the upload error (if available)
            this.displayUploadError(message);
        }
        else {
            // Perform the postback to display the standard error message
            this.executeUploadAction();
        }
    },

    executeUploadAction: function () {
        this.executeAction("upload");
    },

    executeAction: function (act) {
        var oAct = document.getElementById(this.loadData.hfActClientId);
        if (oAct) {
            oAct.value = act;
        }

        if (CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.get_uploadInProgress()) {
            // Another control is uploading...
            // Do not postback, set in wait mode
            this.setWaitMode();
            return;
        }

        // Ensure that the "__doPostBack" ASP.NET method is defined
        CodeCarvings.Wcs.Piczard.Shared.Helpers.UIHelper.ensureAspNetPostBackFunction();

        if (this.get_autoSetPageBlockSubmit()) {
            // Ensure that the postback is not blocked due to validation
            Page_BlockSubmit = false;
        }

        // Perform the postback
        eval(this.loadData.btnPostBack_PostBackEventReference);
    },

    cancelUpload: function () {
        var oFrId = this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_ifr);
        var oFr = this.getSubElement(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_ifr);
        if (oFr) {
            var ex;
            try {
                oFr.parentNode.removeChild(oFr);
            }
            catch (ex) {
            }
        }

        this.onUploadEnd();
        this.executeAction("calcelUpload");
    },

    removeImage: function () {
        this.executeAction("remove");
    },

    displayUploadError: function (message) {
        if (!this.uploadErrorLightbox) {
            // Initialize the LightBox
            this.uploadErrorLightbox = new CodeCarvings.Wcs.Piczard.Shared.UI.LightBox(this.getSubElementId(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadErrorLightbox));
        }

        var windowWidth = 700;
        var windowHeight = 450;
        var parentElement = this.uploadErrorLightbox.open(windowWidth, windowHeight);

        var html = "<div style=\"width:" + windowWidth + "px; height:" + windowHeight + "px; overflow:scroll;\"><div style=\"font-size:12px; color:#cc0000; padding:10px;\">" + message;
        html += "\r\n<br /><br /><input type=\"button\" value=\"  Continue &raquo;  \" onclick=\"CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.hideUploadError('" + this.id + "'); return false;\" /><br />\r\n";
        html += "</div></div>";
        parentElement.innerHTML = html
    },

    hideUploadError: function () {
        if (this.uploadErrorLightbox) {
            this.uploadErrorLightbox.close();
        }

        // Perform the postback to display the standard error message
        this.executeUploadAction();
    },

    startUploadMonitor: function () {
        this.stopUploadMonitor();

        var f = CodeCarvings.Wcs.Piczard.Shared.Helpers.ObjectHelper.getInstanceFunctionReference(this, "uploadMonitorFunction");
        this.uploadMonitorTimeoutId = window.setTimeout(f, this.uploadMonitorDelay);
    },

    stopUploadMonitor: function () {
        if (this.uploadMonitorTimeoutId) {
            window.clearTimeout(this.uploadMonitorTimeoutId);
            this.uploadMonitorTimeoutId = 0;
        }
    },

    uploadMonitorFunction: function () {
        if (!this.xmlHttp) {
            // XMLHttpRequest not yet initialized
            if (window.XMLHttpRequest) {
                this.xmlHttp = new XMLHttpRequest();
            }
            else {
                this.xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
            }
        }

        // Add a timestamp to the url to ensure that cache is not used
        var url = this.loadData.uploadMonitorUrl + "&ts=" + CodeCarvings.Wcs.Piczard.Shared.Helpers.TimeHelper.getTimeStamp();
        this.xmlHttp.open("GET", url, false);
        this.xmlHttp.send();

        var xmlLoadError = false;
        if (this.xmlHttp.status) {
            if (this.xmlHttp.status != 200) {
                xmlLoadError = true;
            }
        }

        if (!xmlLoadError) {
            var xmlDoc = this.xmlHttp.responseXML;
            if (xmlDoc) {
                var nodes = xmlDoc.getElementsByTagName("uploadMonitor");
                if ((nodes) && (nodes.length > 0)) {
                    var uploadMonitorNode = nodes[0];
                    if (uploadMonitorNode) {
                        var stateAttribute = uploadMonitorNode.getAttribute("state");
                        if (stateAttribute == "2") {
                            // Upload success
                            this.onUploadSuccess();
                            return;
                        }
                        if (stateAttribute == "3") {
                            // Upload error
                            var message = ""
                            if (uploadMonitorNode.firstChild) {
                                if (uploadMonitorNode.firstChild.nodeValue) {
                                    message = uploadMonitorNode.firstChild.nodeValue;
                                }
                            }
                            if (!message) {
                                message = "Unknown error.";
                            }

                            this.onUploadError(message);
                            return;
                        }
                    }
                }
            }
        }

        // Continue the monitorning process...
        this.startUploadMonitor();
    },

    openImageEditPopup: function () {
        CodeCarvings.Wcs.Piczard.PictureTrimmer.popup_open(this.loadData.popupPictureTrimmerClientId, this.loadData.imageEditPopupSize_width, this.loadData.imageEditPopupSize_height);
    },

    onImageEditBeforePopupOpen: function (sender, args) {
        // Check if the "configuration" UI must be shown
        var ddlConfigurations = document.getElementById(this.loadData.ddlConfigurationsClientId);
        if ((ddlConfigurations) && (ddlConfigurations.options.length > 0)) {
            this.displayConfigurations = true;
        }
        else {
            this.displayConfigurations = false;
        }

        if (this.displayConfigurations) {
            args.reservedWindowHeight = 44;

            var oPopupExt = this.getSubElement(CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_popupExt);
            args.additionalTopElements[args.additionalTopElements.length] = oPopupExt;
        }
    },

    onImageEditAfterPopupClose: function (sender, args) {
        if (this.configurationChange) {
            // Configuration changed
            this.configurationChange = false;
            this.executeAction("configuration");
        }
        else {
            if (args.saveChanges) {
                this.executeAction("edit");
            }
        }
    },

    onConfigurationChange: function () {
        // Close the popup and save the changes
        this.configurationChange = true;
        CodeCarvings.Wcs.Piczard.PictureTrimmer.popup_close(true);
    }
}

// Consts
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_ifr = "ifr";
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadContainer_0 = "uploadContainer_0";
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadContainer_1 = "uploadContainer_1";
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadPlaceHolder = "uploadPlaceHolder";
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_uploadErrorLightbox = "uploadErrorLightbox";
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_content_statusMessage = "content_statusMessage";
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_content_preview = "content_preview";
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_popupExtContainer = "popupExtContainer";
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.subElementSubId_popupExt = "popupExt";

// If necessary, create a SimpleImageUpload control and then load it
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.loadControl = function (id, loadData) {
    var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.getControl(id);
    if (control == null) {
        // Control NOT found -> create it
        control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.createControl(id);
    }

    // Load the control
    control.load(loadData);

    // Initialize the UI
    control.initializeUI();

    return control;
}

// Get a SimpleImageUpload control
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.getControl = function (id) {
    var controls = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.controls;
    if (!id) {
        if (controls.length > 0) {
            return controls[0];
        }
        else {
            return null;
        }
    }

    for (var i = 0; i < controls.length; i++) {
        var control = controls[i];
        if (control) {
            if (control.id == id) {
                return control;
            }
        }
    }

    return null;
}

// Create a new SimpleImageUpload control
CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.createControl = function (id) {
    var control = new CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload(id);

    var controls = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.controls;
    var index = -1;
    for (var i = 0; i < controls.length; i++) {
        var oldControl = controls[i];
        if (oldControl.id == id) {
            index = i;
            break;
        }
    }

    if (index >= 0) {
        controls[index] = control;
    }
    else {
        CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.controls[CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.controls.length] = control;
    }

    return control;
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.openImageEditPopup = function (id) {
    var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.getControl(id);
    if (control == null) {
        // Control NOT found
        return;
    }

    control.openImageEditPopup();
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.onImageEditBeforePopupOpen = function (sender, args) {
    // Execute the control functions
    sender.parentControl.onImageEditBeforePopupOpen(sender, args);
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.onImageEditAfterPopupOpen = function (sender, args) {
    // Execute the control functions
    sender.parentControl.onImageEditAfterPopupOpen(sender, args);
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.onImageEditAfterPopupClose = function (sender, args) {
    // Execute the control functions
    sender.parentControl.onImageEditAfterPopupClose(sender, args);
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.onConfigurationChange = function (id) {
    var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.getControl(id);
    if (control == null) {
        // Control NOT found
        return true;
    }

    control.onConfigurationChange();
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.cancelUpload = function (id) {
    var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.getControl(id);
    if (control == null) {
        // Control NOT found
        return true;
    }

    return control.cancelUpload();
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.removeImage = function (id) {
    var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.getControl(id);
    if (control == null) {
        // Control NOT found
        return;
    }

    control.removeImage();
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.hideUploadError = function (id) {
    var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.getControl(id);
    if (control == null) {
        // Control NOT found
        return;
    }

    return control.hideUploadError();
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.get_hasImage = function (id) {
    var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.getControl(id);
    if (control == null) {
        // Control NOT found
        return null;
    }

    return control.get_hasImage();
}

CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.get_uploadInProgress = function (id) {
    if (id) {
        var control = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.getControl(id);
        if (control == null) {
            // Control NOT found
            return null;
        }

        return control.get_uploadInProgress();
    }
    else {
        // Return true if there is at least a control that is uploading

        var controls = CodeCarvings.Wcs.Piczard.Upload.SimpleImageUpload.controls;
        for (var i = 0; i < controls.length; i++) {
            var control = controls[i];
            if (control) {
                if (control.get_uploadInProgress()) {
                    return true;
                }
            }
        }

        // No control is uploading...
        return false;
    }
}
