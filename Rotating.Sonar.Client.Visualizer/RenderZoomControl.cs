namespace Rotating.Sonar.Client.Visualizer;

public sealed class RenderZoomControl
{
    public const float MinScale = 0.25f;
    public const float MaxScale = 4f;

    private const float FactorPerStep = 1.12f;
    private const float MaxWheelExponent = 5f;

    private readonly IZoomable zoomSubject;

    public RenderZoomControl(IZoomable zoomSubject)
    {
        this.zoomSubject = zoomSubject;
    }

    public void ZoomIn()
    {
        this.zoomSubject.SetZoom(Math.Min(MaxScale, this.zoomSubject.ZoomScale * FactorPerStep));
    }

    public void ZoomOut()
    {
        this.zoomSubject.SetZoom(Math.Max(MinScale, this.zoomSubject.ZoomScale / FactorPerStep));
    }

    public void ResetZoom()
    {
        this.zoomSubject.SetZoom(1f);
    }

    public void ZoomWheel(float deltaY)
    {
        if (deltaY == 0.0f)
        {
            return;
        }

        float signedMag = Math.Sign(deltaY) * Math.Min(Math.Abs(deltaY), MaxWheelExponent);
        float newScale = this.zoomSubject.ZoomScale * MathF.Pow(FactorPerStep, signedMag);
        this.zoomSubject.SetZoom(newScale);
    }
}
