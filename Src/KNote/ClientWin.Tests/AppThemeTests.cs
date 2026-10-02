using KNote.ClientWin.Utils;
using KNote.Model;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AppThemeTests
{
    [TestMethod]
    [DataRow(AppColorMode.Light, SystemColorMode.Classic)]
    [DataRow(AppColorMode.Dark, SystemColorMode.Dark)]
    [DataRow(AppColorMode.System, SystemColorMode.System)]
    public void ToSystemColorMode_MapsEachSettingToItsWinFormsMode(AppColorMode mode, SystemColorMode expected)
    {
        Assert.AreEqual(expected, AppTheme.ToSystemColorMode(mode));
    }

    [TestMethod]
    public void AdjustControls_FlattensNested3DBordersAndTabPageBackgrounds()
    {
        using var form = new Form();
        var tabs = new TabControl();
        var page = new TabPage { UseVisualStyleBackColor = true };
        var textBox = new TextBox();                                          // Fixed3D by default
        var tree = new TreeView();                                            // Fixed3D by default
        var panel = new Panel { BorderStyle = BorderStyle.Fixed3D };
        var list = new ListView();
        panel.Controls.Add(list);
        page.Controls.AddRange(new Control[] { textBox, tree, panel });
        tabs.TabPages.Add(page);
        form.Controls.Add(tabs);

        AppTheme.AdjustControls(form);

        Assert.IsFalse(page.UseVisualStyleBackColor);
        Assert.AreEqual(BorderStyle.FixedSingle, textBox.BorderStyle);
        Assert.AreEqual(BorderStyle.FixedSingle, tree.BorderStyle);
        Assert.AreEqual(BorderStyle.FixedSingle, panel.BorderStyle);
        Assert.AreEqual(BorderStyle.FixedSingle, list.BorderStyle);
    }

    [TestMethod]
    public void AdjustControls_SingleLineTextBox_ShrinksOnePixelPerSideOnlyOnce()
    {
        using var form = new Form();
        var textBox = new TextBox { Location = new Point(10, 20) };
        form.Controls.Add(textBox);
        int height = textBox.Height;

        AppTheme.AdjustControls(form);
        AppTheme.AdjustControls(form);   // e.g. KNoteManagementForm: on Load and again after LinkComponents

        Assert.AreEqual(21, textBox.Top);
        Assert.AreEqual(height - 2, textBox.Height);
    }

    [TestMethod]
    public void AdjustControls_SingleLineTextBoxWithItsDesignerHeight_ShrinksFromItsAutomaticHeight()
    {
        // In a hidden panel (no handle yet) the box still has the Designer's height, smaller than its
        // automatic one: shrinking that would leave it too short for its font.
        using var form = new Form();
        var panel = new Panel { Visible = false };
        var textBox = new TextBox { Location = new Point(7, 4), Size = new Size(300, 20) };
        panel.Controls.Add(textBox);
        form.Controls.Add(panel);

        AppTheme.AdjustControls(form);

        Assert.AreEqual(textBox.PreferredHeight - 2, textBox.Height);
        Assert.AreEqual(5, textBox.Top);
    }

    [TestMethod]
    public void AdjustControls_MultilineTextBox_KeepsItsBounds()
    {
        using var form = new Form();
        var textBox = new TextBox { Multiline = true, Bounds = new Rectangle(10, 20, 200, 100) };
        form.Controls.Add(textBox);

        AppTheme.AdjustControls(form);

        Assert.AreEqual(BorderStyle.FixedSingle, textBox.BorderStyle);
        Assert.AreEqual(new Rectangle(10, 20, 200, 100), textBox.Bounds);
    }

    [TestMethod]
    public void AdjustControls_LeavesBorderlessControlsWithoutBorder()
    {
        using var form = new Form();
        var tree = new TreeView { BorderStyle = BorderStyle.None };
        var textBox = new TextBox { BorderStyle = BorderStyle.None };
        form.Controls.AddRange(new Control[] { tree, textBox });

        AppTheme.AdjustControls(form);

        Assert.AreEqual(BorderStyle.None, tree.BorderStyle);
        Assert.AreEqual(BorderStyle.None, textBox.BorderStyle);
    }
}
