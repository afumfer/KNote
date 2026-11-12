using KNote.ClientWin.Controllers;
using KNote.ClientWin.Utils;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// KNote Atlas, stage 4 (D14, 4.6b): «Gallery» of the photos found by meaning, the same list as the one of NotesSelectorForm
// (NotesSelectorCtrl.ResourceHits) with big thumbnails: its title, its note and its similarity under each one. Not modal and in sync
// with the list: choosing a photo here selects its row (the preview follows it) and the other way round; a double click opens the
// photo and the context menu its note. It is reloaded with the list, and its bounds are kept (AppUserState.ResourceGalleryWindow).
public class ResourceGalleryForm : KntForm
{
    // The thumbnails: 4:3, the biggest an ImageList allows is 256.
    private const int ThumbnailWidth = 200;
    private const int ThumbnailHeight = 150;

    private readonly NotesSelectorCtrl _ctrl;
    private readonly NotesSelectorForm _list;
    private readonly ListView _view;
    private readonly ImageList _images;
    private readonly List<ResourceSearchHitDto> _hits = new();
    private bool _selecting;
    private int _version;

    public ResourceGalleryForm(NotesSelectorCtrl ctrl, NotesSelectorForm list)
    {
        _ctrl = ctrl;
        _list = list;
        Text = "Photos found";
        ShowInTaskbar = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(LogicalToDeviceUnits(900), LogicalToDeviceUnits(640));

        _images = new ImageList
        {
            ColorDepth = ColorDepth.Depth32Bit,
            ImageSize = new Size(Math.Min(256, LogicalToDeviceUnits(ThumbnailWidth)), Math.Min(192, LogicalToDeviceUnits(ThumbnailHeight)))
        };
        _view = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.LargeIcon,
            LargeImageList = _images,
            MultiSelect = false,
            HideSelection = false,
            BorderStyle = BorderStyle.None
        };
        _view.SelectedIndexChanged += (s, e) =>
        {
            if (!_selecting && SelectedHit() is ResourceSearchHitDto hit)
                _list.SelectHit(hit);
        };
        _view.DoubleClick += (s, e) =>
        {
            if (SelectedHit() is ResourceSearchHitDto hit)
                _list.OpenResource(hit);
        };

        var menu = new KntContextMenuStrip();
        menu.Items.Add("Open photo", null, (s, e) => { if (SelectedHit() is ResourceSearchHitDto hit) _list.OpenResource(hit); });
        menu.Items.Add("Open note", null, (s, e) => { if (SelectedHit() is ResourceSearchHitDto hit) _list.OpenNote(hit); });
        _view.ContextMenuStrip = menu;
        Controls.Add(_view);

        RestoreWindowBounds();
        Reload();
    }

    /// <summary>The photos of the list, again (another search).</summary>
    public void Reload()
    {
        var version = ++_version;
        _view.BeginUpdate();
        _view.Items.Clear();
        _images.Images.Clear();
        _hits.Clear();
        _hits.AddRange(_ctrl.ResourceHits);
        foreach (var hit in _hits)
            _view.Items.Add(new ListViewItem($"{hit.Resource.GetTitle()}\n#{hit.NoteNumber} {hit.Topic} · {hit.Similarity:0.00}") { Tag = hit });
        _view.EndUpdate();
        _ = LoadThumbnailsAsync(version);
    }

    /// <summary>Shows a photo selected in the list.</summary>
    public void SelectHit(ResourceSearchHitDto hit)
    {
        var item = _view.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Tag == hit);
        if (item == null || item.Selected)
            return;
        _selecting = true;
        try
        {
            _view.SelectedItems.Clear();
            item.Selected = true;
            item.EnsureVisible();
        }
        finally
        {
            _selecting = false;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _version++;
        SaveWindowBounds();
        base.OnFormClosing(e);
    }

    private ResourceSearchHitDto SelectedHit() => _view.SelectedItems.Count > 0 ? _view.SelectedItems[0].Tag as ResourceSearchHitDto : null;

    // One after another, in the order of the list: the first ones are seen first.
    private async Task LoadThumbnailsAsync(int version)
    {
        var size = _images.ImageSize;
        foreach (var item in _view.Items.Cast<ListViewItem>().ToList())
        {
            if (version != _version || IsDisposed)
                return;
            var bytes = await _ctrl.GetThumbnailAsync((ResourceSearchHitDto)item.Tag, Math.Max(size.Width, size.Height));
            if (version != _version || IsDisposed || bytes == null)
                continue;
            using var stream = new MemoryStream(bytes);
            using var photo = Image.FromStream(stream);
            _images.Images.Add(item.GetHashCode().ToString(), Fit(photo, size));
            item.ImageKey = item.GetHashCode().ToString();
        }
    }

    // The photo whole in the size of the thumbnails, centered on the background of the list.
    private Bitmap Fit(Image photo, Size size)
    {
        var bitmap = new Bitmap(size.Width, size.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(_view.BackColor);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        var scale = Math.Min((float)size.Width / photo.Width, (float)size.Height / photo.Height);
        var width = (int)(photo.Width * scale);
        var height = (int)(photo.Height * scale);
        graphics.DrawImage(photo, (size.Width - width) / 2, (size.Height - height) / 2, width, height);
        return bitmap;
    }

    // The saved position can be off every current screen (monitor or Windows scale changed since it was saved).
    private void RestoreWindowBounds()
    {
        var bounds = _ctrl.Store.State.ResourceGalleryWindow.Bounds;
        if (bounds.Width > 0)
            Size = new Size(bounds.Width, bounds.Height);
        if (bounds.X != 0 || bounds.Y != 0)
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = WindowPlacement.EnsureVisible(new Rectangle(new Point(bounds.X, bounds.Y), Size));
        }
    }

    private void SaveWindowBounds()
    {
        var state = _ctrl.Store.State.ResourceGalleryWindow;
        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        state.Bounds.X = bounds.X;
        state.Bounds.Y = bounds.Y;
        state.Bounds.Width = bounds.Width;
        state.Bounds.Height = bounds.Height;
        _ctrl.Store.SaveConfig();
    }
}
