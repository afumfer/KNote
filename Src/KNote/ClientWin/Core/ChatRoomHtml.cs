using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using KNote.Model.Dto;
using Markdig;

namespace KNote.ClientWin.Core;

// HTML of the conversation of a chat room thread (KntChatForm): the page with the thread's messages, loaded once, and
// the message its script (window.kntChatRoom) adds when a new one arrives (see Views/ChatRoomWebRenderer). The same
// bubbles as the Web chat room: the user's own messages on the right, the others' on the left with their name. Pure
// functions: no WebView2 here.
public static class ChatRoomHtml
{
    private const string StyleResourceName = "KNote.ClientWin.Resources.KNoteChatRoom.css";

    // Raw HTML in a message is shown as text: other users write it, and the page runs a script.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    private static string _style;

    // append(html): adds a message at the end and scrolls to it, so what comes in is always seen. Links other than
    // web, mail and in-page ones do nothing; the host opens web and mail links outside the view, and an image of a
    // message clicked (going to its url, which the host opens in the browser too). The host's
    // shortcuts (window.kntChatShortcuts, written as AiChatHtml.ShortcutText does) are posted to it: while the page
    // has the focus, the keys reach the browser, not the window's menu.
    private const string Script = """
        window.kntChatRoom = (() => {
          const chat = document.getElementById('chat');
          const toBottom = () => window.scrollTo(0, document.documentElement.scrollHeight);
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
            const image = e.target.closest('.bubble img');
            if (image && /^https?:$/.test(new URL(image.src).protocol)) {
              window.location.href = image.src;
              return;
            }
            const a = e.target.closest('a[href]');
            if (a && !a.getAttribute('href').startsWith('#') && !/^(https?|mailto):$/.test(a.protocol))
              e.preventDefault();
          });
          // An image loaded after the message was added makes it taller: still at the end.
          chat.addEventListener('load', e => { if (e.target.tagName === 'IMG') toBottom(); }, true);
          toBottom();
          return {
            append(html) {
              chat.insertAdjacentHTML('beforeend', html);
              toBottom();
            }
          };
        })();
        """;

    // messages: the thread's, in order. userName: the user of the app (their messages go on the right).
    // markdownStyle: the <style> element of the app's Markdown viewer (Store.KNoteWebViewStyle), for the messages'
    // text; the chat layout goes on top of it. emptyText: what the page says when there are no messages.
    // hostShortcuts: the keys the page passes on to the host. serverBaseUrl: the base of the chat room's Server, which
    // the urls of its resources (the images of the messages) may be relative to.
    public static string Page(IEnumerable<ChatRoomMessageDto> messages, string userName, string markdownStyle,
        string emptyText, IEnumerable<string> hostShortcuts = null, Uri serverBaseUrl = null)
    {
        var html = new StringBuilder();
        html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        if (serverBaseUrl != null)
            html.Append("<base href=\"").Append(WebUtility.HtmlEncode(serverBaseUrl.AbsoluteUri)).Append("\">");
        html.Append(markdownStyle);
        html.Append("<style>").Append(Style).Append("</style></head><body>");
        html.Append("<main id=\"chat\" data-empty=\"").Append(WebUtility.HtmlEncode(emptyText ?? "")).Append("\">");
        DateTime? previous = null;
        foreach (var message in messages)
        {
            html.Append(Message(message, userName, previous));
            previous = message.SentDateTime;
        }
        html.Append("</main><script>");
        html.Append("window.kntChatShortcuts = ").Append(JsonSerializer.Serialize(hostShortcuts ?? Array.Empty<string>())).Append(';');
        html.Append(Script).Append("</script></body></html>");
        return html.ToString();
    }

    // One message, with a date separator before it when it is the first of its day (previous: the one before it).
    public static string Message(ChatRoomMessageDto message, string userName, DateTime? previous)
    {
        var own = string.Equals(message.UserName, userName, StringComparison.OrdinalIgnoreCase);
        var name = WebUtility.HtmlEncode(message.UserName ?? "");
        var separator = previous?.Date == message.SentDateTime.Date
            ? ""
            : $"<div class=\"day\"><span>{WebUtility.HtmlEncode(message.SentDateTime.ToString("D"))}</span></div>";
        var author = own ? "" : $"<span class=\"name\">{name}</span>";

        return $"{separator}<div class=\"msg {(own ? "own" : "other")}\">" +
            $"<div class=\"avatar\" title=\"{name}\">{WebUtility.HtmlEncode(Initials(message.UserName))}</div>" +
            "<div class=\"body\">" +
            $"<div class=\"info\">{author}<span class=\"time\">{message.SentDateTime:HH:mm}</span></div>" +
            $"<div class=\"bubble\">{MarkdownToHtml(message.Text)}</div>" +
            "</div></div>";
    }

    public static string MarkdownToHtml(string markdown)
        => string.IsNullOrEmpty(markdown) ? "" : Markdown.ToHtml(markdown, Pipeline);

    // A call to one of window.kntChatRoom's functions with a text argument: serialized as JSON, which is a JavaScript
    // string literal with everything that could break out of it escaped.
    public static string ScriptCall(string function, string argument)
        => $"window.kntChatRoom.{function}({JsonSerializer.Serialize(argument ?? "")});";

    // "maria" → "MA", "Ana López" → "AL".
    public static string Initials(string userName)
    {
        var parts = (userName ?? "").Split(new[] { ' ', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant(),
            _ => $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
        };
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
