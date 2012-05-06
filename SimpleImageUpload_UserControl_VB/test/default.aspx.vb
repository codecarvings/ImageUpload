Imports System.Drawing
Imports CodeCarvings.Piczard

Partial Class test_default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If (Not Me.IsPostBack) Then
            Me.ImageUpload1.CropConstraint = New FixedCropConstraint(300, 200)
            Me.ImageUpload1.PreviewResizeConstraint = New FixedResizeConstraint(200, 200, Color.Black)

            Me.ImageUpload2.PostProcessingFilter = New ScaledResizeConstraint(300, 300)
        End If
    End Sub
End Class
