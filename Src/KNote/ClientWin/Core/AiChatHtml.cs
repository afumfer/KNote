using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Markdig;

namespace KNote.ClientWin.Core;

// HTML of the chat view of the AI assistant (KNoteAIAssistantForm in Navigation mode): the page with the
// conversation so far, loaded once, and the fragments its script (window.kntChat) adds or replaces while a
// new turn arrives (see Views/AiChatWebRenderer). Pure functions: no WebView2 here.
public static class AiChatHtml
{
    private const string StyleResourceName = "KNote.ClientWin.Resources.KNoteAIChat.css";

    // Raw HTML in an answer is shown as text: it is written by the model, and the page runs a script.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    private static string _style;

    // append(html): adds turns at the end and scrolls to them. replaceLast(html): replaces the last message
    // (the answer arriving), following it down only if the user had not scrolled up to read.
    // showModelInfo(visible): shows or hides the usage line of the answers. Links other than web, mail and
    // in-page ones do nothing; the host opens web and mail links outside the view. The host's shortcuts
    // (window.kntChatShortcuts, written as ShortcutText does) are posted to it: while the page has the focus,
    // the keys reach the browser, not the window's menu.
    private const string Script = """
        window.kntChat = (() => {
          const chat = document.getElementById('chat');
          const root = document.documentElement;
          const atBottom = () => window.innerHeight + window.scrollY >= root.scrollHeight - 48;
          const toBottom = () => window.scrollTo(0, root.scrollHeight);
          const shortcutOf = e => {
            const c = e.code;
            const key = c.startsWith('Key') ? c.slice(3) : c.startsWith('Digit') ? 'D' + c.slice(5) : c === 'NumpadEnter' ? 'Enter' : c;
            return (e.ctrlKey ? 'Ctrl+' : '') + (e.shiftKey ? 'Shift+' : '') + (e.altKey ? 'Alt+' : '') + key;
          };
          document.addEventListener('keydown', e => {
            const shortcut = shortcutOf(e);
            if (!window.chrome?.webview || !(window.kntChatShortcuts || []).includes(shortcut))
              return;
            e.preventDefault();
            window.chrome.webview.postMessage(shortcut);
          });
          document.addEventListener('click', e => {
            const a = e.target.closest('a[href]');
            if (a && !a.getAttribute('href').startsWith('#') && !/^(https?|mailto):$/.test(a.protocol))
              e.preventDefault();
          });
          toBottom();
          return {
            append(html) {
              chat.insertAdjacentHTML('beforeend', html);
              toBottom();
            },
            replaceLast(html) {
              const follow = atBottom();
              if (chat.lastElementChild)
                chat.lastElementChild.outerHTML = html;
              else
                chat.insertAdjacentHTML('beforeend', html);
              if (follow)
                toBottom();
            },
            showModelInfo(visible) {
              document.body.classList.toggle('hide-model-info', !visible);
            }
          };
        })();
        """;

    private const string TypingIndicator = "<div class=\"typing\"><span></span><span></span><span></span></div>";

    // markdownStyle: the <style> element of the app's Markdown viewer (Store.KNoteWebViewStyle), for the
    // answers' text; the chat layout goes on top of it. showModelInfo: the usage line of each answer.
    // hostShortcuts: the keys the page passes on to the host (see ShortcutText).
    public static string Page(IEnumerable<AiChatTurn> turns, string markdownStyle, bool showModelInfo = true,
        IEnumerable<string> hostShortcuts = null)
    {
        var html = new StringBuilder();
        html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        html.Append(markdownStyle);
        html.Append("<style>").Append(Style).Append("</style></head>");
        html.Append(showModelInfo ? "<body>" : "<body class=\"hide-model-info\">");
        html.Append("<main id=\"chat\">");
        foreach (var turn in turns)
            html.Append(UserMessage(turn.Prompt)).Append(AssistantMessage(turn));
        html.Append("</main><script>");
        html.Append("window.kntChatShortcuts = ").Append(JsonSerializer.Serialize(hostShortcuts ?? Array.Empty<string>())).Append(';');
        html.Append(Script).Append("</script></body></html>");
        return html.ToString();
    }

    // A shortcut as the page's script names the keys pressed: "Ctrl+K", "Ctrl+Enter", "Ctrl+Shift+F5".
    public static string ShortcutText(Keys keys)
    {
        var key = keys & Keys.KeyCode;
        // Keys.Enter and Keys.Return are the same value: its name is not reliable.
        var text = key == Keys.Return ? "Enter" : key.ToString();
        if (keys.HasFlag(Keys.Alt))
            text = "Alt+" + text;
        if (keys.HasFlag(Keys.Shift))
            text = "Shift+" + text;
        if (keys.HasFlag(Keys.Control))
            text = "Ctrl+" + text;
        return text;
    }

    // The question as typed: plain text, line breaks kept by the style sheet.
    public static string UserMessage(string prompt)
        => $"<div class=\"msg user\"><div class=\"bubble\">{WebUtility.HtmlEncode(prompt?.Trim() ?? "")}</div></div>";

    public static string AssistantMessage(AiChatTurn turn)
        => Assistant("", turn.ProviderAlias, MarkdownToHtml(turn.Answer), turn.UsageSummary);

    // An answer still arriving: the text received so far, or the typing indicator until the first of it.
    public static string PendingAnswer(string providerAlias, string answerSoFar)
        => Assistant("pending", providerAlias,
            string.IsNullOrWhiteSpace(answerSoFar) ? TypingIndicator : MarkdownToHtml(answerSoFar), "");

    // An answer cut by an error: whatever arrived of it, marked as not part of the conversation.
    public static string FailedAnswer(string providerAlias, string answerSoFar)
        => Assistant("failed", providerAlias, MarkdownToHtml(answerSoFar), "The request failed: this answer is not part of the conversation.");

    public static string MarkdownToHtml(string markdown)
        => string.IsNullOrEmpty(markdown) ? "" : Markdown.ToHtml(markdown, Pipeline);

    // A call to one of window.kntChat's functions with a text argument: serialized as JSON, which is a
    // JavaScript string literal with everything that could break out of it escaped.
    public static string ScriptCall(string function, string argument)
        => $"window.kntChat.{function}({JsonSerializer.Serialize(argument ?? "")});";

    private static string Assistant(string state, string providerAlias, string contentHtml, string footer)
    {
        var author = string.IsNullOrEmpty(providerAlias) ? "Assistant" : $"Assistant · {WebUtility.HtmlEncode(providerAlias)}";
        var meta = string.IsNullOrEmpty(footer) ? "" : $"<div class=\"meta\">{WebUtility.HtmlEncode(footer)}</div>";
        var classes = string.IsNullOrEmpty(state) ? "msg assistant" : $"msg assistant {state}";
        return $"<div class=\"{classes}\"><div class=\"author\">{author}</div><div class=\"content\">{contentHtml}</div>{meta}</div>";
    }

    private static string Style
    {
        get
        {
            if (_style == null)
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(StyleResourceName)
                    ?? throw new InvalidOperationException($"Embedded resource {StyleResourceName} not found.");
                using var reader = new StreamReader(stream);
                _style = reader.ReadToEnd();
            }
            return _style;
        }
    }
}
