namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;

public static class SettingsValidator
{
    // TODO, polish add unit tests for this
    public static void ValidateRecursively(object settings, string path = "")
    {
        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(settings, context, results, validateAllProperties: true))
        {
            string currentPath = string.IsNullOrEmpty(path) ? settings.GetType().Name : path;
            string errors = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new InvalidOperationException($"Settings validation failed at '{currentPath}': {errors}");
        }

        foreach (var property in settings.GetType().GetProperties())
        {
            if (property.PropertyType.Namespace?.StartsWith("Rotating.Sonar") != true)
            {
                continue;
            }

            object? nested = property.GetValue(settings);
            if (nested is null)
            {
                continue;
            }

            string nestedPath = string.IsNullOrEmpty(path)
                ? property.Name
                : $"{path}.{property.Name}";
            ValidateRecursively(nested, nestedPath);
        }
    }
}
