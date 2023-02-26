using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HengDao
{
    public class AssetBundleDescription
    {

    }
    public class AssetBundlePresets
    {
        public const string kPandaVariantName = "panda";
        public const string kLauchDescFileName = "assetbundle_lauch.json";
    }

    public class AssetBundleContent
    {
        public string assetbundleName = string.Empty;
        public List<string> assets = new List<string>();
    }
}

