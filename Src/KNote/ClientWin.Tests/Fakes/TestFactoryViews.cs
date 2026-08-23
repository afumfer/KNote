using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IFactoryViews test double: an empty registry the test populates directly.</summary>
internal class TestFactoryViews : IFactoryViews
{
    public ViewFactoryRegistry Registry { get; } = new();
}
