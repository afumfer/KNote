using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Core
{
    public class RepositoryRef
    {
        public string Alias { get; set; }
        public string ConnectionString { get; set; }
        public string Provider { get; set;  }
        public string Orm { get; set; }
    }
}
