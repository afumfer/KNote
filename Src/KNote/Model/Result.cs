using System.Collections.Generic;
using System.Text;

namespace KNote.Model;

public class Result : ResultBase
{

}

public class Result<TEntity>: ResultBase
{
    #region Properties

    public TEntity Entity { get; set; }
    
    public long TotalCount { get; set; } = -1;

    #endregion

    #region Constructor

    public Result(TEntity entity) : this()
    {
        Entity = entity;            
    }

    public Result(): base()
    {

    }

    #endregion

}