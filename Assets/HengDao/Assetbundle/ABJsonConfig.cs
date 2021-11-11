using System;

namespace HengDao
{
	/// <summary>
    /// Assetbunlde structures
    /// </summary>
	public class ABJsonConfig
	{
	    public class AssetBundleInfo
	    {
	        public string name
	        {
	            get;
	            set;
	        }

	        public string[] elements
	        {
	            get;
	            set;
	        }
	    }

	    public class AssetBundleSet
	    {
	        public string name
	        {
	            get;
	            set;
	        }
	        public AssetBundleInfo[] bundles
	        {
	            get;
	            set;
	        }
	    }

	    public AssetBundleSet[] sets
	    {
	        get;
	        set;
	    }

	    public string[] scenes
	    {
	        get;
	        set;
	    }
	}
}