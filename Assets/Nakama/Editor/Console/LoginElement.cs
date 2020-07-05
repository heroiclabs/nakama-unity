using System.Collections.Generic;
using UnityEngine.UIElements;

namespace NakamaConsole
{
    class LoginElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<LoginElement, UxmlTraits> { }

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