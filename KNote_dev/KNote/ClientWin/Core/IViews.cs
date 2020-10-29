using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Core
{
    #region  Base view

    public interface IViewBase
    {
        void ShowView();
        void OnClosingView();
        void RefreshView();
        void ShowInfo(string info);
    }

    public interface IViewConfigurable
    {
        void ConfigureEmbededMode();
        void ConfigureWindowMode();
    }

    #endregion

    #region Generals views

    public interface IEditorView<T> : IViewBase, IViewConfigurable
    {        
        void CleanView();        
        void RefreshBindingModel();        
    }

    public interface ISelectorView<TItem> : IViewBase, IViewConfigurable
    {                
        void RefreshItem(TItem item);
        void DeleteItem(TItem item);
        void AddItem(TItem item);
        object SelectItem(TItem item);
    }

    #endregion

    #region Specific views

    #endregion 

}
