using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace UniPurity.Editor
{
    public class UniPurityEditorSettingsProvider : SettingsProvider
    {
        private SerializedObject _serializedObject;
        private SerializedProperty _staticNeededAOTAss;
        private SerializedProperty _neededAOTAss;
        private SerializedProperty _defaultDllPathProp;
        private SerializedProperty _customDllPathProp;

        public UniPurityEditorSettingsProvider() : base("Project/UniPurity Settings", SettingsScope.Project)
        { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            EditorStatusWatcher.OnEditorFocused += OnEditorFocused;
            InitGUI();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            EditorStatusWatcher.OnEditorFocused -= OnEditorFocused;
            UniPurityEditorSettings.Instance.Save();
        }

        public override void OnGUI(string searchContext)
        {
            if (_serializedObject is null || !_serializedObject.targetObject)
                InitGUI();
            _serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_staticNeededAOTAss);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(_neededAOTAss);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_defaultDllPathProp);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(_customDllPathProp);
            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                UniPurityEditorSettings.Instance.Save();
            }
        }

        private void InitGUI()
        {
            var setting = UniPurityEditorSettings.Instance;
            _serializedObject?.Dispose();
            _serializedObject = new SerializedObject(setting);
            _staticNeededAOTAss = _serializedObject.FindProperty("staticNeededAOTAssemblies");
            _neededAOTAss = _serializedObject.FindProperty("neededAOTAssemblies");
            _defaultDllPathProp = _serializedObject.FindProperty("defaultDllPath");
            _customDllPathProp = _serializedObject.FindProperty("customDllPath");
        }

        private void OnEditorFocused()
        {
            InitGUI();
            Repaint();
        }

        static UniPurityEditorSettingsProvider provider;
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (provider is null)
            {
                provider = new UniPurityEditorSettingsProvider();
                using (var so = new SerializedObject(UniPurityEditorSettings.Instance))
                {
                    provider.keywords = GetSearchKeywordsFromSerializedObject(so);
                }
            }
            return provider;
        }
    }
}
