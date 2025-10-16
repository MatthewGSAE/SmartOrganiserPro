using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

public static class SceneTagger
{
    public static string TagSceneObjects()
    {
        // Find TagRules asset
        string[] guids = AssetDatabase.FindAssets("t:TagRules");
        if (guids.Length == 0)
        {
            Debug.LogError("No TagRules asset found. Create one via Assets > Create > Quick Organizer > Tag Rules Config.");
            return "[ERROR] TagRules asset not found!";
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        TagRules tagRules = AssetDatabase.LoadAssetAtPath<TagRules>(path);
        if (tagRules == null)
        {
            Debug.LogError("Failed to load TagRules asset at path: " + path);
            return "[ERROR] Could not load TagRules asset.";
        }

        int taggedCount = 0;
        int skippedCount = 0;
        Dictionary<string, int> ruleUsage = new Dictionary<string, int>();

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.hideFlags != HideFlags.None) continue;

            string bestTag = null;
            int highestPriority = int.MinValue;
            string bestRuleSource = null;

            // --- Component-based rules ---
            foreach (var rule in tagRules.componentRules)
            {
                if (string.IsNullOrEmpty(rule.componentType)) continue;

                Component[] comps = obj.GetComponents<Component>();
                foreach (Component c in comps)
                {
                    if (c == null) continue;

                    if (c.GetType().Name == rule.componentType && IsValidTag(rule.tagName))
                    {
                        if (rule.priority > highestPriority)
                        {
                            highestPriority = rule.priority;
                            bestTag = rule.tagName;
                            bestRuleSource = "Component:" + rule.componentType;
                        }
                    }
                }
            }

            // --- Name-based rules ---
            foreach (var rule in tagRules.nameRules)
            {
                if (string.IsNullOrEmpty(rule.nameContains)) continue;

                if (obj.name.Contains(rule.nameContains) && IsValidTag(rule.tagName))
                {
                    if (rule.priority > highestPriority)
                    {
                        highestPriority = rule.priority;
                        bestTag = rule.tagName;
                        bestRuleSource = "Name:" + rule.nameContains;
                    }
                }
            }

            // --- Apply best match ---
            if (!string.IsNullOrEmpty(bestTag))
            {
                Undo.RecordObject(obj, "Scene Tagging");
                obj.tag = bestTag;

                // Attach or update TagInfo component
                TagInfo info = obj.GetComponent<TagInfo>();
                if (info == null) info = obj.AddComponent<TagInfo>();
                info.ruleSource = bestRuleSource;

                taggedCount++;

                if (!ruleUsage.ContainsKey(bestRuleSource))
                    ruleUsage[bestRuleSource] = 0;
                ruleUsage[bestRuleSource]++;
            }
            else
            {
                skippedCount++;
            }
        }

        // --- Build summary ---
        string summary = "[Scene Tagger Report]\n";
        summary += "Tagged objects: " + taggedCount + "\n";
        summary += "Skipped objects: " + skippedCount + "\n\n";

        foreach (var entry in ruleUsage)
        {
            summary += "Rule Applied: " + entry.Key + " (" + entry.Value + " objects)\n";
        }

        Debug.Log(summary);
        return summary;
    }

    private static bool IsValidTag(string tag)
    {
        foreach (string t in InternalEditorUtility.tags)
        {
            if (t == tag) return true;
        }
        return false;
    }
}