module Garoozis.WebServer

open System
open System.Collections.Generic
open System.IO
open System.Net
open System.Text
open System.Web.Hosting
open Garoozis
open Garoozis.Models

let errorTemplate = @"<html><head><title>problems...</title></head>
<body style='color:#fff;background-color:blue;font-family:consolas,system,lucida console,terminal;font-size:16px;'>
<div style='color:red;font-weight:bold;font-size:24px'>Error!!!!</div>
<div style='border:1px solid #c0c0c0;padding:10px;margin:10px;'>
<p style='font-weight:bold;'>$message</p>
$content
</div>

<div style='color:yellow;font-weight:bold;font-size:24px'>Available Urls</div>
<div style='border:1px solid #c0c0c0;padding:10px;margin:10px;'>
<ul>
$urls
</ul>
</div>
</body>
</html>"


let system_default_doc_name = "index.html"

let get_url_list www_dir = 
    let files = Utils.get_files(www_dir) |> Seq.filter (fun f -> Transformer.is_valid_page_name(f) = true )
    let urlify (f:string) = 
        if f.IndexOf("_posts") <> -1 then
            Transformer.get_url_from_filename(f).Replace(www_dir, "").Replace("\\", "/").Substring(1).Replace("_posts/", "")
        else 
            f.Replace(www_dir, "").Replace("\\", "/").Replace(".md",".html").Substring(1)

    files |> Seq.filter (fun f -> f.IndexOf("_layouts") = -1 ) |> Seq.map (fun f -> urlify(f)) |> Seq.toList

// what is the default document for this site?
// defaults to index.html
let get_default_doc (urls:IEnumerable<string>) = 
    let defs = urls |> Seq.filter ( fun f -> f.StartsWith("index.") || f.StartsWith("default.") ) |> Seq.toList
    let htmls = defs |> List.filter (fun s -> s.EndsWith(".html") || s.EndsWith(".htm") )
    let has_sys_default = defs |> List.exists (fun s -> s.ToLower() = system_default_doc_name)
    if has_sys_default then
        system_default_doc_name
    elif List.length htmls > 0 then
        List.head htmls
    else
        system_default_doc_name

// this gets a list of pages for the error screen
let get_html_list (xs:IEnumerable<string>) = 
    let sb = new StringBuilder()
    for s in xs do
        sb.Append("<li><a style='color:white' href='/") |> ignore
        sb.Append(s) |> ignore
        sb.Append("'>") |> ignore
        sb.Append(s) |> ignore
        sb.Append("</a></li>") |> ignore
    sb.ToString()

// simple function to read static pages
let render_static (path:string) (www_path:string) = 
    let pagepath = Path.Combine(www_path, path)
    File.ReadAllBytes(pagepath)

// function reads static pages, or will build the model
// to run it through Razor
let render_or_contents (path:string) (www_path:string) = 
    let pagepath = Path.Combine(www_path, path)
    if File.Exists(pagepath) then
        let page = Transformer.get_page(pagepath)
        if page.Title = "" then
            File.ReadAllBytes(pagepath)
        else
            let r0 = Transformer.RenderPageForUrl path www_path 
            System.Text.Encoding.UTF8.GetBytes(r0)
    else
        let r = Transformer.RenderPageForUrl path www_path
        System.Text.Encoding.UTF8.GetBytes(r)


let get_error_bytes message content urls = 
    let error = errorTemplate.Replace("$message", message).Replace("$content", content).Replace("$urls", get_html_list(urls))
    System.Text.Encoding.UTF8.GetBytes(error)


// main httplistener loop
// this is the actual server
let run_httplistener (port:int) (www_dir:string) (renderer:string->string->byte[]) = 
    let listener = new HttpListener()
    listener.AuthenticationSchemes <- AuthenticationSchemes.Anonymous
    listener.Prefixes.Add("http://localhost:" + port.ToString() + "/")
    listener.Prefixes.Add("http://127.0.0.1:" + port.ToString() + "/")
    //listener.Prefixes.Add("http://+:" + port.ToString() + "/")
    listener.Start()

    printfn "starting server port %i for %s" port www_dir
    printfn ""

    let urls = get_url_list(www_dir)
    let default_document = get_default_doc(urls)

    printfn "default document: %s" default_document

    while true do
        let ctx = listener.GetContext()
        let mutable path = ctx.Request.Url.LocalPath.Substring(1)
        if path = "" then
            path <- default_document
        let query = ctx.Request.Url.Query.Replace("?", "")
        if path <> "favicon.ico" then
            printfn " request:  %s " path
        use sw = new BinaryWriter(ctx.Response.OutputStream)
        try
            if path <> "favicon.ico" then
                let bytes = renderer path www_dir
                sw.Write(bytes)
        with 
        | :? System.IO.FileNotFoundException as ex ->
                                    printfn "error %s" ex.Message 
                                    let errorBytes = get_error_bytes "System.IO.FileNotFound" ex.Message urls
                                    sw.Write(errorBytes)
        | :? System.Exception as ex ->
                                    printfn "error %s" ex.Message 
                                    let errorBytes = get_error_bytes "System.Exception" ex.Message urls
                                    sw.Write(errorBytes)
        sw.Flush()
        ctx.Response.Close()

// main start method.
// need to start this as an Admin though, not sure about how to get around that.ss
let Start (port:int) (www_dir:string) (is_static:bool) = 
    if is_static = false then
        run_httplistener port www_dir render_or_contents
    else
        run_httplistener port www_dir render_static

