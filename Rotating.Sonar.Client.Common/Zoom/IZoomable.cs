namespace Rotating.Sonar.Client.Common.Zoom;

public interface IZoomable
{
    float ZoomScale { get; }

    void SetZoom(float zoomScale);
}
