using System.IO;
using UnityEditor;
using UnityEngine;

namespace YoungPackage.Ads
{
    public class AdSettingsWindow : EditorWindow
    {
        private AdSettings _adSettings;
    
        private const string DevFilePath = "Assets/AdManager/Resources/AdSettings.asset";
        private const string ReleaseFilePath = "Packages/com.YoungPackage.AdManager/Resources/AdSettings.asset";

        [MenuItem("YoungPackage/Ads/AdSettings")]
        public static void ShowExample()
        {
            var window = GetWindow<AdSettingsWindow>("AdSettings", true);
            window.minSize = new Vector2(300, 400);
            window.ShowUtility();
            AssetDatabase.SaveAssets();
        }

        public void OnEnable()
        {
            var filePath = Directory.Exists("Assets/AdManager") ? DevFilePath : ReleaseFilePath;

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
            _adSettings.andKey = EditorGUILayout.TextField(_adSettings.andKey);
            EditorGUILayout.EndHorizontal();
        
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("iOS Key", GUILayout.MaxWidth(100f));
            _adSettings.iosKey = EditorGUILayout.TextField(_adSettings.iosKey);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Debug Mode", GUILayout.MaxWidth(100f));
            _adSettings.isAdDebug = EditorGUILayout.Toggle(_adSettings.isAdDebug);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("isEditorAlwaysTrue", GUILayout.MaxWidth(100f));
            _adSettings.isAlwaysTrueInEditor = EditorGUILayout.Toggle(_adSettings.isAlwaysTrueInEditor);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Using Reward", GUILayout.MaxWidth(100f));
            _adSettings.isUsingReward = EditorGUILayout.Toggle(_adSettings.isUsingReward);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Using Inter", GUILayout.MaxWidth(100f));
            _adSettings.isUsingInter = EditorGUILayout.Toggle(_adSettings.isUsingInter);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Using Banner", GUILayout.MaxWidth(100f));
            _adSettings.isUsingBanner = EditorGUILayout.Toggle(_adSettings.isUsingBanner);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Save"))
                _adSettings.SaveAsset();
        }
    }
}