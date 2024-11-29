using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace KNote.Model;

public static class KntConst
{
    public static string AppName { get; } = "KaNote";
    public static string AppDescription { get; } = "Another keynotes managment";
    public static int DefaultFolderNumber { get; } = 1;
    public static string SupportedMimeTypes { get; } = @"image/jpeg;image/png;application/pdf;video/mp4;audio/mp3;text/plain";
    public static string HelpUrl { get; } = @"https://github.com/afumfer/KNote/blob/master/Docs/Manual.md";
    public static string GithubProject { get; } = @"https://github.com/afumfer/KNote";
    public static string License { get; private set; }
    public static string TagForMerging { get; private set; } = "[@NoteMerging]";    
    public static int MaxLenResourceFile { get; private set; } = 5120000;
    public static Dictionary<EnumKAttributeDataType, string> KAttributes { get; private set; }
    public static Dictionary<EnumAlarmType, string> AlarmType { get; private set; }   
    public static Dictionary<EnumNotificationType, string> NotificationType { get; private set; }    
    public static Dictionary<EnumActionType, string> ActionType { get; private set; }
    public static Dictionary<EnumRoles, string> Roles { get; private set; }
    public static Dictionary<EnumStatus, string> Status { get; private set; }
    
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
        info.Append(@"Copyright (C) 2021-2024 Armando Fumero Fernández ");
        info.Append(Environment.NewLine + Environment.NewLine);
        info.Append(@"This program is free software; you can redistribute it and/or modify ");
        info.Append(@"it under the terms of the GNU General Public License as published by ");
        info.Append(@"the Free Software Foundation; either version 2 of the License, or ");
        info.Append(@"(at your option) any later version. ");
        info.Append(Environment.NewLine + Environment.NewLine);
        info.Append(@"This program is distributed in the hope that it will be useful, ");
        info.Append(@"but WITHOUT ANY WARRANTY; without even the implied warranty of ");
        info.Append(@"MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the ");
        info.Append(@"GNU General Public License for more details. ");
        info.Append(Environment.NewLine + Environment.NewLine);
        info.Append(@"You should have received a copy of the GNU General Public License along ");
        info.Append(@"with this program; if not, write to the Free Software Foundation, Inc., ");
        info.Append(@"51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA. ");
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
