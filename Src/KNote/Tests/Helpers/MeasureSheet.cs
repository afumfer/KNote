using System.Net;
using System.Text;
using System.Text.Json;

namespace KNote.Tests.Helpers;

/// <summary>
/// A page to rate the items of a measure in the owner's browser (as the sheets of Stage4Measure in ClientWin.Tests): one choice and a
/// comment per item, counted at the top, kept in the browser (localStorage, if it can) and «Copy the results» puts them in the
/// clipboard as text. It holds what the notes say: it goes to the folder of the results, out of the repository.
/// </summary>
public static class MeasureSheet
{
    public sealed record Item(string Title, string Text);

    public static void Write(string path, string title, string summary, string repository, string[] options, IReadOnlyList<Item> items)
    {
        static string H(string? text) => WebUtility.HtmlEncode(text ?? "");
        var html = new StringBuilder();
        html.Append($"<!doctype html><html><head><meta charset=\"utf-8\"><title>{H(title)}</title><style>")
            .Append("body{font-family:Segoe UI,sans-serif;margin:1.5rem;background:#fafafa;color:#222}")
            .Append(".item{background:#fff;border:1px solid #ddd;border-radius:6px;padding:1rem;margin:1rem 0}")
            .Append(".text{white-space:pre-wrap;max-height:40rem;overflow:auto}.title{font-weight:600;margin-bottom:.5rem}")
            .Append(".choices{margin-top:.75rem}.choices label{margin-right:1rem}#counts{position:sticky;top:0;background:#fafafa;padding:.5rem 0;font-weight:600}")
            .Append("</style></head><body>");
        html.Append($"<h1>{H(title)}</h1><p>Repository: {H(repository)} · {DateTime.Now:yyyy-MM-dd HH:mm}. {H(summary)}</p><div id=\"counts\"></div><button id=\"copy\">Copy the results</button>");
        for (var i = 0; i < items.Count; i++)
        {
            html.Append($"<div class=\"item\"><div class=\"title\">{i + 1}. {H(items[i].Title)}</div><div class=\"text\">{H(items[i].Text)}</div><div class=\"choices\">");
            foreach (var option in options)
                html.Append($"<label><input type=\"radio\" name=\"i{i}\" value=\"{H(option)}\"> {H(option)}</label>");
            html.Append($"<input type=\"text\" data-note=\"{i}\" placeholder=\"Comment\" size=\"50\"></div></div>");
        }
        html.Append("<script>")
            .Append($"const options={JsonSerializer.Serialize(options)},count={items.Count},key='knote-'+location.pathname;")
            .Append("function load(){try{return JSON.parse(localStorage.getItem(key)||'{}')}catch(e){return {}}}")
            .Append("function save(s){try{localStorage.setItem(key,JSON.stringify(s))}catch(e){}}")
            .Append("function state(){const s={};for(let i=0;i<count;i++){const c=document.querySelector(`input[name=i${i}]:checked`);")
            .Append("const n=document.querySelector(`input[data-note=\"${i}\"]`).value;s[i]={choice:c?c.value:null,note:n}}return s}")
            .Append("function counts(){const s=state();const t=options.map(o=>`${o}: ${Object.values(s).filter(x=>x.choice===o).length}`);")
            .Append("const left=Object.values(s).filter(x=>!x.choice).length;document.getElementById('counts').textContent=t.join(' · ')+` · not rated: ${left}`;save(s)}")
            .Append("const saved=load();for(const [i,v] of Object.entries(saved)){if(v.choice){const r=document.querySelector(`input[name=i${i}][value=\"${v.choice}\"]`);if(r)r.checked=true}")
            .Append("const n=document.querySelector(`input[data-note=\"${i}\"]`);if(n)n.value=v.note||''}")
            .Append("document.addEventListener('input',counts);counts();")
            .Append("document.getElementById('copy').onclick=()=>{const s=state();const lines=[document.getElementById('counts').textContent];")
            .Append("for(let i=0;i<count;i++)lines.push(`${i+1}. ${s[i].choice||'-'}${s[i].note?' · '+s[i].note:''}`);navigator.clipboard.writeText(lines.join('\\n'))};")
            .Append("</script></body></html>");
        File.WriteAllText(path, html.ToString());
    }
}
