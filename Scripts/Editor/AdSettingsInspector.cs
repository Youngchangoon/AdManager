using UnityEditor;
using UnityEngine;

namespace YoungPackage.Ads
{
    [CustomEditor(typeof(AdSettings))]
    public class AdSettingsInspector : Editor
    {
        private AdSettings _adSettings;

        private void OnEnable()
        {
            _adSettings = target as AdSettings;
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}