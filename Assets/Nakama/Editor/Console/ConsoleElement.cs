using System.Collections.Generic;
using UnityEngine.UIElements;

namespace NakamaConsole
{
    internal class ConsoleElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ConsoleElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
            }
        }
    }
}