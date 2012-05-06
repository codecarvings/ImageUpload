using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using CodeCarvings.Piczard;

public partial class test_default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            this.ImageUpload1.CropConstraint = new FixedCropConstraint(300, 200);
            this.ImageUpload1.PreviewResizeConstraint = new FixedResizeConstraint(200, 200, Color.Black);

            this.ImageUpload2.PostProcessingFilter = new ScaledResizeConstraint(300, 300);
        }
    }
}
