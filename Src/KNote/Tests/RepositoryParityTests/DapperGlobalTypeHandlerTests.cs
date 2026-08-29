using KNote.Repository.Dapper;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// Regression coverage for the "only one Dapper database works when mixing engines" bug:
/// SqlMapper.AddTypeHandler(...) registers process-wide, not per-connection (see
/// Repository.Dapper/KntRepositoryDapperBase.cs). Once a Sqlite-backed Dapper repository has
/// registered these handlers - which used to only parse Sqlite's string-stored values - they also
/// ran for a SQL Server connection in the same process, which returns Guid/DateTimeOffset/TimeSpan
/// natively, not as strings, causing InvalidCastException. These handlers now accept both shapes.
/// </summary>
[TestClass]
public class DapperGlobalTypeHandlerTests
{
    [TestMethod]
    public void GuidHandler_ParsesBothSqliteStringAndSqlServerNativeGuid()
    {
        var handler = new GuidHandler();

        var fromSqliteText = handler.Parse(Guid.NewGuid().ToString());
        Assert.AreNotEqual(Guid.Empty, fromSqliteText);

        // What SqlDataReader.GetValue() returns for a uniqueidentifier column - used to throw
        // InvalidCastException because Parse assumed the value was always a Sqlite-stored string.
        var nativeGuid = Guid.NewGuid();
        var fromNativeGuid = handler.Parse(nativeGuid);
        Assert.AreEqual(nativeGuid, fromNativeGuid);
    }

    [TestMethod]
    public void DateTimeOffsetHandler_ParsesBothSqliteStringAndNativeValue()
    {
        var handler = new DateTimeOffsetHandler();
        var native = DateTimeOffset.UtcNow;

        Assert.AreEqual(native, handler.Parse(native.ToString("o")));
        Assert.AreEqual(native, handler.Parse(native));
    }

    [TestMethod]
    public void TimeSpanHandler_ParsesBothSqliteStringAndNativeValue()
    {
        var handler = new TimeSpanHandler();
        var native = TimeSpan.FromMinutes(90);

        Assert.AreEqual(native, handler.Parse(native.ToString()));
        Assert.AreEqual(native, handler.Parse(native));
    }
}
