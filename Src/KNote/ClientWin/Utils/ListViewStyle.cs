namespace KNote.ClientWin.Utils;

/// <summary>
/// The look shared by every detail-view list in the app (notes' sub-lists, the "manage" screens, the
/// selectors): report view with grid lines and full-row selection, no label editing or column
/// reordering, unsorted until the user clicks a header (see ListViewSortHelper).
/// </summary>
public static class ListViewStyle
{
    public static void ApplyStandard(ListView listView, bool checkBoxes = false)
    {
        listView.View = View.Details;
        listView.LabelEdit = false;
        listView.AllowColumnReorder = false;
        listView.CheckBoxes = checkBoxes;
        listView.FullRowSelect = true;
        listView.GridLines = true;
        listView.Sorting = SortOrder.None;
    }
}
