using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YoungPackage.Ads
{
    public class AdSettings : ScriptableObject
    {
        public string iosKey;
        public string andKey;
        public bool isAdDebug;
        public bool isAlwaysTrueInEditor;

        public bool isUsingReward;
        public bool isUsingInter;
        public bool isUsingBanner;

        #if UNITY_EDITOR
        public void SaveAsset()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused)
                return;
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endif
    }
}