using System;

namespace KNote.Model;

// RS-232 settings of the KntServerCOM component (ClientWin/Controllers/KntServerCOMCtrl), persisted in
// KNoteData.config as AppUserSettings.Connectivity.ServerCOM. Enum-like values (HandShake, Parity, StopBits) are stored as the
// int values of the System.IO.Ports enums of the same name: this project can't reference System.IO.Ports.
// Every property has a default, so a config file saved before this section existed loads with them.
[Serializable]
public class ServerCOMConfig
{
    // Defaults of the serial parameters the old code hard-wired (see ResetSerialParameters).
    public const int DefaultParity = 0;     // Parity.None
    public const int DefaultDataBits = 8;
    public const int DefaultStopBits = 2;   // StopBits.Two

    public string PortName { get; set; } = "COM1";

    public int BaudRate { get; set; } = 115200;

    // Handshake: 0 None, 1 XOnXOff, 2 RequestToSend, 3 RequestToSendXOnXOff.
    public int HandShake { get; set; } = 0;

    // Parity: 0 None, 1 Odd, 2 Even, 3 Mark, 4 Space.
    public int Parity { get; set; } = DefaultParity;

    public int DataBits { get; set; } = DefaultDataBits;

    // StopBits: 1 One, 2 Two, 3 OnePointFive.
    public int StopBits { get; set; } = DefaultStopBits;

    // Pause (in milliseconds) between the packets sent to the device, needed by old retro hardware.
    public int RetroDelay { get; set; } = 60;

    public void ResetSerialParameters()
    {
        Parity = DefaultParity;
        DataBits = DefaultDataBits;
        StopBits = DefaultStopBits;
    }

    // Returns the first validation error, or null when the settings are valid.
    public string Validate()
    {
        if (string.IsNullOrWhiteSpace(PortName))
            return "The port name is required.";
        if (BaudRate <= 0)
            return "The baud rate must be greater than zero.";
        if (HandShake < 0 || HandShake > 3)
            return "The handshake must be between 0 (None) and 3 (RequestToSendXOnXOff).";
        if (Parity < 0 || Parity > 4)
            return "The parity must be between 0 (None) and 4 (Space).";
        if (DataBits < 5 || DataBits > 8)
            return "The data bits must be between 5 and 8.";
        if (StopBits < 1 || StopBits > 3)
            return "The stop bits must be between 1 (One) and 3 (OnePointFive).";
        if (RetroDelay < 0 || RetroDelay > 5000)
            return "The retro delay must be between 0 and 5000 milliseconds.";
        return null;
    }
}
