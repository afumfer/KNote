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
    public string AppUserName { get; set; }

    public string ToolsPath { get; set; }

    public RepositoryRef RepositoryRef { get; set; }

    public KntRedmineConfig()
    {
        AppUserName = "";
        ToolsPath = "";
        RepositoryRef = new RepositoryRef();
    }

}
