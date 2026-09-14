using System.Runtime.InteropServices;

namespace KNote.ClientWin.Utils;

/// <summary>
/// Wires click-to-sort onto a ListView's column headers: the first click on a header sorts that column
/// ascending, a second click on the same header flips to descending, and the header shows the native
/// Windows sort arrow (drawn by the OS header control, same as Explorer/DataGridView - no icon resources
/// needed, follows the current visual theme automatically).
///
/// Usage: keep the returned ListViewColumnSorter as a field on the Form and pass it to ReapplySort after
/// any code mutates listView.Items (Add/Update/Remove), since ListView does not re-sort automatically
/// when its Items collection changes.
/// </summary>
public static class ListViewSortHelper
{
    public static ListViewColumnSorter Attach(ListView listView, IDictionary<int, Comparison<ListViewItem>>? customComparers = null)
    {
        var sorter = new ListViewColumnSorter(customComparers);
        listView.ListViewItemSorter = sorter;
        listView.ColumnClick += (s, e) => OnColumnClick(listView, sorter, e.Column);
        return sorter;
    }

    /// <summary>
    /// Re-sorts the list using the sorter's current column/direction. Call after Add/Update/Remove so a
    /// previously applied sort survives the mutation. No-op while no column has been clicked yet.
    /// </summary>
    public static void ReapplySort(ListView listView, ListViewColumnSorter sorter)
    {
        if (sorter.Order == SortOrder.None)
            return;

        listView.Sort();
    }

    private static void OnColumnClick(ListView listView, ListViewColumnSorter sorter, int column)
    {
        if (sorter.SortColumn == column && sorter.Order != SortOrder.None)
            sorter.Order = sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
        else
        {
            sorter.SortColumn = column;
            sorter.Order = SortOrder.Ascending;
        }

        listView.Sort();
        UpdateHeaderSortIndicators(listView, sorter.SortColumn, sorter.Order);
    }

    #region Native header sort-arrow indicator

    private static void UpdateHeaderSortIndicators(ListView listView, int sortColumn, SortOrder order)
    {
        if (!listView.IsHandleCreated)
            return;

        var headerHandle = NativeMethods.SendMessage(listView.Handle, NativeMethods.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
        if (headerHandle == IntPtr.Zero)
            return;

        for (int i = 0; i < listView.Columns.Count; i++)
        {
            var item = new NativeMethods.HDITEM { mask = NativeMethods.HDI_FORMAT };
            NativeMethods.SendMessage(headerHandle, NativeMethods.HDM_GETITEM, new IntPtr(i), ref item);

            item.fmt &= ~(NativeMethods.HDF_SORTUP | NativeMethods.HDF_SORTDOWN);
            if (i == sortColumn && order != SortOrder.None)
                item.fmt |= order == SortOrder.Ascending ? NativeMethods.HDF_SORTUP : NativeMethods.HDF_SORTDOWN;

            NativeMethods.SendMessage(headerHandle, NativeMethods.HDM_SETITEM, new IntPtr(i), ref item);
        }
    }

    private static class NativeMethods
    {
        internal const int LVM_GETHEADER = 0x101F;
        internal const int HDM_GETITEM = 0x120B;
        internal const int HDM_SETITEM = 0x120C;
        internal const int HDI_FORMAT = 0x0004;
        internal const int HDF_SORTUP = 0x0400;
        internal const int HDF_SORTDOWN = 0x0200;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct HDITEM
        {
            public int mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)] public string? pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam;
            public int iImage;
            public int iOrder;
            public int type;
            public IntPtr pvFilter;
            public int state;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref HDITEM lParam);
    }

    #endregion
}
