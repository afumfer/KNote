using KNote.Model;
using KNote.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Service.Core;

public abstract class KntServiceBase : DomainActionBase
{
    private readonly IKntService _parentService;
    internal IKntService ParentService
    {
        get { return _parentService; }
    }

    internal IKntRepository Repository
    {
        get { return _parentService.Repository; }
    }

    public KntServiceBase(IKntService service)
    {
        _parentService = service;
    }


}
