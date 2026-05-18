namespace Rotating.Sonar.Client.Tests.Settings;

using Rotating.Sonar.Client.Common.Settings;
using Rotating.Sonar.Client.Tests.Settings.Fixtures;

[TestFixture]
public sealed class SettingsValidator_CustomSettings_Tests
{
    [Test]
    public void ValidateRecursively_CustomAnnotatedSettingsValidDefaults_DoesNotThrow()
    {
        var settings = new CustomAnnotatedSettings();

        Assert.DoesNotThrow(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomAnnotatedSettings)));
    }

    [Test]
    public void ValidateRecursively_CustomAnnotatedSettingsNullRequiredLabel_ThrowsWithRootPath()
    {
        var settings = new CustomAnnotatedSettings { Label = null! };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomAnnotatedSettings)));

        Assert.That(ex!.Message, Does.Contain($"at '{nameof(CustomAnnotatedSettings)}.Label':"));
    }

    [Test]
    public void ValidateRecursively_CustomAnnotatedSettingsInvalidPort_ThrowsWithRangeMessage()
    {
        var settings = new CustomAnnotatedSettings { Port = 0 };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomAnnotatedSettings)));

        Assert.That(ex!.Message, Does.Contain($"at '{nameof(CustomAnnotatedSettings)}.Port':"));
    }

    [Test]
    public void ValidateRecursively_CustomAnnotatedSettingsInvalidNestedLevel_ThrowsWithSectionPath()
    {
        var settings = new CustomAnnotatedSettings();
        settings.Section.Level = 99;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomAnnotatedSettings)));

        Assert.That(ex!.Message, Does.Contain(
            $"at '{nameof(CustomAnnotatedSettings)}.{nameof(CustomAnnotatedSettings.Section)}.{nameof(CustomAnnotatedNestedSection.Level)}':"));
    }

    [Test]
    public void ValidateRecursively_CustomAnnotatedSettingsInvalidNestedStringLength_ThrowsWithSectionPath()
    {
        var settings = new CustomAnnotatedSettings();
        settings.Section.Code = "x";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomAnnotatedSettings)));

        Assert.That(ex!.Message, Does.Contain(
            $"at '{nameof(CustomAnnotatedSettings)}.{nameof(CustomAnnotatedSettings.Section)}.{nameof(CustomAnnotatedNestedSection.Code)}':"));
    }

    [Test]
    public void ValidateRecursively_CustomAnnotatedSettingsInvalidNestedBounds_ThrowsWithBoundsPath()
    {
        var settings = new CustomAnnotatedSettings();
        settings.Bounds.Low = 20;
        settings.Bounds.High = 10;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomAnnotatedSettings)));

        Assert.That(ex!.Message, Does.Contain(
            $"at '{nameof(CustomAnnotatedSettings)}.{nameof(CustomAnnotatedSettings.Bounds)}.{nameof(CustomCrossValidatedSettings.Low)}':"));
    }

    [Test]
    public void ValidateRecursively_CustomCrossValidatedSettingsLowGreaterThanHighAndEmptyPath_MessageUsesTypeName()
    {
        var settings = new CustomCrossValidatedSettings { Low = 5, High = 1 };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, string.Empty));

        Assert.That(ex!.Message, Does.Contain($"at '{nameof(CustomCrossValidatedSettings)}.Low':"));
    }

    [Test]
    public void ValidateRecursively_CustomRequiredNestedParentNullRequiredDetail_ThrowsWithDetailPropertyPath()
    {
        var settings = new CustomRequiredNestedParent { Detail = null };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomRequiredNestedParent)));

        Assert.That(ex!.Message, Does.Contain(
            $"at '{nameof(CustomRequiredNestedParent)}.{nameof(CustomRequiredNestedParent.Detail)}': Detail section is required."));
    }

    [Test]
    public void ValidateRecursively_CustomRequiredNestedParentPopulatedDetail_DoesNotThrow()
    {
        var settings = new CustomRequiredNestedParent { Detail = new CustomAnnotatedNestedSection() };

        Assert.DoesNotThrow(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomRequiredNestedParent)));
    }

    [Test]
    public void ValidateRecursively_CustomSettingsWithListMemberNonEmptyList_ThrowsIEnumerableNotSupportedWithSectionsPath()
    {
        var settings = new CustomSettingsWithListMember();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomSettingsWithListMember)));

        Assert.That(ex!.Message, Does.Contain(
            $"Settings validation failed at '{nameof(CustomSettingsWithListMember)}.{nameof(CustomSettingsWithListMember.Sections)}': IEnumerable members are not supported."));
    }

    [Test]
    public void ValidateRecursively_CustomSettingsWithArrayMemberNonEmptyArray_ThrowsIEnumerableNotSupportedWithBlocksPath()
    {
        var settings = new CustomSettingsWithArrayMember();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomSettingsWithArrayMember)));

        Assert.That(ex!.Message, Does.Contain(
            $"Settings validation failed at '{nameof(CustomSettingsWithArrayMember)}.{nameof(CustomSettingsWithArrayMember.Blocks)}': IEnumerable members are not supported."));
    }

    [Test]
    public void ValidateRecursively_CustomSettingsWithEnumerablePropertyNonEmptyList_ThrowsIEnumerableNotSupportedWithNumbersPath()
    {
        var settings = new CustomSettingsWithEnumerableProperty();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomSettingsWithEnumerableProperty)));

        Assert.That(ex!.Message, Does.Contain(
            $"Settings validation failed at '{nameof(CustomSettingsWithEnumerableProperty)}.{nameof(CustomSettingsWithEnumerableProperty.Numbers)}': IEnumerable members are not supported."));
    }

    [Test]
    public void ValidateRecursively_CustomSettingsSelfReferentialOwnerNull_DoesNotThrow()
    {
        var settings = new CustomSettingsSelfReferential();

        Assert.DoesNotThrow(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomSettingsSelfReferential)));
    }

    [Test]
    public void ValidateRecursively_CustomSettingsSelfReferentialOwnerPointsToSelf_ThrowsCircularReferenceAtOwnerPath()
    {
        var settings = new CustomSettingsSelfReferential();
        settings.Owner = settings;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomSettingsSelfReferential)));

        Assert.That(ex!.Message, Does.Contain(
            $"Settings validation failed at '{nameof(CustomSettingsSelfReferential)}.{nameof(CustomSettingsSelfReferential.Owner)}': Circular reference detected"));
    }

    [Test]
    public void ValidateRecursively_CustomSettingsSelfReferentialParentPointsToSelf_ThrowsCircularReferenceAtParentPath()
    {
        var settings = new CustomSettingsSelfReferentialParent();
        settings.Parent = settings;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomSettingsSelfReferentialParent)));

        Assert.That(ex!.Message, Does.Contain(
            $"Settings validation failed at '{nameof(CustomSettingsSelfReferentialParent)}.{nameof(CustomSettingsSelfReferentialParent.Parent)}': Circular reference detected"));
    }

    [Test]
    public void ValidateRecursively_TypeWithPublicInstanceField_ThrowsPropertiesOnlyMessage()
    {
        var settings = new CustomIllegalPublicFieldSettings();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomIllegalPublicFieldSettings)));

        Assert.That(ex!.Message, Does.Contain(
            $"Settings validation failed at '{nameof(CustomIllegalPublicFieldSettings)}': Type '{typeof(CustomIllegalPublicFieldSettings).FullName}' declares public instance field(s): {nameof(CustomIllegalPublicFieldSettings.DisallowedPublicField)}. Settings types must use properties only; public fields are not supported."));
    }

    [Test]
    public void ValidateRecursively_CustomGraphLoopRootTwoNodeCycle_ThrowsCircularReferenceAtReentryPath()
    {
        var nodeA = new GraphLoopNodeA();
        var nodeB = new GraphLoopNodeB { A = nodeA };
        nodeA.B = nodeB;
        var root = new CustomGraphLoopRoot { Entry = nodeA };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(root, nameof(CustomGraphLoopRoot)));

        Assert.That(ex!.Message, Does.Contain(
            $"Settings validation failed at '{nameof(CustomGraphLoopRoot)}.{nameof(CustomGraphLoopRoot.Entry)}.{nameof(GraphLoopNodeA.B)}.{nameof(GraphLoopNodeB.A)}': Circular reference detected"));
    }

    [Test]
    public void ValidateRecursively_CustomSettingsWithFloatColor4ValidColor_DoesNotThrow()
    {
        var settings = new CustomSettingsWithFloatColor4();

        Assert.DoesNotThrow(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(CustomSettingsWithFloatColor4)));
    }

}
