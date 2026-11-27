using KNote.ClientWin.Utils;
using KNote.Model.Core;

namespace KNote.ClientWin.Views;

/// <summary>
/// The legend under a list of the history (Model/Core/HistoryRowStates, the same as the web app): the states that appear in it, once
/// each, with a swatch of the tone of their rows and why. Hidden if no row has one. Its lines wrap to its width.
/// </summary>
public class HistoryStatesLegend : FlowLayoutPanel
{
    public HistoryStatesLegend()
    {
        FlowDirection = FlowDirection.TopDown;
        WrapContents = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Visible = false;
    }

    public void SetStates(IEnumerable<EnumHistoryRowState> states)
    {
        var shown = HistoryRowStates.InLegend(states);
        SuspendLayout();
        foreach (Control control in Controls.Cast<Control>().ToList())
            control.Dispose();
        Controls.Clear();
        foreach (var state in shown)
            Controls.Add(Line(state));
        Visible = shown.Count > 0;
        FitLines();
        ResumeLayout(true);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        FitLines();
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        if (Parent != null)
            Parent.ClientSizeChanged += (s, a) => FitLines();
    }

    private Control Line(EnumHistoryRowState state)
    {
        var line = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, LogicalToDeviceUnits(2))
        };
        var swatch = new Panel
        {
            BackColor = HistoryRowColors.For(state), BorderStyle = BorderStyle.FixedSingle,
            Size = new Size(LogicalToDeviceUnits(22), LogicalToDeviceUnits(14)), Margin = new Padding(0, LogicalToDeviceUnits(2), LogicalToDeviceUnits(6), 0)
        };
        var text = new Label
        {
            AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(0),
            Text = $"{HistoryRowStates.Text(state)}. {HistoryRowStates.Legend(state)}"
        };
        line.Controls.Add(swatch);
        line.Controls.Add(text);
        return line;
    }

    // Each text as wide as the room the parent leaves (its lines wrap), not wider.
    private void FitLines()
    {
        var room = (Parent?.ClientSize.Width ?? Width) - Margin.Horizontal - Padding.Horizontal - LogicalToDeviceUnits(40);
        if (room <= 0)
            return;
        foreach (var line in Controls.OfType<FlowLayoutPanel>())
            foreach (var label in line.Controls.OfType<Label>())
                label.MaximumSize = new Size(room, 0);
    }
}
