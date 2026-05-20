using UnityEditor;
using System.IO;

/// <summary>
/// Unity Editor utility for building asset bundles consumed by the mod at runtime.
/// Place this script under UNITY_CWSDK/Assets/... (see PUT_MY_CONTENT_IN_EDITOR_FOLDER).
/// Output bundles must be named to match CustomContent constants in the mod project:
/// <c>example_mod</c> (prefabs, icons) and <c>example_scene</c> (custom map scene).
/// Copy built bundles next to the compiled DLL before publishing via update.ps1 / Steam Workshop.
/// </summary>
public class CreateAssetBundles
{
    /// <summary>
    /// Menu entry: Assets → Build AssetBundles. Writes to Assets/AssetBundles for StandaloneWindows64.
    /// </summary>
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        // Content Warning runs on Windows x64; target must match the game's standalone build.
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64);

        UnityEngine.Debug.Log("AssetBundles built successfully!");
    }
}
