namespace Rotating.Sonar.Client.Tests.Zoom;

using Rotating.Sonar.Client.Common.Settings;
using Rotating.Sonar.Client.Common.Settings.Sections;
using Rotating.Sonar.Client.Common.Zoom;

[TestFixture]
public sealed class RenderZoomControl_Tests
{
    private sealed class FakeZoomable : IZoomable
    {
        public float ZoomScale { get; private set; } = 1f;

        public int SetCount { get; private set; }

        public void SetZoom(float zoomScale)
        {
            this.ZoomScale = zoomScale;
            this.SetCount++;
        }

        public void SeedZoom(float zoomScale) => this.ZoomScale = zoomScale;
    }

    private static ZoomSettings CreateSettings() => new()
    {
        MinScale = 0.25f,
        MaxScale = 4f,
        DefaultScale = 1f,
        FactorPerStep = 2f,
        MaxWheelExponent = 3f,
    };

    [Test]
    public void ZoomIn_FromOne_MultipliesByFactorPerStep()
    {
        var fake = new FakeZoomable();
        var control = new RenderZoomControl(fake, CreateSettings());

        control.ZoomIn();

        Assert.That(fake.ZoomScale, Is.EqualTo(2f));
    }

    [Test]
    public void ZoomIn_AtMaxScale_ClampsToMaxScale()
    {
        var fake = new FakeZoomable();
        fake.SeedZoom(4f);
        var control = new RenderZoomControl(fake, CreateSettings());

        control.ZoomIn();

        Assert.That(fake.ZoomScale, Is.EqualTo(4f));
    }

    [Test]
    public void ZoomOut_FromOne_DividesByFactorPerStep()
    {
        var fake = new FakeZoomable();
        var control = new RenderZoomControl(fake, CreateSettings());

        control.ZoomOut();

        Assert.That(fake.ZoomScale, Is.EqualTo(0.5f));
    }

    [Test]
    public void ZoomOut_AtMinScale_ClampsToMinScale()
    {
        var fake = new FakeZoomable();
        fake.SeedZoom(0.25f);
        var control = new RenderZoomControl(fake, CreateSettings());

        control.ZoomOut();

        Assert.That(fake.ZoomScale, Is.EqualTo(0.25f));
    }

    [Test]
    public void ResetZoom_SetsDefaultScale()
    {
        var fake = new FakeZoomable();
        fake.SeedZoom(3.5f);
        var control = new RenderZoomControl(fake, CreateSettings());

        control.ResetZoom();

        Assert.That(fake.ZoomScale, Is.EqualTo(1f));
    }

    [Test]
    public void ZoomWheel_ZeroDelta_DoesNotChangeScale()
    {
        var fake = new FakeZoomable();
        var control = new RenderZoomControl(fake, CreateSettings());

        control.ZoomWheel(0f);

        Assert.That(fake.SetCount, Is.EqualTo(0));
        Assert.That(fake.ZoomScale, Is.EqualTo(1f));
    }

    [Test]
    public void ZoomWheel_PositiveDelta_MultipliesByFactorPow()
    {
        var fake = new FakeZoomable();
        var control = new RenderZoomControl(fake, CreateSettings());

        control.ZoomWheel(1f);

        Assert.That(fake.ZoomScale, Is.EqualTo(MathF.Pow(2f, 1f)).Within(1e-5f));
    }

    [Test]
    public void ZoomWheel_HugePositiveDelta_ClampsExponentToMaxWheelExponent()
    {
        // RenderZoomControl caps the exponent at MaxWheelExponent (3 here) and defers final
        // MinScale/MaxScale clamping to IZoomable.SetZoom. With FactorPerStep=2 the call site
        // should send 1 * 2^3 = 8 to the subject — even though MaxScale=4 — verifying the
        // boundary belongs to the subject rather than the control.
        var fake = new FakeZoomable();
        var control = new RenderZoomControl(fake, CreateSettings());

        control.ZoomWheel(100f);

        Assert.That(fake.ZoomScale, Is.EqualTo(MathF.Pow(2f, 3f)).Within(1e-5f));
    }

    [Test]
    public void ZoomWheel_NegativeDelta_DividesByFactorPow()
    {
        var fake = new FakeZoomable();
        var control = new RenderZoomControl(fake, CreateSettings());

        control.ZoomWheel(-1f);

        Assert.That(fake.ZoomScale, Is.EqualTo(MathF.Pow(2f, -1f)).Within(1e-5f));
    }
}
