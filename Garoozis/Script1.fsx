#r @"System.Web.Razor.dll"
#r @"Microsoft.CSharp.dll"
#r @"..\packages\MarkdownSharp.1.13.0.0\lib\35\MarkdownSharp.dll"
#r @"..\packages\Newtonsoft.Json.4.0.2\lib\net40-full\Newtonsoft.Json.dll"

open System
open System.IO

// this Page will be the model
type Page() = 
    let mutable m_title = ""
    let mutable m_content = ""
    let mutable m_filename = ""
    let mutable m_layout = ""
    member x.Title with get () = m_title
                        and set title = m_title <- title
    member x.Content with get () = m_content
                        and set content = m_content <- content
    member x.FileName with get () = m_filename
                        and set fileName = m_filename <- m_filename
    member x.Layout with get () = m_layout
                        and set layout = m_layout <- layout


let markdown = new MarkdownSharp.Markdown()
let md_to_html md = markdown.Transform(md)

let base_dir = @"C:\Source\IONChamp\Garoozis\sample"
let layouts = Directory.GetFiles(base_dir + @"\_layouts")
let posts = Directory.GetFiles(base_dir + @"\_posts")
let layout_map = 
    layouts
    |> Array.map (fun f -> (Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)  ))
    |> Map.ofArray

let getHeader filename = 
    let text = File.ReadAllText(filename)
    let jsonHeader = text.Substring(0, text.IndexOf("}") + 1)
    let page = Newtonsoft.Json.JsonConvert.DeserializeObject<Page>(jsonHeader)
    let contents = text.Substring(text.IndexOf("}") + 1)
    page.FileName <- filename
    match Path.GetExtension(filename) with
    | ".md" -> page.Content <- md_to_html(contents)
    | _ -> page.Content <- contents
    page

let post_data = posts |> Seq.map (fun f -> getHeader(f)) |> Seq.toList
post_data 
|> List.iter (fun p -> 
                printfn "%A" layout_map.[p.Layout] )
