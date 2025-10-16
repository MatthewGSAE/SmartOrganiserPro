using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class AssetOrganiser
{
    // Folder map: file extension -> target folder
    private static readonly Dictionary<string, string> folderMap = new Dictionary<string, string>()
    {
        { ".cs", "Scripts" },
        { ".shader", "Shaders" },
        { ".mat", "Materials" },
        { ".png", "Textures" },
        { ".jpg", "Textures" },
        { ".jpeg", "Textures" },
        { ".tga", "Textures" },
        { ".fbx", "Models" },
        { ".obj", "Models" },
        { ".prefab", "Prefabs" },
        { ".anim", "Animations" },
        { ".controller", "Animations" },
        { ".wav", "Audio" },
        { ".mp3", "Audio" }
    };

    private const string rootPath = "Assets/";

    public static string OrganiseAssets()
    {
        int movedCount = 0;
        int skippedCount = 0;
        int createdCount = 0;

        // Ensure the main organization folder exists
        string mainFolder = Path.Combine(rootPath, "OrganisedAssets");
        if (!AssetDatabase.IsValidFolder(mainFolder))
        {
            AssetDatabase.CreateFolder("Assets", "OrganisedAssets");
            createdCount++;
        }

        // Get all assets (excluding folders)
        string[] allAssets = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);

        foreach (string assetPath in allAssets)
        {
            if (assetPath.EndsWith(".meta")) continue; // skip meta files

            string ext = Path.GetExtension(assetPath).ToLower();
            if (!folderMap.ContainsKey(ext))
            {
                skippedCount++;
                continue; // skip unsupported file types
            }

            string folderName = folderMap[ext];
            string targetFolder = Path.Combine(mainFolder, folderName);

            // Create target subfolder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                AssetDatabase.CreateFolder(mainFolder, folderName);
                createdCount++;
            }

            // Determine new path
            string fileName = Path.GetFileName(assetPath);
            string newPath = Path.Combine(targetFolder, fileName).Replace("\\", "/");

            // Skip if already in correct folder
            if (assetPath == newPath)
            {
                skippedCount++;
                continue;
            }

            // Move the asset
            string error = AssetDatabase.MoveAsset(assetPath, newPath);
            if (string.IsNullOrEmpty(error))
                movedCount++;
            else
                Debug.LogWarning($"Failed to move {fileName}: {error}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return $"[Organizer Report]\n" +
               $"Created folders: {createdCount}\n" +
               $"Moved assets: {movedCount}\n" +
               $"Skipped: {skippedCount}\n";
    }
}