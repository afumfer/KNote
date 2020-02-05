using System.Collections.Generic;
using System.Text;

namespace KNote.Shared
{
    public class Result<TEntity>: ResultBase        
    {
        #region Properties

        public TEntity Entity { get; set; }

        public int CountEntity { get; set; } = 1;

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

        #region Override methods

        public override bool IsValid
        {
            get
            {
                return (base.IsValid && Entity != null);
            }
        }

        #endregion

    }
}