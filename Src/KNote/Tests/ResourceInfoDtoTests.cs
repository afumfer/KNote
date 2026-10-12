using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// ResourceInfoDto.NameOut: the resource's file name without the note's guid prefix ("{guid}_{name}"), whether the
/// guid was stored with its dashes or with underscores.
/// </summary>
[TestClass]
public class ResourceInfoDtoTests
{
    [TestMethod]
    [DataRow("dc47b0c0-8d49-4580-8bb2-12112e3f4a5b_report.pdf", "report.pdf")]
    [DataRow("dc47b0c0_8d49_4580_8bb2_12112e3f4a5b_report.pdf", "report.pdf")]
    [DataRow("dc47b0c0_8d49_4580_8bb2_12112e3f4a5b_my_file_name.png", "my_file_name.png")]
    [DataRow("report.pdf", "report.pdf")]
    [DataRow("not_a_guid_prefix_at_all_but_long_enough_name.txt", "not_a_guid_prefix_at_all_but_long_enough_name.txt")]
    public void NameOut_DropsTheNoteGuidPrefix(string name, string expected)
    {
        Assert.AreEqual(expected, new ResourceInfoDto { Name = name }.NameOut);
    }
}
