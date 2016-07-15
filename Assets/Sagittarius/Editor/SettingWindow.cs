using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Griphone.Sagittarius
{
    public class SettingWindow : EditorWindow
    {
        private Settings setting { get { return Settings.Instance; } }

        [MenuItem("Window/Sagittarius/Settings Open")]
        public static void Open()
        {
            GetWindow<SettingWindow>("Sagittarius Settings");
        }

        public void OnGUI()
        {
            Initialize();

            setting.IdRuleRegex = EditorGUILayout.TextField("IDのルール(正規表現)", setting.IdRuleRegex);

            // TODO 思ったけど、ScriptableObject側のInspector拡張でもいいから後回し
        }


        // 初期化を実行します.
        public void Initialize()
        {
        }
    }
}