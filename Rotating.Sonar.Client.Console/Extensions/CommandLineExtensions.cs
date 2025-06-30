using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotating.Sonar.ClientApp.Console.Extensions;

/// <summary>
/// Extension methods for command-line argument processing
/// </summary>
public static class CommandLineExtensions
{
    /// <summary>
    /// Gets the value of a command-line option
    /// </summary>
    /// <param name="args">Command-line arguments array</param>
    /// <param name="optionName">Option name without the '-' prefix</param>
    /// <returns>The option value or null if the option is not found</returns>
    public static string? GetCommandOption(this string[] args, string optionName)
    {
        string optionFlag = $"-{optionName}";
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == optionFlag && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        
        return null;
    }

    /// <summary>
    /// Validates command-line options for duplicates using LINQ
    /// </summary>
    /// <param name="args">Command-line arguments array</param>
    /// <exception cref="ArgumentException">Thrown when duplicate options are found</exception>
    public static void ValidateCommandOptions(this string[] args)
    {
        var duplicateOptions = args
            .Where(arg => arg.StartsWith("-"))
            .GroupBy(arg => arg)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateOptions.Any())
        {
            var duplicateList = string.Join(", ", duplicateOptions);
            throw new ArgumentException($"Duplicate options found: {duplicateList}");
        }
    }

    /// <summary>
    /// Checks if a command-line flag is present (flag without value)
    /// </summary>
    /// <param name="args">Command-line arguments array</param>
    /// <param name="flagName">Flag name without the '-' prefix</param>
    /// <returns>True if the flag is present, false otherwise</returns>
    public static bool HasCommandFlag(this string[] args, string flagName)
    {
        string flag = $"-{flagName}";
        return args.Contains(flag);
    }
}

/// <summary>
/// Represents the result of command-line option validation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
} 