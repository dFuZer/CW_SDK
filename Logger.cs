using UnityEngine;

namespace ExampleMod;

/// <summary>
/// Thin wrapper around Unity's <see cref="Debug"/> logging with a consistent mod prefix.
/// Centralizing logs here makes it easy to filter the console and swap in file logging later.
/// </summary>
public static class Logger
{
    private const string Prefix = "[ExampleMod]";

    /// <summary>Standard info log routed to the Unity console.</summary>
    public static void Log(string message) => Debug.Log($"{Prefix} {message}");

    /// <summary>Non-fatal warning — content may load partially or behaviour may degrade.</summary>
    public static void LogWarning(string message) => Debug.LogWarning($"{Prefix} {message}");

    /// <summary>Fatal or unexpected error — registration failed or patch threw.</summary>
    public static void LogError(string message) => Debug.LogError($"{Prefix} {message}");
}
