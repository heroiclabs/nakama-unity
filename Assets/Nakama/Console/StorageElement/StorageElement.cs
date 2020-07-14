using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nakama.Console
{
    internal class StorageElement : VisualElement
    {
        public void Init()
        {
            // Create some list of data, here simply numbers in interval [1, 1000]
            const int itemCount = 1000;
            var items = new List<string>(itemCount);
            for (int i = 1; i <= itemCount; i++)
            {
                items.Add("some_key");
            }

            // The "makeItem" function will be called as needed
            // when the ListView needs more items to render
            Func<VisualElement> makeItem = () => {
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
            };

            // As the user scrolls through the list, the ListView object
            // will recycle elements created by the "makeItem"
            // and invoke the "bindItem" callback to associate
            // the element with the matching data item (specified as an index in the list)
            Action<VisualElement, int> bindItem = (e, i) => (e as VisualElement).Q<Label>().text = items[i];

            var listView = this.Q<ListView>("storage-list");
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;
            listView.itemsSource = items;
            listView.selectionType = SelectionType.Multiple;
            // Callback invoked when the user double clicks an item
            listView.onItemChosen += obj => Debug.Log(obj);

            // Callback invoked when the user changes the selection inside the ListView
            listView.onSelectionChanged += objects => Debug.Log(objects);
        }
    }

    internal class StorageElementFactory : UxmlFactory<StorageElement> { }
}