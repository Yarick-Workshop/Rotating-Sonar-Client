namespace Rotating.Sonar.Client.Visualizer;

public interface IZoomable
{
    float ZoomScale { get; }

    void SetZoom(float zoomScale);
}
