namespace Rotating.Sonar.Client.Common.Zoom;

using Rotating.Sonar.Client.Common.Settings;
using Rotating.Sonar.Client.Common.Settings.Sections;

public sealed class RenderZoomControl
{
    private readonly IZoomable zoomSubject;
    private readonly ZoomSettings zoomSettings;

    public RenderZoomControl(IZoomable zoomSubject, ZoomSettings zoomSettings)
    {
        this.zoomSubject = zoomSubject;
        this.zoomSettings = zoomSettings;
    }

    public void ZoomIn()
    {
        this.zoomSubject.SetZoom(Math.Min(this.zoomSettings.MaxScale, this.zoomSubject.ZoomScale * this.zoomSettings.FactorPerStep));
    }

    public void ZoomOut()
    {
        this.zoomSubject.SetZoom(Math.Max(this.zoomSettings.MinScale, this.zoomSubject.ZoomScale / this.zoomSettings.FactorPerStep));
    }

    public void ResetZoom()
    {
        this.zoomSubject.SetZoom(this.zoomSettings.DefaultScale);
    }

    public void ZoomWheel(float deltaY)
    {
        if (deltaY == 0.0f)
        {
            return;
        }

        float signedMag = Math.Sign(deltaY) * Math.Min(Math.Abs(deltaY), this.zoomSettings.MaxWheelExponent);
        float newScale = this.zoomSubject.ZoomScale * MathF.Pow(this.zoomSettings.FactorPerStep, signedMag);
        this.zoomSubject.SetZoom(newScale);
    }
}
