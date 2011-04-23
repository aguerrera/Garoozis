module Garoozis.Transformer

open System
open System.Collections.Generic
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions
open RazorEngine
open Newtonsoft.Json
open Garoozis.Models


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
    | ".md" -> page.Content <- Garoozis.Utils.md_to_html(contents)
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
let build_pages output_dir source_dir =

    // setup output folder
    if Directory.Exists(output_dir) = false then
        Directory.CreateDirectory(output_dir) |> ignore

    // clean up anything in the output director
    Garoozis.Utils.delete_files_and_directories(output_dir)


    // the .cshtml Razor templates are the'Layouts', in the vernacular
    // this is a map of the key:filename, value=actual razor template content.
    let layout_map = 
        Directory.GetFiles(source_dir + @"\_layouts")
        |> Array.map (fun f -> (Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)  ))
        |> Map.ofArray

    // get all files in the top level directory, or in the special _posts directory
    let posts = Directory.GetFiles(source_dir + @"\_posts") 
    let pages = Directory.GetFiles(source_dir + @"\")

    let files_to_transorm = pages |> Array.append <| posts 

    // the actual renderer to be used.  you can use a fake_renderer to bypass
    // any issues with Razor
    let renderer = razor_renderer

    printfn "copying pages to output: %s" output_dir

    Directory.GetDirectories(source_dir) 
        |> Seq.map (fun d -> new DirectoryInfo(d))
        |> Seq.filter (fun d -> d.Name.StartsWith("_") = false )
        |> Seq.iter (fun d ->
                            printfn "copying dir: %s" d.Name
                            Garoozis.Utils.copy_directory d.FullName output_dir
                            )
                                
    files_to_transorm 
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

