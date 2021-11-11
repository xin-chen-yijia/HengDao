using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HengDao
{
    class TagManager
    {
        public static void AddTag(string tag)
        {
            if (!isHasTag(tag))
            {
                //SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                //SerializedProperty it = tagManager.GetIterator();
                //while (it.NextVisible(true))
                //{
                //    if (it.name == "tags")
                //    {
                //        for (int i = 0; i < it.arraySize; i++)
                //        {
                //            SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                //            if (string.IsNullOrEmpty(dataPoint.stringValue))
                //            {
                //                dataPoint.stringValue = tag;
                //                tagManager.ApplyModifiedProperties();
                //                return;
                //            }
                //        }
                //    }
                //}

                UnityEditorInternal.InternalEditorUtility.AddTag(tag);
            }
        }

        public static void SetTags(string[] tags)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty it = tagManager.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.name == "tags")
                {
                    if(tags.Length > it.arraySize)
                    {
                        int dif = tags.Length - it.arraySize;
                        for(int i=0;i < dif; ++i)
                        {
                            it.InsertArrayElementAtIndex(it.arraySize);
                        }
                    }
                    for (int i = 0; i < it.arraySize; i++)
                    {
                        SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                        dataPoint.stringValue = tags[i];
                        tagManager.ApplyModifiedProperties();
                    }

                    break;
                }
            }
        }

        public static void SetLayer(int index, string layer)
        {
            if (!isHasLayer(layer))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty it = tagManager.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.name == "layers")
                    {
                        SerializedProperty p = it.GetArrayElementAtIndex(index);
                        p.stringValue = layer;
                        tagManager.ApplyModifiedProperties();
                    }
                }
            }
        }
        public static bool isHasTag(string tag)
        {
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
            {
                if (UnityEditorInternal.InternalEditorUtility.tags[i].Contains(tag))
                    return true;
            }
            return false;
        }
        public static bool isHasLayer(string layer)
        {
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++)
            {
                if (UnityEditorInternal.InternalEditorUtility.layers[i].Contains(layer))
                    return true;
            }
            return false;
        }
    }
}

public class TagLayerImporter: AssetPostprocessor
{


    //static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    //{
    //    foreach (string s in importedAssets) 
    //    {
    //        string aname = s.Substring(s.LastIndexOf('/') + 1);
    //        if (aname.Equals("FireTagLayerImporter.cs"))
    //        {
    //            AddTag("abc");
    //            foreach(var v in tags)
    //            {
    //                //AddTag(v);
    //            }

    //            break;
    //        }
    //    }
    //}

}