using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto.Info
{
    public class KLogInfoDto : KntModelBase
    {
        public Guid KLogId { get; set; }
        public Guid EntityId { get; set; }
        public string EntityName { get; set; }
        public DateTime RegistryDateTime { get; set; }
        public string RegistryMessage { get; set; }
    }
}
