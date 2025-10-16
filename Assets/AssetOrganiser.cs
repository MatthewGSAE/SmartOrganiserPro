using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AssetOrganiser
{
    // File extension -> main type folder
    private static readonly Dictionary<string, string> folderMap = new Dictionary<string, string>()
    {
        { ".cs", "Scripts" },
        { ".shader", "Shaders" },
        { ".mat", "Materials" },
        { ".mtl", "Materials" },
        { ".png", "Textures" },
        { ".jpg", "Textures" },
        { ".jpeg", "Textures" },
        { ".tga", "Textures" },
        { ".svg", "Vectors" },
        { ".xml", "SpriteSheets" },
        { ".fbx", "Models" },
        { ".obj", "Models" },
        { ".glb", "Models" },
        { ".prefab", "Prefabs" },
        { ".anim", "Animations" },
        { ".controller", "Animations" },
        { ".wav", "Audio" },
        { ".mp3", "Audio" },
        { ".ogg", "Audio" }
    };

    private const string rootPath = "Assets/";
    private const string organisedRoot = "OrganisedAssets";
    private const string duplicatesFolderName = "Duplicates";

    [MenuItem("Tools/Quick Organiser/Organise Project Assets")]
    public static string OrganiseProjectAssets()
    {
        int createdCount = 0;
        int movedCount = 0;
        int skippedCount = 0;
        int duplicatesCount = 0;
        List<string> skippedFiles = new List<string>();

        // Ensure main organised folder exists
        string mainFolder = Path.Combine(rootPath, organisedRoot).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(mainFolder))
        {
            AssetDatabase.CreateFolder("Assets", organisedRoot);
            createdCount++;
        }

        // Ensure duplicates folder exists
        string duplicatesFolder = Path.Combine(mainFolder, duplicatesFolderName).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(duplicatesFolder))
        {
            AssetDatabase.CreateFolder(mainFolder, duplicatesFolderName);
            createdCount++;
        }

        // Recursively find all files in Assets folder
        string[] allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);

        foreach (string file in allFiles)
        {
            if (file.EndsWith(".meta")) continue; // Skip meta files

            string ext = Path.GetExtension(file).ToLower();

            if (!folderMap.ContainsKey(ext))
            {
                skippedFiles.Add(file + " (unsupported file type)");
                skippedCount++;
                continue;
            }

            // Determine main type folder
            string typeFolder = folderMap[ext];
            string typeFolderPath = Path.Combine(mainFolder, typeFolder).Replace("\\", "/");

            // Determine subfolder for extension (e.g., FBX, OBJ, PNG)
            string extSubfolder = ext.TrimStart('.').ToUpper();
            string targetFolder = Path.Combine(typeFolderPath, extSubfolder).Replace("\\", "/");

            // Ensure main type folder exists
            if (!AssetDatabase.IsValidFolder(typeFolderPath))
            {
                AssetDatabase.CreateFolder(mainFolder, typeFolder);
                createdCount++;
            }

            // Ensure subfolder for extension exists
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                AssetDatabase.CreateFolder(typeFolderPath, extSubfolder);
                createdCount++;
            }

            string fileName = Path.GetFileName(file);

            // Convert system path to Unity asset path
            string assetPath = file.Replace("\\", "/");
            if (assetPath.IndexOf(rootPath) >= 0)
                assetPath = assetPath.Substring(assetPath.IndexOf(rootPath));

            string newPath = Path.Combine(targetFolder, fileName).Replace("\\", "/");

            // Skip if already inside target folder
            if (assetPath.StartsWith(targetFolder))
            {
                skippedFiles.Add(assetPath + " (already in target folder)");
                skippedCount++;
                continue;
            }

            // Only check for duplicates if not already in target folder
            if (AssetDatabase.LoadAssetAtPath<Object>(newPath) != null)
            {
                string duplicatePath = Path.Combine(duplicatesFolder, fileName).Replace("\\", "/");
                string errorDup = AssetDatabase.MoveAsset(assetPath, duplicatePath);
                if (string.IsNullOrEmpty(errorDup))
                {
                    duplicatesCount++;
                    skippedFiles.Add(assetPath + " (duplicate moved to Duplicates folder)");
                }
                else
                {
                    skippedFiles.Add(assetPath + $" (duplicate move failed: {errorDup})");
                    skippedCount++;
                }
                continue;
            }

            // Move asset
            string error = AssetDatabase.MoveAsset(assetPath, newPath);
            if (string.IsNullOrEmpty(error))
            {
                movedCount++;
            }
            else
            {
                skippedFiles.Add(assetPath + $" (failed to move: {error})");
                skippedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Build report string
        string report = $"[Organiser Report]\nFolders created: {createdCount}\nAssets moved: {movedCount}\nDuplicates moved: {duplicatesCount}\nSkipped: {skippedCount}\n";

        if (skippedFiles.Count > 0)
        {
            report += "\nSkipped Files Details:\n";
            foreach (string s in skippedFiles)
            {
                report += "- " + s + "\n";
            }
        }

        Debug.Log(report);
        return report;
    }
}