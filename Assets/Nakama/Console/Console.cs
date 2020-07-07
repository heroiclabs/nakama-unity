using UnityEditor.UIElements;
using UnityEditor;

using UnityEngine.UIElements;
using UnityEngine;

[assembly: UxmlNamespacePrefix("NakamaConsole", "console")]

namespace NakamaConsole
{
    public class Console : EditorWindow
    {
        [MenuItem("Nakama/Console")]
        public static void OpenConsole()
        {
            var window = EditorWindow.GetWindow<Console>();            
            window.titleContent = new GUIContent("Nakama");
        }
        
        private void OnEnable()
        {
            string consolePath = "Assets/Nakama/Console/ConsoleElement/ConsoleElement.uxml";
            var console = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(consolePath);

            TemplateContainer consoleTree = console.CloneTree();

            rootVisualElement.Add(consoleTree);
//            var field = consoleTree.Q<EnumField>("config-scheme");
  //          Debug.Log("found field " + field);
        }
    }
}