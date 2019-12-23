using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Shared
{
    public enum EnumKAttributeDataType
    {
        dtTag,
        dtBool,
        dtInt,
        dtDouble,
        dtString,
        dtDateTime,
        dtTabulate,
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
}
