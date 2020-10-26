using System.IO;
using UnityEditor;
using UnityEngine;

namespace YoungPackage.Ads
{
    public class AdSettingsWindow : EditorWindow
    {
        private AdSettings _adSettings;
    
        private const string devFilePath = "Assets/AdManager/Resources/YoungPackage/Ads/AdSettings.asset";
        private const string releaseFilePath = "Packages/com.YoungPackage.AdManager/Resources/YoungPackage/Ads/AdSettings.asset";

        [MenuItem("YoungPackage/Ads/AdSettings")]
        public static void ShowExample()
        {
            var window = GetWindow<AdSettingsWindow>("AdSettings", true);
            window.minSize = new Vector2(300, 400);
            window.ShowUtility();
        }

        public void OnEnable()
        {
            var filePath = Directory.Exists("Assets/AdManager/Resources/YoungPackage/Ads") ? devFilePath : releaseFilePath;
        
            if(!File.Exists(filePath))
                AssetDatabase.CreateAsset(CreateInstance<AdSettings>(), filePath);

            _adSettings = (AdSettings) AssetDatabase.LoadAssetAtPath(filePath, typeof(AdSettings));
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("IronSource Setting", new GUIStyle {fontSize = 28, fontStyle = FontStyle.Bold});
            EditorGUILayout.Space(20);
        
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Android Key", GUILayout.MaxWidth(100f));
            _adSettings.AndKey = EditorGUILayout.TextField(_adSettings.AndKey);
            EditorGUILayout.EndHorizontal();
        
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("iOS Key", GUILayout.MaxWidth(100f));
            _adSettings.IosKey = EditorGUILayout.TextField(_adSettings.IosKey);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Debug Mode", GUILayout.MaxWidth(100f));
            _adSettings.IsAdDebug = EditorGUILayout.Toggle(_adSettings.IsAdDebug);
            EditorGUILayout.EndHorizontal();
        }
    }
}