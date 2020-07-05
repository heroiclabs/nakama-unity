using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace NakamaConsole
{
    internal class Console : EditorWindow
    {
        private const string _ASSET_BASE_PATH = "Assets/Nakama/Editor/Console/";

        [MenuItem("Nakama/Console")]
        public static void ShowConsole()
        {
            var window = GetWindow<Console>();
            window.titleContent = new GUIContent("Nakama Console");
            window.minSize = new Vector2(400, 300);            
        }

        private void OnEnable()
        {
            var consoleElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_ASSET_BASE_PATH + "ConsoleElement.uxml");
            consoleElement.CloneTree(rootVisualElement);
        }
    }
}