module Garoozis.Transformer

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions
open RazorEngine
open Newtonsoft.Json
open Garoozis.Models


// render page with RazorEngine.  Can't send FSI types into this
// b/c it uses CodeDom to compile.  Thus and so, this executable.
let razor_renderer (model:Model) (map:Map<string,string>) =
    let template = map.[model.Page.Layout]
    let html = RazorEngine.Razor.Parse(template, model)
    html

// fake rendering function.  just does a replacement of model values
let fake_renderer (model:Model) (map:Map<string,string>) =
    let template = map.[model.Page.Layout]
                    .Replace("@Model.Title", model.Page.Title)
                    .Replace("@Model.Content", model.Page.Content)
    template

// this is what is used to render the page contents. 
// can switch rendering functions as needed.    
let render_page renderf (model:Model) (map:Map<string,string>) =
    let html = renderf model map
    html

// valid page name (no pages that begin with . or ~)
let is_valid_page_name (n:string) = 
    let valid = n.StartsWith(".") = false && n.IndexOf("~") = -1
    valid

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
        if page = null then
            new Page()
        else
            page
    with
        _ -> new Page()

// get a 'Page' object from an actual file.
let get_page filename = 
    let text = File.ReadAllText(filename)
    let page = get_page_from_front_matter(text)
    if String.IsNullOrWhiteSpace(page.Title) = false then
        let contents = text.Substring(text.IndexOf("}") + 1)
        page.FileName <- filename
        if page.Url = "" then page.Url <- get_url_from_filename(Path.GetFileName(filename))
        if page.Created = DateTime.MinValue then 
            let fi = new FileInfo(filename)
            page.Created <- fi.CreationTime
        if String.IsNullOrWhiteSpace(page.PubDate) = false then
            page.Created <- DateTime.Parse(page.PubDate)
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


// is this a post?  check the filename
let is_post (page:Page) = 
    page.FileName.IndexOf("_posts") <> -1

// get the next post.
let get_next_post (page:Page) (posts:IEnumerable<Page>) = 
    try
        let acc (p1:Page) (p2:Page) = 
            match p1.Created < p2.Created with
            | true -> p1
            | false -> p2
        let nextposts = posts |> Seq.filter (fun p -> p.Created > page.Created) 
        let head = nextposts |> Seq.head
        let pnext = nextposts |> Seq.fold acc head
        pnext
    with
    |_ -> new Page()

// get the prev post.
let get_prev_post (page:Page) (posts:IEnumerable<Page>) = 
    try
        let acc (p1:Page) (p2:Page) = 
            match p1.Created > p2.Created with
            | true -> p1
            | false -> p2
        let prevposts = posts |> Seq.filter (fun p -> p.Created < page.Created) 
        let head = prevposts |> Seq.head
        let pprev = prevposts |> Seq.fold acc head
        pprev
    with
    |_ -> new Page()

// build up model to pass to razor template.
// need to determine Next, Previous pages. As well as all Posts and non-Post pages.
let get_model_from_page (page:Page) (pageList:IEnumerable<Page>) = 
    let model = new Model()
    model.Page <- page
    let posts = new List<Page>() 
    posts.AddRange( pageList |> Seq.filter (fun p -> is_post(p) = true   )  )
    model.Posts <- posts
    let pages = new List<Page>() 
    pages.AddRange( pageList |> Seq.filter (fun p -> is_post(p) = false)  )
    model.Pages <- pages
    let fi = new FileInfo(page.FileName)
    if is_post(page) = true then
        model.NextPost <- get_next_post page posts
        model.PrevPost <- get_prev_post page posts
    model

// build an rss feed, and save it as feed.rss
let create_rss (posts:IEnumerable<Page>) (url:string) (title:string) (desc:string) (author:string) (output_dir:string) = 
    let feed = new Argotic.Syndication.RssFeed()
    feed.Channel.Link <- new Uri(url)
    feed.Channel.Title <- title
    feed.Channel.Description <- desc
    feed.Channel.Copyright <- author
    feed.Channel.TimeToLive <- 60
    feed.Channel.LastBuildDate <- DateTime.Now.ToUniversalTime()
    let sortedposts = posts |> Seq.toList |> List.sortWith (fun px py -> DateTime.Compare(py.Created, px.Created) )
    for p in sortedposts do
        let itemurl = new Uri(url + "/" + p.Url)
        let item = new Argotic.Syndication.RssItem()
        item.Guid <- new Argotic.Syndication.RssGuid(itemurl.ToString(),true)
        item.Title <- p.Title
        item.Link <- itemurl
        item.Author <- author
        item.Description <- p.Content
        feed.Channel.AddItem(item) |> ignore
    use stream = new FileStream(Path.Combine(output_dir,"rss.xml"), FileMode.Create, FileAccess.Write)
    feed.Save(stream)
    ()

// build map of files in layout directory
let get_layout_map source_dir = 
    Directory.GetFiles(source_dir + @"\_layouts")
    |> Array.map (fun f -> (Path.GetFileNameWithoutExtension(f), File.ReadAllText(f)  ))
    |> Map.ofArray


