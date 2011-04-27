module Garoozis.WebServer

open System
open System.IO
open System.Net
open System.Web.Hosting
open Garoozis
open Garoozis.Models

let errorTemplate = @"<html><head><title>problems...</title></head>
<body style='color:#fff;background-color:blue;font-family:consolas,system,lucida console,terminal;font-size:16px;'>
<div style='color:yellow;font-weight:bold;font-size:24px'>Error!!!!</div>
<div style='border:1px solid #c0c0c0;padding:10px;margin:10px;'>
$content
</div>
</body>
<html>"


// start a simple webserver for your output folder
// need to start this as an Admin though, not sure about how to get around that.ss
let StartOutputServer (port:int) (config:Config) = 
    let listener = new HttpListener()
    listener.AuthenticationSchemes <- AuthenticationSchemes.Anonymous
    listener.Prefixes.Add("http://+:" + port.ToString() + "/")
    listener.Start()
    while true do
        let ctx = listener.GetContext()
        let path = ctx.Request.Url.LocalPath.Substring(1)
        let query = ctx.Request.Url.Query.Replace("?", "")
        printfn "Received request for %s?%s" path query 
        use sw = new BinaryWriter(ctx.Response.OutputStream)
        try
            let pagepath = Path.Combine(config.OutputDir, path)
            let bytes = File.ReadAllBytes(pagepath)
            sw.Write(bytes)
        with 
        | :? System.Exception as ex ->
                                    printfn "error %s" ex.Message 
                                    let error = errorTemplate.Replace("$content", ex.ToString())
                                    let errorBytes = System.Text.Encoding.UTF8.GetBytes(error)
                                    sw.Write(errorBytes)
        sw.Flush()
        ctx.Response.Close()


let render_or_contents (path:string) (config:Config) = 
    let pagepath = Path.Combine(config.SourceDir, path)
    if File.Exists(pagepath) then
        let page = Transformer.get_page(pagepath)
        if page.Title = "" then
            File.ReadAllBytes(pagepath)
        else
            let r0 = Transformer.RenderPageForUrl path config 
            System.Text.Encoding.UTF8.GetBytes(r0)
    else
        let r = Transformer.RenderPageForUrl path config 
        System.Text.Encoding.UTF8.GetBytes(r)
    

// start a simple webserver for your output folder
// need to start this as an Admin though, not sure about how to get around that.ss
let StartDevServer (port:int) (config:Config) = 
    let listener = new HttpListener()
    listener.AuthenticationSchemes <- AuthenticationSchemes.Anonymous
    listener.Prefixes.Add("http://+:" + port.ToString() + "/")
    listener.Start()
    while true do
        let ctx = listener.GetContext()
        let path = ctx.Request.Url.LocalPath.Substring(1)
        let query = ctx.Request.Url.Query.Replace("?", "")
        printfn "Received request for %s?%s" path query 
        use sw = new BinaryWriter(ctx.Response.OutputStream)
        try
            let bytes = render_or_contents path config
            sw.Write(bytes)
        with 
        | :? System.Exception as ex ->
                                    printfn "error %s" ex.Message 
                                    let error = errorTemplate.Replace("$content", ex.ToString())
                                    let errorBytes = System.Text.Encoding.UTF8.GetBytes(error)
                                    sw.Write(errorBytes)
        sw.Flush()
        ctx.Response.Close()



 