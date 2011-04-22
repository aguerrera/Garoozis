open System
open System.Collections.Generic
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions
open RazorEngine
open Newtonsoft.Json



// various configuration
let source_dir = Environment.CurrentDirectory + @"\..\..\..\sample"
let output_dir = @"C:\Staging\www\"
if Directory.Exists(output_dir) = false then
    Directory.CreateDirectory(output_dir) |> ignore


// each web page is represented as a Page
type Page() = 
    let mutable m_title = ""
    let mutable m_content = ""
    let mutable m_filename = ""
    let mutable m_layout = ""
    let mutable m_created = DateTime.MinValue
    let mutable m_url = ""
    member x.Title with get () = m_title
                        and set title = m_title <- title
    member x.Content with get () = m_content
                        and set content = m_content <- content
    member x.FileName with get () = m_filename
                        and set fileName = m_filename <- fileName
    member x.Layout with get () = m_layout
                        and set layout = m_layout <- layout
    member x.Url with get () = m_url
                        and set url = m_url <- url
    member x.Created with get () = m_created
                        and set created = m_created <- created

// the Model will represent the currently processed page as well as a list of all pages.
type Model() = 
    let mutable m_page = new Page()
    let mutable m_next = new Page()
    let mutable m_prev = new Page()
    let mutable m_pages = new List<Page>()
    member x.Page with get () = m_page
                        and set page = m_page <- page
    member x.Next with get () = m_next
                        and set next = m_next <- next
    member x.Prev with get () = m_prev
                        and set prev = m_prev <- prev
    member x.AllPages with get () = m_pages
    member x.Title with get () = m_page.Title
    member x.Content with get () = m_page.Content
    member x.FileName with get () = m_page.FileName
    member x.Layout with get () = m_page.Layout
    member x.Created with get () = m_page.Created


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

// this is what is used to render the page contents. 
// can switch rendering functions as needed.    
let render_page renderf (page:Page) (map:Map<string,string>) =
    let html = renderf page map
    html

// get the new file name, by looking at the format.  
// if it starts with yyyy-mm or yyyy-mm-dd, then those get changed 
// into a folder structure.
let get_url_from_filename fn = 
    let re1="(\\d+)-(\\d+)-(\\d+)-"
    let re2="(\\d+)-(\\d+)-"
    let mutable res = ""
    let m1 = Regex.Match(fn, re1, RegexOptions.IgnoreCase ||| RegexOptions.Singleline)
    if m1.Success = true then
        res <- fn.Replace(m1.Value, m1.Value.Replace("-","/"))
    else 
        let m2 = Regex.Match(fn, re2, RegexOptions.IgnoreCase ||| RegexOptions.Singleline)
        if m2.Success = true then
            res <- fn.Replace(m2.Value, m2.Value.Replace("-","/"))
        else
            res <- fn
    res.Replace(Path.GetExtension(res), ".html")

// this deserializes the json "front matter" and uses that for 
// additional configuration.
// returns an empty page if no f/m
let get_page_from_front_matter (text:string) = 
    try
        let jsonHeader = text.Substring(0, text.IndexOf("}") + 1)
        let page = JsonConvert.DeserializeObject<Page>(jsonHeader)
        page
    with
        _ -> new Page()

// get a 'Page' object from an actual file.
let get_page filename = 
    let text = File.ReadAllText(filename)
    let page = get_page_from_front_matter(text)
    let contents = text.Substring(text.IndexOf("}") + 1)
    page.FileName <- filename
    if page.Url = "" then page.Url <- get_url_from_filename(Path.GetFileName(filename))
    if page.Created = DateTime.MinValue then 
        let fi = new FileInfo(filename)
        page.Created <- fi.CreationTime
    match Path.GetExtension(filename) with
    | ".md" -> page.Content <- Utils.md_to_html(contents)
    | _ -> page.Content <- contents
    page

// write the transformed content to some file, whatever the output dir is
// this will create the folder hiearchy.
let write_output_file (url:string) (content:string) (outdir:string) = 
    let dout = Path.Combine(outdir, Path.GetDirectoryName(url))
    if Directory.Exists(dout) = false then Directory.CreateDirectory(dout) |> ignore
    let fout = Path.Combine(outdir, url)
    File.WriteAllText(fout, content)
    () 



// lets get down to brass tacks
let build_pages () =

    // clean up anything in the output director
    Utils.delete_files_and_directories(output_dir)


    // the .cshtml Razor templates are the'Layouts', in the vernacular
    // this is a map of the key:filename, value=actual razor template content.
    let layout_map = 
        Directory.GetFiles(source_dir + @"\_layouts")
        |> Array.map (fun f -> (Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)  ))
        |> Map.ofArray

    // get all files in the top level directory, or in the special _posts directory
    let pages = 
        Directory.GetFiles(source_dir + @"\_posts") 
        |> Array.append <| Directory.GetFiles(source_dir + @"\")

    // the actual renderer to be used.  you can use a fake_renderer to bypass
    // any issues with Razor
    let renderer = razor_renderer

    printfn "copying pages to output: %s" output_dir

    pages 
        |> Seq.map (fun f -> get_page(f)) 
        |> Seq.toList
        |> List.iter (fun p -> 
                    let rendered = render_page renderer p layout_map
                    let fn = Path.GetFileName(p.FileName)
                    printfn "   processing page: %s to %s" fn p.Url
                    write_output_file p.Url rendered output_dir                
                    )

    let page_count = Seq.length pages
    printfn "processed %i pages" page_count
    ()

build_pages()