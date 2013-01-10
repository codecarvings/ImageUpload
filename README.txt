Piczard - .NET Image Manipulation Library
###  Image Upload  ###

This package includes SimpleImageUpload, an advanced image upload
user control with a powerful and intuitive image manipulation interface.

<ccPiczardUC:SimpleImageUpload runat="server" />

SimpleImageUpload user control allows the final user to:

- Upload an image to the server
- Interactively crop, resize, rotate/flip and color adjust the uploaded image (WYSIWYG) 
- Automatically apply post-processing filters (e.g.: image resize + watermark)
- View a preview of the uploaded image
- Edit a previously uploaded image maintaining the image parameters selected in the past (crop coordinates, resize factor, rotation angle, color adjustments, etc.. )
 
Since this control is provided as ASCX, the full source code is available.
You are free to change/edit it according to your needs.


Quick links:
---------------

- For more information please visit:
  http://piczard.com
  
- Online demo:
  http://piczard.com/demos/imageUpload 

- Piczard installer (examples and help file are included):
  http://piczard.com/download

- Examples (Git):
  https://github.com/CodeCarvings/Piczard.Examples

- Changes log:
  http://piczard.com/docs/help/v1/online/?Changes_Log.html
  
- Online documentation:
  http://piczard.com/docs/help/v1/online/?SimpleImageUpload_ASCX_User_Control_Overview.html
 
- Support forum:
  http://forum.codecarvings.com/products/piczard
  
- NuGet package:
  http://www.nuget.org/packages/CodeCarvings.Piczard.ImageUpload  
   
- Git repository:
  https://github.com/CodeCarvings/ImageUpload

  
*********
SimpleImageUpload.ascx does not require Ajax.
However, since it can perform some page postbacks, for optimal use it is suggested to insert it within an UpdatePanel.

Please note that the ASCX control is part of "Web Site" projects.
If you want to use it in a "Web Application" project you need to convert the control files.
From within Microsoft Visual Studio open your "Web Application" project.
In the Solution Explorer right-click the folder that contains the ASCX control and then click Convert to Web Application.

For more information about "Web Site" and "Web Application" projects please see:

http://msdn.microsoft.com/en-us/library/aa983476.aspx 
http://msdn.microsoft.com/en-us/library/dd547590.aspx 
*********  
  
  
============================================================
Image Upload package is released under the terms of BSD License.

Copyright (c) 2011-2013 Sergio Turolla
All rights reserved.

Redistribution and use in source and binary forms, with or
without modification, are permitted provided that the 
following conditions are met:

- Redistributions of source code must retain the above 
  copyright notice, this list of conditions and the 
  following disclaimer.
- Redistributions in binary form must reproduce the above
  copyright notice, this list of conditions and the 
  following disclaimer in the documentation and/or other
  materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
SUCH DAMAGE.
============================================================

