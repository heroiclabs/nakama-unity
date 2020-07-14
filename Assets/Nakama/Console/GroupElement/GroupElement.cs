using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nakama.Console
{
    internal class GroupElement : VisualElement
    {
        public void Init()
        {   
            string groupRowPath = "Assets/Nakama/Console/GroupElement/GroupRow.uxml";
            var groupRowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(groupRowPath);
            TemplateContainer groupRow = groupRowAsset.CloneTree();
            Debug.Log("Adding group row " + groupRow);
            Add(groupRow);
            groupRow = groupRowAsset.CloneTree();
            Add(groupRow);
            groupRow = groupRowAsset.CloneTree();
            Add(groupRow);  
        }
    }

    internal class GroupElementFactory : UxmlFactory<GroupElement> { }
}