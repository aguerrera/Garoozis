open System
open System.IO
open System.Security.Cryptography
open System.Text
open RazorEngine
open Newtonsoft.Json

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
                        and set fileName = m_filename <- fileName
    member x.Layout with get () = m_layout
                        and set layout = m_layout <- layout

// various configuration
let source_dir = Environment.CurrentDirectory + @"\..\..\..\sample"
let output_dir = @"C:\Staging\www\"
if Directory.Exists(output_dir) = false then
    Directory.CreateDirectory(output_dir) |> ignore


// text conversion
let markdown = new MarkdownSharp.Markdown()
let md_to_html md = markdown.Transform(md)

// render page with RazorEngine.  Can't send FSI types into this
// b/c it uses CodeDom to compile.  Thus and so, this executable.
let razor_renderer (page:Page) (map:Map<string,string>) =
    let template = map.[page.Layout]
    let html = RazorEngine.Razor.Parse(template, page)
    html
// fake rendering function.  just does a replacement of model values
let fake_renderer (page:Page) (map:Map<string,string>) =
    let template = map.[page.Layout]
                    .Replace("@Model.Title", page.Title)
                    .Replace("@Model.Content", page.Content)
    template
    
let render_page renderf (page:Page) (map:Map<string,string>) =
    let html = renderf page map
    html

let get_page filename = 
    let text = File.ReadAllText(filename)
    let jsonHeader = text.Substring(0, text.IndexOf("}") + 1)
    let page = JsonConvert.DeserializeObject<Page>(jsonHeader)
    let contents = text.Substring(text.IndexOf("}") + 1)
    page.FileName <- filename
    match Path.GetExtension(filename) with
    | ".md" -> page.Content <- TextUtils.md_to_html(contents)
    | _ -> page.Content <- contents
    page


let layouts = Directory.GetFiles(source_dir + @"\_layouts")

let pages = 
    Directory.GetFiles(source_dir + @"\_posts") 
    |> Array.append <| Directory.GetFiles(source_dir + @"\")

// this is a map of the key:filename, value=actual razor template content.
let layout_map = 
    layouts
    |> Array.map (fun f -> (Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)  ))
    |> Map.ofArray

let write_output_file (filename:string) (content:string) (outdir:string) = 
    let fn = Path.GetFileNameWithoutExtension(filename) + ".html"
    let fout = Path.Combine(outdir, fn)
    File.WriteAllText(fout, content)
    () 

let renderer = razor_renderer
let post_data = pages |> Seq.map (fun f -> get_page(f)) |> Seq.toList
post_data 
|> List.iter (fun p -> 
                let rendered = render_page renderer p layout_map
                printfn "processing page: %s" p.FileName
                write_output_file p.FileName rendered output_dir                
                )

