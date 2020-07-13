using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nakama.Console
{
    internal class WalletElement : IMGUIContainer
    {
        internal delegate void LedgerAddHandler(WalletLedgerItem item);
        internal delegate void LedgerRemoveHandler(WalletLedgerItem item);

        internal event LedgerAddHandler OnLedgerAdd;
        internal event LedgerRemoveHandler OnLedgerRemove;

        private ReorderableList ledger;
        private bool initialized = false;

        private SerializedObject serializedObject;

        internal void Init(SerializedObject serializedObject, SerializedProperty serializedProperty)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            this.serializedObject = serializedObject;


            this.onGUIHandler += handleOnGui;

            ledger = new ReorderableList(serializedObject, serializedProperty, draggable: true,
                displayHeader: false, displayAddButton: true, displayRemoveButton: true);

            ledger.onAddCallback += HandleOnAdd;
            ledger.drawElementCallback += HandleDrawElement;
        }

        private void HandleDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty ledgerItem = ledger.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;

            var idRect = new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight);
            SerializedProperty idProperty = ledgerItem.FindPropertyRelative("id");

            Debug.Log(" id prop " + idProperty);
            EditorGUI.PropertyField(idRect, idProperty, GUIContent.none);

            var amountRect = new Rect(rect.x + 60, rect.y, rect.width - 60 - 30, EditorGUIUtility.singleLineHeight);
            SerializedProperty amountProperty = ledgerItem.FindPropertyRelative("amount");

            EditorGUI.PropertyField(amountRect, amountProperty, GUIContent.none);
        }

        internal void HandleOnAdd(ReorderableList ledger)
        {
            if (OnLedgerAdd != null)
            {
                OnLedgerAdd(new WalletLedgerItem { Id = "test", Amount = 120.2f });
            }


        }

        private void handleOnGui()
        {
            serializedObject.Update();
            ledger.DoLayoutList();
        }
    }

    internal class WalletElementFactory : UxmlFactory<WalletElement> { }
}