// optimize js and css output
let optimize_output (output_dir:string) = 
    let extfilter = [| ".js"; ".css" |]
    let files = 
        Utils.get_files(output_dir) 
        |> Seq.map (fun f -> new FileInfo(f))
        |> Seq.filter (fun f -> Array.exists (fun x-> x = f.Extension) <| extfilter)

    for fi in files do
        let text = File.ReadAllText(fi.FullName)
        let compressed = 
            match fi.Extension with
            | ".js" -> Yahoo.Yui.Compressor.JavaScriptCompressor.Compress(text)
            | ".css" -> Yahoo.Yui.Compressor.CssCompressor.Compress(text)
            | _ -> text
        File.WriteAllText(fi.FullName, compressed)
    ()


// get the rendered output for a url
let RenderPageForUrl (url:string) (source_dir:string) = 

    let layout_map = get_layout_map source_dir
    let posts = Directory.GetFiles(source_dir + @"\_posts") |> Array.filter ( fun p -> Path.GetFileName(p).StartsWith(".") = false) 
    let pages = Directory.GetFiles(source_dir + @"\") |> Array.filter ( fun p -> Path.GetFileName(p).StartsWith(".") = false)

    let files_to_transorm = pages |> Array.append <| posts 
    let pageModels = 
        files_to_transorm 
        |> Seq.map (fun f -> get_page(f)) 
        |> Seq.filter (fun p -> p.FileName <> "" && p.Ignore = false)
        |> Seq.toList

    // the actual renderer to be used.  you can use a fake_renderer to bypass
    // any issues with Razor
    let renderer = razor_renderer

    printfn "rendering page: %s" url

    let page = pageModels |> Seq.find (fun p -> p.Url.ToLower() = url.ToLower())

    let model = get_model_from_page page pageModels // gets a model with more details
    let rendered = render_page renderer model layout_map
    rendered

// main Build method.  this does the following:
// 1. deletes files from output dir
// 2. copies non-transformable files to output
// 3. transforms and copies files to output
let Build (config:Config) =

    let output_dir = config.OutputDir
    let source_dir = config.SourceDir

    let stopwatch = new System.Diagnostics.Stopwatch();
    stopwatch.Start();

    // setup output folder
    if Directory.Exists(output_dir) = false then
        Directory.CreateDirectory(output_dir) |> ignore

    // clean up anything in the output director
    printfn "deleting content from output dir: %s" output_dir
    Garoozis.Utils.delete_files_and_directories(output_dir)

    printfn "copying pages to output: %s" output_dir

    // copy transformed files to output folder
    Directory.GetDirectories(source_dir) 
        |> Seq.map (fun d -> new DirectoryInfo(d))
        |> Seq.filter (fun d -> d.Name.StartsWith("_") = false && d.Name.StartsWith(".") = false)
        |> Seq.iter (fun d ->
                            printfn "copying dir: %s" d.Name
                            Utils.copy_directory d.FullName output_dir
                            )

    // the .cshtml Razor templates are the'Layouts', in the vernacular
    // this is a map of the key:filename, value=actual razor template content.
    let layout_map = get_layout_map source_dir

    // get all files in the top level directory, or in the special _posts directory
    let posts = Directory.GetFiles(source_dir + @"\_posts") |> Array.filter ( fun p -> 
                                                                                       let pn = System.IO.Path.GetFileName(p)
                                                                                       is_valid_page_name(pn) = true ) 
    let pages = Directory.GetFiles(source_dir + @"\") |> Array.filter ( fun p -> 
                                                                                let pn = System.IO.Path.GetFileName(p)
                                                                                pn.StartsWith(".") = false && pn.EndsWith("~") = false)

    let files_to_transorm = pages |> Array.append <| posts 


    // build list of page models
    let pageModels = 
        files_to_transorm 
        |> Seq.map (fun f -> get_page(f)) 
        |> Seq.filter (fun p -> p.FileName <> "")
        |> Seq.toList

    // the actual renderer to be used.  you can use a fake_renderer to bypass
    // any issues with Razor
    let renderer = razor_renderer

    printfn "transforming content and writing to output dir"

    // cycle through page models and render the html using each model
    pageModels
        |> Seq.iter (fun p -> 
                        let model = get_model_from_page p pageModels // gets a model with more details
                        let rendered = render_page renderer model layout_map
                        let fn = Path.GetFileName(p.FileName)
                        printfn "   processing page: %s to %s" fn p.Url
                        write_output_file p.Url rendered output_dir                
                    )



    printfn "processed %i pages" <| Seq.length pages

    // compress js and css using YUI
    if config.CompressOutput = true then
        printfn "optimizing css and js output"
        optimize_output output_dir

    // create a blog
    if String.IsNullOrEmpty(config.Url) = false then
        printfn "creating rss"
        let posts = pageModels |> Seq.filter (fun p -> is_post(p) = true   ) 
        create_rss posts config.Url config.BlogTitle config.BlogDesc config.BlogAuthor config.OutputDir


    stopwatch.Stop()
    let elapsed = stopwatch.Elapsed.ToString();
    printfn "ellapsed %s" elapsed
    printfn "done!"


    ()

