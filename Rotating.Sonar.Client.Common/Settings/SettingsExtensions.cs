namespace Rotating.Sonar.Client.Common.Settings;

using System.Text.RegularExpressions;
using Rotating.Sonar.Client.Common.Settings.Sections;

public static class SettingsExtensions
{
    public static Regex CreateLineRegex(this SerialSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return new Regex(
            settings.LinePattern,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(settings.LinePatternMatchTimeoutMilliseconds));
    }
}
