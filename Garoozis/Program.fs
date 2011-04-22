open System
open System.IO
open System.Security.Cryptography
open System.Text
open RazorEngine


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


let md5 (data : byte array) : string =
    use md5 = MD5.Create()
    (StringBuilder(), md5.ComputeHash(data))
    ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
    |> string

//let hash = md5 "hello world"B;
let markdown = new MarkdownSharp.Markdown()
let md_to_html md = markdown.Transform(md)

let render_page (page:Page) (map:Map<string,string>) =
    let template = map.[page.Layout]
    let html = RazorEngine.Razor.Parse(template, page)
    html

let get_page filename = 
    let text = File.ReadAllText(filename)
    let jsonHeader = text.Substring(0, text.IndexOf("}") + 1)
    let page = Newtonsoft.Json.JsonConvert.DeserializeObject<Page>(jsonHeader)
    let contents = text.Substring(text.IndexOf("}") + 1)
    page.FileName <- filename
    match Path.GetExtension(filename) with
    | ".md" -> page.Content <- md_to_html(contents)
    | _ -> page.Content <- contents
    page


let base_dir = @"\Garoozis\sample"
let layouts = Directory.GetFiles(base_dir + @"\_layouts")
let posts = Directory.GetFiles(base_dir + @"\_posts")
let layout_map = 
    layouts
    |> Array.map (fun f -> (Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)  ))
    |> Map.ofArray

let post_data = posts |> Seq.map (fun f -> get_page(f)) |> Seq.toList
post_data 
|> List.iter (fun p -> 
                let rendered = render_page p layout_map
                printfn "%A" rendered )


(*
let template = "Hello @Model.Title! Welcome to Razor!";
let model = new Page ( Title = "World" )
let t = typeof<Page>
let result = RazorEngine.Razor.Parse<Page>(template, model)
printfn "results %s" result
*)

