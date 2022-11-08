using KNote.Model;
using KNote.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KntRedmineApi;

public class KntRedmineConfig
{    
    public RepositoryRef RepositoryRef { get; set; }

    public KntRedmineConfig()
    {
        RepositoryRef = new RepositoryRef();
    }

    public KntRedmineConfig(RepositoryRef repositoryRef)
    {
        RepositoryRef = repositoryRef;
    }

}
