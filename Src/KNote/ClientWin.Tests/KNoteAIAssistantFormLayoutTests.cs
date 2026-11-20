using System.Reflection;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.ClientWin.Utils;
using KNote.ClientWin.Views;
using KNote.Model;

namespace KNote.ClientWin.Tests;

/// <summary>
/// The AI assistant's window when its splitter is dragged: the prompt's header (Send, New session, Attach, Internet and the model)
/// never covers the "Prompt:" label (the model's list narrows and the chat's panel has the header's minimum width), and a flat
/// (dark mode) combo is repainted whole when it is resized.
/// </summary>
[TestClass]
public class KNoteAIAssistantFormLayoutTests
{
    private static T Field<T>(object o, string name) => (T)o.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(o)!;

    private static void OnStaThread(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { error = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (error != null)
            throw new AssertFailedException($"The window failed: {error}");
    }

    [TestMethod]
    public void DraggingTheSplitter_NeverCoversThePromptLabel()
    {
        OnStaThread(() =>
        {
            var ctrl = new KNoteAIAssistantCtrl(TestStoreFactory.CreateEmpty()) { PersistSession = true };
            ctrl.SetChatClientForTesting(new FakeChatClient(), new AiProviderRef { Provider = EnumAiProvider.Anthropic, Alias = "Model" });
            using var form = new KNoteAIAssistantForm(ctrl) { StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000), ShowInTaskbar = false };
            form.GetType().GetMethod("ShowWebSearch", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(form, null);
            form.Show();
            Application.DoEvents();

            var split = Field<SplitContainer>(form, "splitSessions");
            var header = Field<Panel>(form, "panelPromptHeader");
            var label = Field<Label>(form, "labelPrompt");
            var combo = Field<ComboBox>(form, "comboProviders");
            var widest = combo.Width;

            for (var distance = split.SplitterDistance; distance >= 0; distance -= 20)
            {
                split.SplitterDistance = Math.Max(distance, split.Panel1MinSize);
                Application.DoEvents();
                var others = header.Controls.Cast<Control>().Where(c => c != label && c.Visible).ToList();
                Assert.IsTrue(others.All(c => c.Left > label.Right), $"at {split.SplitterDistance}: {string.Join(", ", others.Select(c => $"{c.Name}{c.Bounds}"))}");
                Assert.IsTrue(others.All(c => c.Right <= header.Width), $"at {split.SplitterDistance}");
            }
            Assert.AreEqual(split.Panel1MinSize, split.SplitterDistance, "dragged to its minimum");
            Assert.IsTrue(combo.Width < widest, "the model's list narrowed");
            Assert.AreEqual(widest, combo.DropDownWidth, "its list keeps the width");
        });
    }

    [TestMethod]
    public void TheLabels_StartWhereTheControlUnderThemDoes()
    {
        OnStaThread(() =>
        {
            var ctrl = new KNoteAIAssistantCtrl(TestStoreFactory.CreateEmpty()) { PersistSession = true };
            using var form = new KNoteAIAssistantForm(ctrl);

            void Check(string labelName, string controlName)
            {
                var label = Field<Label>(form, labelName);
                var under = Field<Control>(form, controlName);
                var padded = TextRenderer.MeasureText(label.Text, label.Font, Size.Empty, TextFormatFlags.Left | TextFormatFlags.SingleLine).Width;
                var unpadded = TextRenderer.MeasureText(label.Text, label.Font, Size.Empty, TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
                var textLeft = label.Parent!.PointToScreen(new Point(label.Left + (padded - unpadded) / 2, 0)).X;
                Assert.AreEqual(under.Parent!.PointToScreen(under.Location).X, textLeft, labelName);
            }
            _ = form.Handle;
            Check("labelResult", "kntEditViewResult");
            Check("labelPrompt", "textPrompt");
            Check("labelSessions", "comboRepositories");
        });
    }

    [TestMethod]
    public void TheRepositoriesCombo_IsRightUnderItsLabel_AndTheSessionsUnderIt()
    {
        OnStaThread(() =>
        {
            var ctrl = new KNoteAIAssistantCtrl(TestStoreFactory.CreateEmpty()) { PersistSession = true };
            using var form = new KNoteAIAssistantForm(ctrl) { StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000), ShowInTaskbar = false };
            form.Show();
            Application.DoEvents();

            var label = Field<Label>(form, "labelSessions");
            var combo = Field<ComboBox>(form, "comboRepositories");
            var header = Field<Panel>(form, "panelSessionsHeader");
            var sessions = Field<ListView>(form, "listViewSessions");

            Assert.AreEqual(label.Bottom + form.LogicalToDeviceUnits(4), combo.Top);
            Assert.AreEqual(header.Bottom, sessions.Top);
            Assert.AreEqual(combo.Bottom + header.Padding.Bottom, header.Height);
        });
    }

    [TestMethod]
    public void AFlatCombo_IsRepaintedWholeWhenResized()
    {
        OnStaThread(() =>
        {
            using var form = new Form { StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000), ShowInTaskbar = false };
            var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            form.Controls.Add(combo);
            AppTheme.AdjustControls(form);
            form.Show();
            Application.DoEvents();
            var invalidated = 0;
            combo.Invalidated += (_, _) => invalidated++;

            combo.Width += 50;

            Assert.AreEqual(FlatStyle.Flat, combo.FlatStyle);
            Assert.IsTrue(invalidated > 0);
        });
    }
}
