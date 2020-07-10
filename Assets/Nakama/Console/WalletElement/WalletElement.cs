using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine.UIElements;

namespace NakamaConsole
{

    public class WalletElement : IMGUIContainer
    {
        public new class UxmlFactory : UxmlFactory<WalletElement> {}

        private ReorderableList ledger;

        public WalletElement()
        {
            this.onGUIHandler += handleOnGui;

            
            var item1 = new LedgerItem("testId1", 4);
            var item2 = new LedgerItem("testId2", 5);
            var item3 = new LedgerItem("testId3", 3);
            var item4 = new LedgerItem("testId4", 2);
            
            ledger = new ReorderableList(new List<LedgerItem>{item1, item2, item3, item4}, typeof(LedgerItem));

        }

        private void handleOnGui()
        {
            ledger.DoLayoutList();
        }
    }

}