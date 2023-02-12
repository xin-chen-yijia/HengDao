using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="AssetBundlePluginRequirements", menuName ="HengDao/Plugins Requirement")]
public class AssetBundlePluginRequirement : ScriptableObject
{
    public List<string> plugins = new List<string>();
}
