Imports System.Drawing
Imports CodeCarvings.Piczard
Imports CodeCarvings.Piczard.Filters.Colors
Imports CodeCarvings.Piczard.Filters.Watermarks

Partial Class test_default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If (Not Me.IsPostBack) Then
            Me.ImageUpload1.CropConstraint = New FixedCropConstraint(300, 200)
            Me.ImageUpload1.PreviewFilter = New FixedResizeConstraint(200, 200, Color.Black)

            Me.ImageUpload2.PostProcessingFilter = New ScaledResizeConstraint(300, 300)
        End If
    End Sub

    Protected Sub btnLoadImage_Click(sender As Object, e As EventArgs) Handles btnLoadImage.Click
        Me.ImageUpload1.LoadImageFromFileSystem("~/App_Data/source/trevi1.jpg")
    End Sub
End Class
