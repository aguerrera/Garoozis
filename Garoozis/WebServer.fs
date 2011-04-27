module Garoozis.WebServer

open System
open System.IO
open System.Net
open System.Web.Hosting
open Garoozis
open Garoozis.Models


// start a simple webserver for your output folder
// need to start this as an Admin though, not sure about how to get around that.ss
let StartOutputHttpListener (port:int) (config:Config) = 
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
        | _ -> sw.Write("")
        sw.Flush()
        ctx.Response.Close()