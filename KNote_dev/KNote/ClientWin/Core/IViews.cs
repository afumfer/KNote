using KNote.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Core
{
    #region  Base view

    public interface IViewBase
    {
        Control PanelView();
        void ShowView();
        Result<EComponentResult> ShowModalView();
        void OnClosingView();
        void RefreshView();
        DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK);
    }

    public interface IViewConfigurable: IViewBase
    {
        void ConfigureEmbededMode();
        void ConfigureWindowMode();
    }

    #endregion

    #region Generals views

    public interface IEditorView<T> : IViewConfigurable
    {        
        void CleanView();        
        void RefreshBindingModel();        
    }

    public interface ISelectorView<TItem> : IViewConfigurable
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
