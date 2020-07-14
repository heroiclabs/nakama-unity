using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nakama.Console
{
    internal class StorageElement : VisualElement
    {
        public void Init()
        {
            var c1 = CreateCollection();
            Add(c1);

            var spacer = new VisualElement();
            spacer.style.height = 10;
            Add(spacer);

            var c2 = CreateCollection();
            Add(c2);
        }

        private VisualElement CreateCollection()
        {
            var box = new Box();
            var foldout = new Foldout();
            foldout.text = "Test Collection";
            foldout.Add(CreatePermissionDropdowns());
            foldout.Add(CreateCollectionItems());
            box.Add(foldout);
            return box;
        }

        private VisualElement CreatePermissionDropdowns()
        {
            var readPermissions = new EnumField();
            readPermissions.Init(StorageReadPermission.None);
            readPermissions.label = "Read";
            readPermissions.style.paddingRight = 10;

            var writePermissions = new EnumField();
            writePermissions.Init(StorageWritePermission.None);

            writePermissions.label = "Write";

            var container = new VisualElement();
            container.Add(readPermissions);
            container.Add(writePermissions);
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexShrink = 1;

            var search = new ToolbarSearchField();
            search.style.flexGrow = 1;
            search.style.minWidth = 0;
            search.style.width = 0;
            container.Add(search);

            container.style.paddingBottom = container.style.paddingTop = 10;
            container.style.paddingRight = container.style.paddingLeft = 20;
            return container;
        }

        private ListView CreateCollectionItems()
        {
            var listView = new ListView();
            const int itemCount = 200;
            var items = new List<string>(itemCount);
            for (int i = 1; i <= itemCount; i++)
            {
                items.Add("some_key");
            }
            
            Action<VisualElement, int> bindItem = (e, i) => (e as VisualElement).Q<Label>().text = items[i];
            listView.makeItem = CreateCollectionItem;
            listView.bindItem = bindItem;
            listView.itemsSource = items;
            listView.selectionType = SelectionType.None;
            return listView;
        }

        private VisualElement CreateCollectionItem()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = new StyleEnum<Align>(Align.Center);
            row.style.height = new StyleLength(20);
            var textField = new TextField();

            var label = new Label();
            row.Add(label);
            textField.style.flexGrow = new StyleFloat(1f);
            row.Add(textField);
            
            return row;
        }
    }

    internal class StorageElementFactory : UxmlFactory<StorageElement> { }
}