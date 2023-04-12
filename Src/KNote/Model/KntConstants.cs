using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace KNote.Model;

public static class KntConst
{       
    public static Dictionary<EnumKAttributeDataType, string> KAttributes { get; private set; }
    public static Dictionary<EnumAlarmType, string> AlarmType { get; private set; }   
    public static Dictionary<EnumNotificationType, string> NotificationType { get; private set; }    
    public static Dictionary<EnumActionType, string> ActionType { get; private set; }
    public static Dictionary<EnumRoles, string> Roles { get; private set; }
    public static Dictionary<EnumStatus, string> Status { get; private set; }
    public static int DefaultFolderNumber { get; } = 1;
    public static string HelpUrl { get; } = @"https://github.com/afumfer/KNote/blob/master/Docs/Manual.md";
    public static string GithubProject { get; } = @"https://github.com/afumfer/KNote";
    public static string License { get; private set; }

    static KntConst()
    {
        KAttributes = new Dictionary<EnumKAttributeDataType, string>
        {
            { EnumKAttributeDataType.String, "String"},
            { EnumKAttributeDataType.TextArea, "Text Area"},
            { EnumKAttributeDataType.Int, "Integer"},
            { EnumKAttributeDataType.Double, "Double"},
            { EnumKAttributeDataType.DateTime, "Date Time"},
            { EnumKAttributeDataType.Bool, "Boolean"},
            { EnumKAttributeDataType.TabulatedValue, "Tabulated Value"},
            { EnumKAttributeDataType.TagsValue, "Tags Value"}
        };

        Roles = new Dictionary<EnumRoles, string>
        {
            { EnumRoles.Public, "Public user"},
            { EnumRoles.Staff, "Staff"},
            { EnumRoles.ProjecManager, "Project manager"},
            { EnumRoles.Admin, "Admin"}
        };

        Status = new Dictionary<EnumStatus, string>
        {
            { EnumStatus.Resolved, "Resolved"},
            { EnumStatus.AlarmsPending, "Alarms Pending"}
        };

        AlarmType = new Dictionary<EnumAlarmType, string>
        {
            { EnumAlarmType.Standard, "Standard" },
            { EnumAlarmType.Daily, "Daily" },
            { EnumAlarmType.Weekly, "Weekly" },
            { EnumAlarmType.Monthly, "Monthly" },
            { EnumAlarmType.Annual, "Annual" },
            { EnumAlarmType.InMinutes, "InMinutes" }
        };

        NotificationType = new Dictionary<EnumNotificationType, string> 
        {                
            { EnumNotificationType.PostIt, "PostIt" },
            { EnumNotificationType.Email, "Email" },
            { EnumNotificationType.AppInfo, "Application info" },
            { EnumNotificationType.ExecuteKntScript, "Execute KntScript" }
        };

        ActionType = new Dictionary<EnumActionType, string> 
        {
            { EnumActionType.UserAlarm, "User Alarm" },
            { EnumActionType.NoteAlarm, "Note Alarm" },
            { EnumActionType.UserMessage, "User Message" },
            { EnumActionType.NoteMessage, "Note Message" },
            { EnumActionType.ScriptExecution, "Script Execution" }
        };

        StringBuilder info = new();
        info.Append(@"Permission is hereby granted, free of charge, to any person obtaining a copy of this ");
        info.Append("software and associated documentation files (the 'Software'), to deal in the Software without ");
        info.Append("restriction, including without limitation the rights to use and copy.");
        info.Append(Environment.NewLine + Environment.NewLine);
        info.Append("THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, ");
        info.Append("INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR ");
        info.Append("PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE ");
        info.Append("FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ");
        info.Append("ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.");
        License = info.ToString();
    }
}

#region Enums 

public enum EnumKAttributeDataType
{
    String,
    TextArea,
    Int,
    Double,
    DateTime,
    Bool,
    TabulatedValue,
    TagsValue
}

public enum EnumRoles
{
    Public,
    Staff,
    ProjecManager,
    Admin
}

public enum EnumStatus
{
    Resolved,
    AlarmsPending
}

public enum EnumAlarmType
{
    Standard,
    Daily,
    Weekly,
    Monthly,
    Annual,
    InMinutes
}

public enum EnumNotificationType
{        
    PostIt,
    Email,
    AppInfo,
    ExecuteKntScript
}

public enum EnumActionType
{
    UserAlarm,
    NoteAlarm,
    UserMessage,
    NoteMessage,
    ScriptExecution
}

public enum EnumEventType
{
    OnCreateActionDefault,
    OnSaveActionDefault,
    OnDeleteActionDefault,
    OnPropertyGetValueActionDefault,
    OnPropertyChangeActionDefault,
    OnCreateScriptExec,
    OnSaveScriptExec,
    OnDeleteScriptExec,
    OnPropertyGetValueScriptExec,
    OnPropertyChangeScriptExec
}


#endregion 
