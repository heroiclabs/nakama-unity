using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace NakamaConsole
{
    public class Console : EditorWindow
    {
        public const string ASSET_BASE_PATH = "Assets/Nakama/Editor/Console/";

        [MenuItem("Nakama/Console")]
        public static void ShowConsole()
        {
            // Opens the window, otherwise focuses it if it’s already open.
            var window = GetWindow<Console>();

            // Adds a title to the window.
            window.titleContent = new GUIContent("Nakama Console");

            // Sets a minimum size to the window.
            window.minSize = new Vector2(250, 50);
        }

        private void OnEnable()
        {
            
            var consolePath = ASSET_BASE_PATH + "ConsoleElement.uxml";
            var consoleTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(consolePath);
            VisualElement console = consoleTemplate.CloneTree(string.Empty);

            rootVisualElement.Add(console);

            var loginUSSPath = ASSET_BASE_PATH + "LoginElement.uss";
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(loginUSSPath);
            rootVisualElement.styleSheets.Add(styleSheet);
        }
    }
}