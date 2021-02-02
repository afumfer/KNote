using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Model
{
    public static class KntConst
    {       
        public static Dictionary<EnumKAttributeDataType, string> KAttributes { get; set; }
     
        public static Dictionary<EnumAlarmType, string> AlarmType { get; set; }
        
        public static Dictionary<EnumNotificationType, string> NotificationType { get; set; }
        
        public static Dictionary<EnumActionType, string> ActionType { get; set; }

        public static Dictionary<EnumRoles, string> Roles { get; set; }

        public static string ContainerResources { get; set; } = @"NotesResources";

        // EventType

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
}
