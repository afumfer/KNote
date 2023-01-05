using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Service.Core;

public abstract class KntCommandServiceBase<TParam, TResult> : KntCommandServiceBase<TResult>
{
    public TParam Param { get; init; }

    public KntCommandServiceBase(IKntService service, TParam param) : base(service)
    {
        Param = param;
    }
}

public abstract class KntCommandServiceBase<TResult> 
{
    private readonly IKntService _service;
    internal IKntService Service
    {
        get { return _service; }
    }

    internal IKntRepository Repository
    {
        get { return _service.Repository; }
    }

    public KntCommandServiceBase(IKntService service)
    {
        _service = service;

    }

    public virtual bool ValidateAuthorization()
    {
        var aa = Service.UserIdentityName;
        return true;
    }

    public virtual bool ValidateParam()
    {
        return true;
    }

    public abstract Task<TResult> Execute();

    #region Protected methods

    protected string GetNoteStatus(List<NoteTaskDto> tasks, List<KMessageDto> messages)
    {
        string status = "";

        var tasksValid = tasks.Where(t => t.IsDeleted() == false).Select(t => t).ToList();
        var messagesValid = messages.Where(m => m.IsDeleted() == false).Select(m => m).ToList();

        bool allTaskResolved = true;
        if (tasksValid?.Count > 0)
        {
            foreach (var item in tasks)
            {
                if (item.Resolved == false)
                {
                    allTaskResolved = false;
                    break;
                }
            }
        }
        else
        {
            allTaskResolved = false;
        }

        bool alarmsPending = false;
        foreach (var item in messagesValid)
        {
            if (item.AlarmActivated == true)
            {
                alarmsPending = true;
                break;
            }

        }

        if (allTaskResolved == true)
            status = KntConst.Status[EnumStatus.Resolved];

        if (alarmsPending == true)
        {
            if (!string.IsNullOrEmpty(status))
                status += "; ";
            status += KntConst.Status[EnumStatus.AlarmsPending];
        }

        return status;
    }

    protected (string, string) GetResourceUrls(ResourceDto resource)
    {
        string rootUrl = Repository.RespositoryRef.ResourcesContainerCacheRootUrl;
        string relativeUrl;
        string fullUrl;

        if (string.IsNullOrEmpty(resource.Container))
        {
            resource.Container = Repository.RespositoryRef.ResourcesContainer + @"\" + DateTime.Now.Year.ToString();
            resource.ContentInDB = Repository.RespositoryRef.ResourceContentInDB;
        }

        if (string.IsNullOrEmpty(rootUrl) || string.IsNullOrEmpty(resource.Container) || string.IsNullOrEmpty(resource.Name))
            return (null, null);

        relativeUrl = (Path.Combine(resource.Container, resource.Name)).Replace(@"\", @"/");
        fullUrl = (Path.Combine(rootUrl, relativeUrl)).Replace(@"\", @"/");

        return (relativeUrl, fullUrl);
    }

    #endregion

}
