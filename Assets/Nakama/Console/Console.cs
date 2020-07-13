using UnityEditor.UIElements;
using UnityEditor;

using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Collections.Generic;

[assembly: UxmlNamespacePrefix("Nakama.Console", "console")]

namespace Nakama.Console
{
    internal class Console : EditorWindow
    {
        [SerializeField] private List<WalletLedgerItem> walletLedgerItems = new List<WalletLedgerItem>();

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
            InitWallet(consoleTree);
            rootVisualElement.Add(consoleTree);
        }

        private void InitWallet(TemplateContainer consoleTree)
        {
            WalletElement wallet = consoleTree.Q("wallet") as WalletElement;
            var asSerializedObject = new SerializedObject(this);
            SerializedProperty serializedLedgerItems = asSerializedObject.FindProperty(nameof(walletLedgerItems));
            wallet.Init(asSerializedObject, serializedLedgerItems);
        }
    }
}