namespace KNote.ClientWin.Utils;

/// <summary>
/// Dark mode look of a DataGridView (the notes list). WinForms' dark mode leaves the grid's column headers
/// light (they are drawn with the light visual styles) and its grid lines too bright, so in dark mode the
/// grid gets flat headers and its own colors instead. In light mode it is left as designed.
/// </summary>
public static class DataGridViewStyle
{
    internal static readonly Color DarkBack = Color.FromArgb(0x1F, 0x1F, 0x1F);
    internal static readonly Color DarkAlternateBack = Color.FromArgb(0x26, 0x26, 0x26);
    internal static readonly Color DarkHeaderBack = Color.FromArgb(0x2D, 0x2D, 0x2D);
    internal static readonly Color DarkHeaderText = Color.FromArgb(0xC8, 0xC8, 0xC8);
    internal static readonly Color DarkText = Color.FromArgb(0xE4, 0xE4, 0xE4);
    internal static readonly Color DarkGridLines = Color.FromArgb(0x3A, 0x3A, 0x3A);
    internal static readonly Color DarkSelectionBack = Color.FromArgb(0x26, 0x4F, 0x78);

    public static void ApplyDarkIfNeeded(DataGridView grid)
    {
        if (AppTheme.IsDark)
            ApplyDark(grid);
    }

    // Before the grid gets its rows (it changes the row template).
    internal static void ApplyDark(DataGridView grid)
    {
        grid.EnableHeadersVisualStyles = false;
        grid.BackgroundColor = DarkBack;
        grid.GridColor = DarkGridLines;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        // Set in code, so not scaled by WinForms (see "Escalado (DPI)" in ClientWin/CLAUDE.md).
        grid.RowTemplate.Height = Math.Max(grid.RowTemplate.Height, grid.LogicalToDeviceUnits(24));

        var header = grid.ColumnHeadersDefaultCellStyle;
        header.BackColor = DarkHeaderBack;
        header.ForeColor = DarkHeaderText;
        header.SelectionBackColor = DarkHeaderBack;
        header.SelectionForeColor = DarkHeaderText;
        // The header height is AutoSize: this padding is what makes it a bit taller than the rows.
        header.Padding = new Padding(grid.LogicalToDeviceUnits(4), grid.LogicalToDeviceUnits(4), 0, grid.LogicalToDeviceUnits(4));

        var cells = grid.DefaultCellStyle;
        cells.BackColor = DarkBack;
        cells.ForeColor = DarkText;
        cells.SelectionBackColor = DarkSelectionBack;
        cells.SelectionForeColor = Color.White;
        cells.Padding = new Padding(grid.LogicalToDeviceUnits(4), 0, grid.LogicalToDeviceUnits(4), 0);
        grid.AlternatingRowsDefaultCellStyle.BackColor = DarkAlternateBack;

        grid.CellPainting -= PaintLightSortGlyph;
        grid.CellPainting += PaintLightSortGlyph;
    }

    // Flat headers (EnableHeadersVisualStyles = false) draw the sort glyph in black, invisible over the dark
    // header: paint the header without it and draw it again in the header text color.
    private static void PaintLightSortGlyph(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex != -1 || e.ColumnIndex < 0 || e.Graphics == null)
            return;

        var grid = (DataGridView)sender;
        var sortOrder = grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection;
        if (sortOrder == SortOrder.None)
            return;

        e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentBackground);

        int width = grid.LogicalToDeviceUnits(8);
        int height = grid.LogicalToDeviceUnits(4);
        int x = e.CellBounds.Right - width - grid.LogicalToDeviceUnits(8);
        int y = e.CellBounds.Top + (e.CellBounds.Height - height) / 2;
        Point[] triangle = sortOrder == SortOrder.Ascending
            ? new[] { new Point(x, y + height), new Point(x + width, y + height), new Point(x + width / 2, y) }
            : new[] { new Point(x, y), new Point(x + width, y), new Point(x + width / 2, y + height) };

        var smoothing = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using (var brush = new SolidBrush(DarkHeaderText))
            e.Graphics.FillPolygon(brush, triangle);
        e.Graphics.SmoothingMode = smoothing;

        e.Handled = true;
    }
}
