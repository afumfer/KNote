namespace KNote.ClientWin.Core
{
    // The wait cursor while it lives. Application.UseWaitCursor keeps it during an await (with only Cursor.Current, the
    // first message of the mouse after the await gave the arrow back). Several can be alive at once and they don't end in
    // the order they started (an async void, as the folders tree when the app starts, outlives the WaitCursor around it):
    // they are counted, and the arrow comes back when the last one ends. Restoring "the previous value" instead left the
    // wait cursor for ever when the outer one ended first.
    public class WaitCursor : IDisposable
    {
        private static int _alive;
        private bool _disposed;

        public WaitCursor()
        {
            _alive++;
            Application.UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            if (--_alive > 0)
                return;
            _alive = 0;
            Application.UseWaitCursor = false;
            Cursor.Current = Cursors.Default;
        }
    }
}
