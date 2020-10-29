using System;
using System.Collections.Generic;
using System.Text;

using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Components
{
    public class FolderSelectorComponent : ComponentViewBase<ISelectorView<FolderWithServiceRef>>
    {
        public FolderSelectorComponent(Store store) : base(store)
        {

        }

        protected override ISelectorView<FolderWithServiceRef> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        protected override Result OnInitialized()
        {
            var result = base.OnInitialized();

            View.ShowView();


            return result;
        }

    }
}
