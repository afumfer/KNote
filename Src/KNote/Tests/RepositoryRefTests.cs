using System.Xml.Serialization;
using KNote.Model;

namespace KNote.Tests;

/// <summary>
/// RepositoryRef no longer keeps the repository's name in the configuration (it lives in the database), but
/// still reads the &lt;Alias&gt; of configuration files written before, and keeps writing it only until the
/// repository has been opened (its RepositoryId is known) - see RepositoryRef.LegacyAlias.
/// </summary>
[TestClass]
public class RepositoryRefTests
{
    private const string OldXml = """
        <RepositoryRef>
          <Alias>Personal repository</Alias>
          <ConnectionString>Data Source=C:\KaNote\Data\knote.db</ConnectionString>
          <Provider>Microsoft.Data.Sqlite</Provider>
          <Orm>EntityFramework</Orm>
        </RepositoryRef>
        """;

    [TestMethod]
    public void OldConfiguration_AliasBecomesTheName()
    {
        var repositoryRef = Deserialize(OldXml);

        Assert.AreEqual("Personal repository", repositoryRef.Name);
        Assert.AreEqual("Personal repository", repositoryRef.LegacyAlias);
        Assert.AreEqual(Guid.Empty, repositoryRef.RepositoryId);
    }

    [TestMethod]
    public void NotOpenedYet_KeepsWritingTheAlias()
    {
        var xml = Serialize(Deserialize(OldXml));

        StringAssert.Contains(xml, "<Alias>Personal repository</Alias>");
        Assert.IsFalse(xml.Contains("<Name>"));
        Assert.IsFalse(xml.Contains("RepositoryId"));
    }

    [TestMethod]
    public void OnceOpened_NeitherAliasNorNameNorIdAreWritten()
    {
        var repositoryRef = Deserialize(OldXml);
        repositoryRef.RepositoryId = Guid.NewGuid();
        repositoryRef.Name = "Name from the database";

        var xml = Serialize(repositoryRef);

        Assert.IsFalse(xml.Contains("Alias"));
        Assert.IsFalse(xml.Contains("<Name>"));
        Assert.IsFalse(xml.Contains("RepositoryId"));
        Assert.IsFalse(xml.Contains("Name from the database"));
    }

    [TestMethod]
    public void NewRepository_NameIsNotWritten()
    {
        var xml = Serialize(new RepositoryRef { Name = "New repository", Provider = "Microsoft.Data.Sqlite" });

        Assert.IsFalse(xml.Contains("New repository"));
        Assert.IsFalse(xml.Contains("Alias"));
    }

    [TestMethod]
    [DataRow("Microsoft.Data.Sqlite", @"Data Source=C:\KaNote\Data\knote_jdoe.db", "knote_jdoe")]
    [DataRow("Microsoft.Data.SqlClient", @"Data Source=MyServer\SQLEXPRESS; Initial Catalog=KNoteTeam; Trusted_Connection=True", "KNoteTeam")]
    [DataRow("Microsoft.Data.SqlClient", @"Server=MyServer;Database=KNoteDb;User Id=sa;Password=x", "KNoteDb")]
    [DataRow("Microsoft.Data.SqlClient", @"Data Source=MyServer", "KNote repository")]
    [DataRow("Microsoft.Data.Sqlite", "", "KNote repository")]
    [DataRow("Microsoft.Data.Sqlite", "not a connection string", "KNote repository")]
    public void GetDefaultName_IsTheDatabaseName(string provider, string connectionString, string expected)
    {
        var repositoryRef = new RepositoryRef { Provider = provider, ConnectionString = connectionString };

        Assert.AreEqual(expected, repositoryRef.GetDefaultName());
    }

    private static RepositoryRef Deserialize(string xml)
    {
        using var reader = new StringReader(xml);
        return (RepositoryRef)new XmlSerializer(typeof(RepositoryRef)).Deserialize(reader)!;
    }

    private static string Serialize(RepositoryRef repositoryRef)
    {
        using var writer = new StringWriter();
        new XmlSerializer(typeof(RepositoryRef)).Serialize(writer, repositoryRef);
        return writer.ToString();
    }
}
