using System.Collections.Generic;
using System.Text;

namespace KNote.Model
{
    public class Result : ResultBase
    {

    }

    public class Result<TEntity>: ResultBase
    {
        #region Properties

        public TEntity Entity { get; set; }
        
        public long Count { get; set; } = 1;

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

        // TODO: Penditne de comprobar y eliminar este código

        //public override bool IsValid
        //{
        //    get
        //    {
        //        // TODO: Revisar / repensar la condición de que la entidasd tenga que ser 
        //        //       distinta de null
        //        return (base.IsValid && Entity != null);                
        //    }
        //}

        #endregion

    }

}