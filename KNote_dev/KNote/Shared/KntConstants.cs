using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Shared
{
    public static class KntConst
    {
        // KAttributes
        public static Dictionary<EnumKAttributeDataType, string> KAttributes { get; set; }

        // EventType

        // AlarmType

        // NotificationType

        // ActionType

        // Roles
        public static Dictionary<EnumRoles, string> Roles { get; set; }


        static KntConst()
        {
            KAttributes = new Dictionary<EnumKAttributeDataType, string>
            {
                { EnumKAttributeDataType.String, "String"},
                { EnumKAttributeDataType.Int, "Integer"},
                { EnumKAttributeDataType.Double, "Double"},
                { EnumKAttributeDataType.DateTime, "Date Time"},
                { EnumKAttributeDataType.Bool, "Boolean"},
                { EnumKAttributeDataType.TabulatedValue, "Tabulated value"},
                { EnumKAttributeDataType.Tag, "Tag"}
            };

            Roles = new Dictionary<EnumRoles, string>
            {
                { EnumRoles.Public, "Public user"},
                { EnumRoles.Staff, "Staff"},
                { EnumRoles.ProjecManager, "Project manager"},
                { EnumRoles.Admin, "Admin"}
            };

        }
    }

    #region Enums 

    public enum EnumKAttributeDataType
    {
        String,
        Int,
        Double,
        DateTime,
        Bool,
        TabulatedValue,
        Tag
    }

    public enum EnumRoles
    {
        Public,
        Staff,
        ProjecManager,
        Admin
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
        PrivateInfo,
        PostIt,
        Email
    }

    public enum EnumActionType
    {
        UserAlarm,
        NoteAlarm,
        UserMessage,
        NoteMessage,
        ScriptExecution
    }



    #endregion 
}
