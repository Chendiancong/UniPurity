using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UniPurity.Editor
{
    public class UniPuritySettingWindow : EditorWindow
    {
        private SerializedObject mSerializedObject;
        private SerializedProperty mStaticNeededAOTAssProp;
        private SerializedProperty mNeededAOTAssProp;
        private SerializedProperty mDefaultDllPathProp;
        private SerializedProperty mCustomDllPathProp;
        private bool mIsFocus = false;
        private GUIContent mGUIContent_SaveSetting = new GUIContent("保存设置");
        private GUIContent mGUIContent_BuildSetting = new GUIContent("构建设置");
        private GUIContent mGUIContent_TargetSelect = new GUIContent("目标平台");
        private GUIContent mGUIContent_BuildOptions = new GUIContent("构建选项");
        private GUIContent mGUIContent_OutputOptions = new GUIContent("输出处理");
        private GUIContent mGUIContent_Build = new GUIContent("开始");
        private GUIStyle mStyle_box;

        private GUIContent[] mTargetOptions = new GUIContent[]
        {
            new GUIContent("ActiveTarget"),
            new GUIContent("Win32"),
            new GUIContent("Win64"),
            new GUIContent("Android"),
            new GUIContent("IOS")
        };
        private Dictionary<string, BuildTarget> mOption2Targets = new Dictionary<string, BuildTarget>(
            new KeyValuePair<string, BuildTarget>[]
            {
                new KeyValuePair<string, BuildTarget>("Win32", BuildTarget.StandaloneWindows),
                new KeyValuePair<string, BuildTarget>("Win64", BuildTarget.StandaloneWindows64),
                new KeyValuePair<string, BuildTarget>("Android", BuildTarget.Android),
                new KeyValuePair<string, BuildTarget>("IOS", BuildTarget.iOS)
            }
        );
        private int mCurTargetOptionId = 0;

        private GUIContent[] mBuildOptions = new GUIContent[]
        {
            new GUIContent("构建全部"),
            new GUIContent("仅构建aot部分"),
            new GUIContent("仅构建热更部分")
        };
        private int mCurBuildOptionId = 0;

        private GUIContent[] mOutputOptions = new GUIContent[]
        {
            new GUIContent("拷贝dll到指定目录"),
            new GUIContent("不拷贝dll")
        };
        private int mCurOutputOptionId = 0;

        [MenuItem("UniPurity/SettingWindow")]
        public static void OpenWindow()
        {
            var window = (UniPuritySettingWindow)GetWindow(typeof(UniPuritySettingWindow));
            window.titleContent = new GUIContent("UniPurity Setting");
            window.minSize = new Vector2(400, 300);
        }

        private Vector2 mScrollPos = Vector2.zero;
        private void OnGUI()
        {
            if (mSerializedObject is null)
                return;
            InitStyles();
            mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);
            PropertyField(mStaticNeededAOTAssProp, false);
            PropertyField(mNeededAOTAssProp);
            PropertyField(mDefaultDllPathProp, false);
            PropertyField(mCustomDllPathProp);
            Rect rect;

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(mGUIContent_SaveSetting))
            {
                SaveObject();
                Debug.Log($"保存成功");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(mStyle_box);
            rect = EditorGUILayout.GetControlRect();
            GUI.Label(rect, mGUIContent_BuildSetting);
            rect = EditorGUILayout.GetControlRect();
            rect.width = 100;
            GUI.Label(rect, mGUIContent_TargetSelect);
            rect.x += 120;
            rect.width = 200;
            mCurTargetOptionId = EditorGUI.Popup(rect, mCurTargetOptionId, mTargetOptions);
            rect = EditorGUILayout.GetControlRect();
            rect.width = 100;
            GUI.Label(rect, mGUIContent_BuildOptions);
            rect.x += 120;
            rect.width = 200;
            mCurBuildOptionId = EditorGUI.Popup(rect, mCurBuildOptionId, mBuildOptions);
            rect = EditorGUILayout.GetControlRect();
            rect.width = 100;
            GUI.Label(rect, mGUIContent_OutputOptions);
            rect.x += 120;
            rect.width = 200;
            mCurOutputOptionId = EditorGUI.Popup(rect, mCurOutputOptionId, mOutputOptions);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(mGUIContent_Build))
            {
                string targetStr = mTargetOptions[mCurTargetOptionId].text;
                string buildOptionStr = mBuildOptions[mCurBuildOptionId].text;
                string outputOptionStr = mOutputOptions[mCurOutputOptionId].text;
                Debug.Log($"构建 {targetStr}, {buildOptionStr}, {outputOptionStr}");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void OnEnable()
        {
            var setting = UniPurityEditorSettings.Instance;
            mSerializedObject?.Dispose();
            mSerializedObject = new SerializedObject(setting);
            mStaticNeededAOTAssProp = mSerializedObject.FindProperty("staticNeededAOTAssemblies");
            mNeededAOTAssProp = mSerializedObject.FindProperty("neededAOTAssemblies");
            mDefaultDllPathProp = mSerializedObject.FindProperty("defaultDllPath");
            mCustomDllPathProp = mSerializedObject.FindProperty("customDllPath");
        }

        private void OnFocus()
        {
            if (mIsFocus)
                return;
            mIsFocus = true;
            Repaint();
        }

        private void OnLostFocus()
        {
            mIsFocus = false;
        }

        private void OnDestroy()
        {
            SaveObject();
        }

        private void PropertyField(SerializedProperty prop, bool enable = true)
        {
            if (!enable)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(prop);
            if (!enable)
                GUI.enabled = true;
        }

        private void SaveObject()
        {
            if (!(mSerializedObject is null))
            {
                mSerializedObject.ApplyModifiedProperties();
                UniPurityEditorSettings.Instance.Save();
            }
        }

        private void InitStyles()
        {
            if (mStyle_box is null)
            {
                mStyle_box = new GUIStyle("box");
            }
        }
    }
}