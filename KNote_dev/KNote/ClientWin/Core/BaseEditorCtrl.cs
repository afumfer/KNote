using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model;

namespace KNote.ClientWin.Core
{
    //abstract public class BaseEditorCtrl<TEntity, TView> : BaseCtrl
    //    where TEntity: class, new ()
    //    where TView: IEditorView<TEntity>
    //{

    //    public KntService Service { get; protected set; }

    //    public TEntity Entity { get; protected set; }
        
    //    public BaseEditorCtrl(KntContext context)
    //       : base(context)
    //    {

    //    }

    //    public BaseEditorCtrl(KntContext context, KntService service, TEntity entity)
    //       : base(context)
    //    {
    //        Service = service;
    //        Entity = entity;            
    //    }

    //    private TView _view;
    //    public TView View         
    //    {
    //        get
    //        {
    //            if (_view == null)
    //            {
    //                _view = CreateView();
    //            }
    //            return _view;
    //        }
    //    }

    //    #region Abstract methods

    //    protected abstract TView CreateView();
               
    //    public abstract Result<CtrlResult> UpdateModelCtrl(KntService service, Guid? idEntity);

    //    public abstract Result<CtrlResult> SaveEntityAction();

    //    public abstract Result<CtrlResult> CancelAction();

    //    #endregion

    //}
}
