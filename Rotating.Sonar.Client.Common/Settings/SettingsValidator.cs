namespace Rotating.Sonar.Client.Common.Settings;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class SettingsValidator
{
    public static void ValidateRecursively(object settings, string path = "")
    {
        ArgumentNullException.ThrowIfNull(settings);
        ValidateCore(settings, path, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    private static void ValidateCore(object settings, string path, HashSet<object> visited)
    {
        Type settingsType = settings.GetType();
        string basePath = string.IsNullOrEmpty(path) ? settingsType.Name : path;

        if (!settingsType.IsValueType && !visited.Add(settings))
        {
            throw new InvalidOperationException(
                $"Settings validation failed at '{basePath}': Circular reference detected; the same object instance was visited more than once.");
        }

        FieldInfo[] publicFields = settingsType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        if (publicFields.Length > 0)
        {
            string fieldList = string.Join(", ", publicFields.Select(f => f.Name));
            throw new InvalidOperationException(
                $"Settings validation failed at '{basePath}': Type '{settingsType.FullName}' declares public instance field(s): {fieldList}. " +
                "Settings types must use properties only; public fields are not supported.");
        }

        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(settings, context, results, validateAllProperties: true))
        {
            IEnumerable<string> entries = results.SelectMany(r => FormatResult(r, basePath));
            throw new InvalidOperationException($"Settings validation failed: {string.Join("; ", entries)}");
        }

        foreach (PropertyInfo property in EnumerateCandidateProperties(settingsType))
        {
            object? nested = property.GetValue(settings);
            string nestedPath = BuildMemberPath(path, property.Name);
            ProcessNestedValue(nested, nestedPath, visited);
        }
    }

    private static void ProcessNestedValue(object? nested, string nestedPath, HashSet<object> visited)
    {
        if (nested is null)
        {
            return;
        }

        if (nested is IEnumerable && nested is not string)
        {
            throw new InvalidOperationException(
                $"Settings validation failed at '{nestedPath}': IEnumerable members are not supported.");
        }

        if (IsLeafType(nested.GetType()))
        {
            return;
        }

        ValidateCore(nested, nestedPath, visited);
    }

    private static string BuildMemberPath(string path, string memberName) =>
        string.IsNullOrEmpty(path) ? memberName : $"{path}.{memberName}";

    private static IEnumerable<string> FormatResult(ValidationResult result, string basePath)
    {
        string[] memberNames = result.MemberNames?.ToArray() ?? Array.Empty<string>();
        if (memberNames.Length == 0)
        {
            yield return $"at '{basePath}': {result.ErrorMessage}";
            yield break;
        }

        foreach (string memberName in memberNames)
        {
            yield return $"at '{BuildMemberPath(basePath, memberName)}': {result.ErrorMessage}";
        }
    }

    private static IEnumerable<PropertyInfo> EnumerateCandidateProperties(Type settingsType)
    {
        foreach (PropertyInfo property in settingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (!property.CanRead)
            {
                continue;
            }

            yield return property;
        }
    }

    private static bool IsLeafType(Type type)
    {
        if (type == typeof(string))
        {
            return true;
        }

        if (type.IsPrimitive)
        {
            return true;
        }

        if (type.IsEnum)
        {
            return true;
        }

        if (type == typeof(decimal))
        {
            return true;
        }

        if (type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly))
        {
            return true;
        }

        if (typeof(Delegate).IsAssignableFrom(type))
        {
            return true;
        }

        return false;
    }
}
