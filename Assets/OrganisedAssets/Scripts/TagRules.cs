using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TagRulesConfig", menuName = "Quick Organiser/Tag Rules Config", order = 1)]
public class TagRules : ScriptableObject
{
    [Header("Component-based Tagging")]
    [Tooltip("Tag to assign to GameObjects with the specified component")]
    public List<ComponentTagRule> componentRules = new List<ComponentTagRule>();

    [Header("Name-based Tagging")]
    [Tooltip("Tag to assign to GameObjects whose name contains this string")]
    public List<NameTagRule> nameRules = new List<NameTagRule>();
}

[Serializable]
public class ComponentTagRule
{
    [Tooltip("Component type to check for")]
    public string componentType; // Use the component's class name, e.g., "Light"

    [Tooltip("Tag to assign")]
    public string tagName;

    [Tooltip("Priority")]
    public int priority = 0; // Higher value = higher priority
}

[Serializable]
public class NameTagRule
{
    [Tooltip("Substring to match in GameObject name")]
    public string nameContains;

    [Tooltip("Tag to assign")]
    public string tagName;

    [Tooltip("Priority")]
    public int priority = 0; // Higher value = higher priority
}
