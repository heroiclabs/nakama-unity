using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nakama.Console
{
    internal class WalletElement : IMGUIContainer
    {
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
                displayHeader: true, displayAddButton: true, displayRemoveButton: true);

            ledger.onAddCallback += HandleOnAdd;
            ledger.onRemoveCallback += HandleOnRemove;
            ledger.drawElementCallback += HandleDrawElement;
            ledger.elementHeightCallback += HandleElementHeight;
            ledger.drawHeaderCallback += HandleDrawHeader;
            ledger.drawElementBackgroundCallback += HandleElementBackground;
            ledger.onChangedCallback += HandleOnChanged;
        }

        private void HandleOnChanged(ReorderableList list)
        {
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void HandleElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            // do nothing
        }

        private void HandleDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Ledger");
        }

        private float HandleElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 15;
        }

        private void HandleDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            const float ROW_SPACING = 5;
            SerializedProperty ledgerItem = ledger.serializedProperty.GetArrayElementAtIndex(index);
            
            var idRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            SerializedProperty idProperty = ledgerItem.FindPropertyRelative("id");
            DrawLedgerItemRow("Id", idProperty, idRect);

            var amountRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + ROW_SPACING, rect.width, EditorGUIUtility.singleLineHeight);
            SerializedProperty amountProperty = ledgerItem.FindPropertyRelative("amount");
            DrawLedgerItemRow("Amount", amountProperty, amountRect);
        }

        private void HandleOnAdd(ReorderableList ledger)
        {
            ledger.serializedProperty.InsertArrayElementAtIndex(ledger.serializedProperty.arraySize);
            ledger.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void HandleOnRemove(ReorderableList ledger)
        {
            ledger.serializedProperty.DeleteArrayElementAtIndex(ledger.serializedProperty.arraySize - 1);
            ledger.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void handleOnGui()
        {
            ledger.serializedProperty.serializedObject.ApplyModifiedProperties();
            ledger.DoLayoutList();
        }

        private void DrawLedgerItemRow(string label, SerializedProperty property, Rect rowRect)
        {
            const int LABEL_WIDTH = 50;
            const int LABEL_FIELD_SPACING = 10;

            var labelRect = new Rect(rowRect.x, rowRect.y, LABEL_WIDTH, rowRect.height);
            EditorGUI.LabelField(labelRect, label);

            var propertyRect = new Rect(rowRect.x + LABEL_FIELD_SPACING + LABEL_WIDTH, rowRect.y, 
            rowRect.width - LABEL_WIDTH - LABEL_FIELD_SPACING, rowRect.height);
            EditorGUI.PropertyField(propertyRect, property, GUIContent.none);
        }
    }

    internal class WalletElementFactory : UxmlFactory<WalletElement> { }
}