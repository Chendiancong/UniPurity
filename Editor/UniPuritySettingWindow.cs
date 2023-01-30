using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

namespace UniPurity.Editor
{
    public class UniPuritySettingWindow : EditorWindow
    {
        private SerializedObject mSerializedObject;
        private SerializedProperty mStaticNeededAOTAssProp;
        private SerializedProperty mNeededAOTAssProp;
        private SerializedProperty mDefaultDllPathProp;
        private SerializedProperty mCustomDllPathProp;
        private GUIContent mGUIContent_SaveSetting = new GUIContent("保存设置");
        private GUIContent mGUIContent_BuildSetting = new GUIContent("构建设置");
        private GUIContent mGUIContent_TargetSelect = new GUIContent("目标平台");
        private GUIContent mGUIContent_BuildOptions = new GUIContent("构建选项");
        private GUIContent mGUIContent_OutputOptions = new GUIContent("输出处理");
        private GUIContent mGUIContent_Build = new GUIContent("开始");
        private GUIStyle mStyle_box;
        private bool mIsFocus = false;
        private bool mIsBuilding = false;

        private GUIContent[] mTargetOptionContents = new GUIContent[]
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

        private GUIContent[] mBuildOptionContents = new GUIContent[]
        {
            new GUIContent("构建全部"),
            new GUIContent("仅构建aot部分"),
            new GUIContent("仅构建热更部分")
        };
        private ThisBuildOption[] mBuildOptions = new ThisBuildOption[]
        {
            ThisBuildOption.All,
            ThisBuildOption.AOT,
            ThisBuildOption.HotUpdate
        };
        private int mCurBuildOptionId = 0;

        private GUIContent[] mOutputOptionContents = new GUIContent[]
        {
            new GUIContent("拷贝dll到指定目录"),
            new GUIContent("不拷贝dll")
        };
        private ThisOutputOption[] mOutputOptions = new ThisOutputOption[]
        {
            ThisOutputOption.CopyDll,
            ThisOutputOption.DoNothing
        };
        private int mCurOutputOptionId = 0;

        [MenuItem("UniPurity/SettingWindow")]
        public static void OpenWindow()
        {
            var window = (UniPuritySettingWindow)GetWindow(typeof(UniPuritySettingWindow));
            window.titleContent = new GUIContent("UniPurity Setting");
            window.minSize = new Vector2(400, 300);
        }

        private void PropertyField(SerializedProperty prop, bool enable = true)
        {
            if (!prop.serializedObject.targetObject)
                return;
            if (!enable || mIsBuilding)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(prop);
            if (!enable || mIsBuilding)
                GUI.enabled = true;
        }

        private bool LayoutButton(GUIContent content, bool enable = true)
        {
            if (!enable || mIsBuilding)
                GUI.enabled = false;
            bool clicked = GUILayout.Button(content);
            if (!enable || mIsBuilding)
                GUI.enabled = true;
            return clicked;
        }

        private void SaveObject()
        {
            if (mSerializedObject is not null && mSerializedObject.targetObject)
            {
                mSerializedObject.ApplyModifiedProperties();
                UniPurityEditorSettings.Instance.Save();
            }
        }

        private void RefreshProp(ref SerializedProperty prop, string propName, SerializedObject obj)
        {
            prop?.Dispose();
            prop = obj.FindProperty(propName);
        }

        private void InitProps()
        {
            if (mSerializedObject is not null && mSerializedObject.targetObject)
                return;
            mSerializedObject?.Dispose();
            mSerializedObject = new SerializedObject(UniPurityEditorSettings.Instance);
            RefreshProp(ref mStaticNeededAOTAssProp, "staticNeededAOTAssemblies", mSerializedObject);
            RefreshProp(ref mNeededAOTAssProp, "neededAOTAssemblies", mSerializedObject);
            RefreshProp(ref mDefaultDllPathProp, "defaultDllPath", mSerializedObject);
            RefreshProp(ref mCustomDllPathProp, "customDllPath", mSerializedObject);
        }

        private void InitStyles()
        {
            if (mStyle_box is null)
            {
                mStyle_box = new GUIStyle("box");
            }
        }

        private IEnumerator ProcessBuild()
        {
            mIsBuilding = true;
            string targetStr = mTargetOptionContents[mCurTargetOptionId].text;
            string buildOptionStr = mBuildOptionContents[mCurBuildOptionId].text;
            string outputOptionStr = mOutputOptionContents[mCurOutputOptionId].text;
            Debug.Log($"构建 {targetStr}, {buildOptionStr}, {outputOptionStr}");
            yield return new EditorWaitForSeconds(0.3f);

            BuildTarget target;
            if (!mOption2Targets.TryGetValue(targetStr, out target))
                target = EditorUserBuildSettings.activeBuildTarget;
            ThisBuildOption buildOption = mBuildOptions[mCurBuildOptionId];
            ThisOutputOption outputOption = mOutputOptions[mCurOutputOptionId];

            switch (buildOption)
            {
                case ThisBuildOption.AOT:
                    UniPurityCommands.BuildAOTWithTarget(target);
                    break;
                case ThisBuildOption.HotUpdate:
                    UniPurityCommands.BuildHotUpdateWithTarget(target);
                    break;
                case ThisBuildOption.All:
                    UniPurityCommands.BuildAllWithTarget(target);
                    break;
            }

            if (outputOption == ThisOutputOption.CopyDll)
            {
                UniPurityCommands.CopyAOTDllWithTarget(target);
                UniPurityCommands.CopyHotUpdateDllWithTarget(target);
            }

            mIsBuilding = false;
        }

        private Vector2 mScrollPos = Vector2.zero;
        private void OnGUI()
        {
            InitProps();
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
            if (LayoutButton(mGUIContent_SaveSetting))
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
            mCurTargetOptionId = EditorGUI.Popup(rect, mCurTargetOptionId, mTargetOptionContents);
            rect = EditorGUILayout.GetControlRect();
            rect.width = 100;
            GUI.Label(rect, mGUIContent_BuildOptions);
            rect.x += 120;
            rect.width = 200;
            mCurBuildOptionId = EditorGUI.Popup(rect, mCurBuildOptionId, mBuildOptionContents);
            rect = EditorGUILayout.GetControlRect();
            rect.width = 100;
            GUI.Label(rect, mGUIContent_OutputOptions);
            rect.x += 120;
            rect.width = 200;
            mCurOutputOptionId = EditorGUI.Popup(rect, mCurOutputOptionId, mOutputOptionContents);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (LayoutButton(mGUIContent_Build))
                EditorWindowCoroutineExtension.StartCoroutine(this, ProcessBuild());
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void OnEnable()
        {
            InitProps();
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

        private enum ThisBuildOption
        {
            AOT,
            HotUpdate,
            All
        }

        private enum ThisOutputOption
        {
            DoNothing,
            CopyDll
        }
    }
}