using UnityEditor;
using UnityEngine;

public class QuickOrganiserWindow : EditorWindow
{
    private string logOutput = "";
    private Vector2 scrollPos;
    private int selectedTagIndex = 0;
    private string[] allTags;

    [MenuItem("Tools/Quick Organiser")]
    public static void ShowWindow()
    {
        GetWindow<QuickOrganiserWindow>("Quick Organiser");
    }

    private void OnEnable()
    {
        // Load all project tags for the dropdown
        allTags = UnityEditorInternal.InternalEditorUtility.tags;
    }

    private void OnGUI()
    {
        GUILayout.Label("Quick Organiser Plugin", EditorStyles.boldLabel);
        GUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "This tool helps organise your project assets and automatically tag scene objects based on defined rules.\n\n" +
            "Each rule supports a Priority value – higher numbers override lower ones. The log shows which rule applied to each object.",
            MessageType.Info
        );

        GUILayout.Space(10);
        DrawSeparator();

        // --- Asset Management Section ---
        GUILayout.Label("Asset Management", EditorStyles.boldLabel);
        if (GUILayout.Button("Organise Project Assets"))
        {
            AssetOrganiser.OrganiseProjectAssets();
            logOutput = "Project assets have been organised. Check Console for full report.";
        }

        GUILayout.Space(10);
        DrawSeparator();

        // --- Scene Tagging Section ---
        GUILayout.Label("Scene Tagging", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Tags scene objects based on Component and Name rules.\nRules with higher priority values take precedence.",
            MessageType.None
        );

        if (GUILayout.Button("Tag Scene Objects (Priority Mode)"))
        {
            logOutput = SceneTagger.TagSceneObjects();
        }

        GUILayout.Space(10);
        DrawSeparator();

        // --- Highlight Tagged Objects Section ---
        GUILayout.Label("Highlight Tagged Objects", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select a tag from the dropdown and click 'Highlight Objects' to select all objects with that tag in the scene.\n" +
            "The log will display which rule applied to each object.",
            MessageType.None
        );

        if (allTags.Length > 0)
        {
            selectedTagIndex = EditorGUILayout.Popup("Tag to Highlight", selectedTagIndex, allTags);

            if (GUILayout.Button("Highlight Objects"))
            {
                string tagToSelect = allTags[selectedTagIndex];
                GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tagToSelect);

                if (taggedObjects.Length > 0)
                {
                    Selection.objects = taggedObjects;

                    logOutput = $"Highlighted {taggedObjects.Length} objects tagged '{tagToSelect}':\n";

                    // Show which rule applied to each object (if available)
                    foreach (GameObject obj in taggedObjects)
                    {
                        // Attempt to get applied rule info from the object (requires SceneTagger to record it in a component or dictionary)
                        // Here we assume SceneTagger optionally attaches a TagInfo component (can be added to your SceneTagger)
                        TagInfo info = obj.GetComponent<TagInfo>();
                        if (info != null)
                        {
                            logOutput += $"- {obj.name}: {info.ruleSource}\n";
                        }
                        else
                        {
                            logOutput += $"- {obj.name}: (rule unknown)\n";
                        }
                    }

                    Debug.Log(logOutput);
                }
                else
                {
                    Selection.objects = new Object[0];
                    logOutput = $"No objects found with tag '{tagToSelect}'.";
                    Debug.Log(logOutput);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No tags found in the project. Add tags via Project Settings > Tags.", MessageType.Warning);
        }

        GUILayout.Space(10);
        DrawSeparator();

        // --- Log Output ---
        GUILayout.Label("Log Output", EditorStyles.boldLabel);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        EditorGUILayout.TextArea(logOutput, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        GUILayout.Space(5);
        if (GUILayout.Button("Clear Log"))
        {
            logOutput = "";
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Quick Organiser v1.0", EditorStyles.centeredGreyMiniLabel);
    }

    private void DrawSeparator()
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(5);
    }
}