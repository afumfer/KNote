using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Service.Core
{
    internal abstract class KntCommandServiceBase<TParam, TResult>
    {
        public TParam Param { get; init; }

        public KntCommandServiceBase()
        {

        }

        public KntCommandServiceBase(TParam param)
        {
            Param = param;
        }

        public virtual bool ValidateAuthorization()
        {
            return true;
        }

        public virtual bool ValidateParamn()
        {
            return true;
        }

        public abstract TResult Execute();

    }
}
