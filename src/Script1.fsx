#r @"System.Web.Razor.dll"
#r @"..\packages\MarkdownSharp.1.13.0.0\lib\35\MarkdownSharp.dll"
#r @"..\packages\Newtonsoft.Json.4.0.2\lib\net40-full\Newtonsoft.Json.dll"
#r @"..\packages\RazorEngine.2.1\lib\.NetFramework 4.0\RazorEngine.dll"
#r @"..\packages\LitS3.1.0.1\lib\LitS3.dll"
#r @"..\packages\YUICompressor.NET.1.5.0.0\lib\NET35\Yahoo.Yui.Compressor.dll"
#r @"..\packages\YUICompressor.NET.1.5.0.0\lib\NET35\EcmaScript.NET.modified.dll"
#r "bin\debug\Garoozis.dll"

open System
open System.Collections.Generic
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions

let configSource = __SOURCE_DIRECTORY__  + @"\test.js"
let config = Garoozis.Utils.get_config(configSource)

let get_date1 () = 
    DateTime.Now

let get_date2 () = 
    DateTime.Now.AddMinutes(2.0)

let is_now f = 
    let d = f()
    printfn "%A" d
    DateTime.Now = f()

is_now get_date1

let fs = Garoozis.Utils.get_files config.SourceDir

fs |> Seq.toList


Garoozis.Transformer.Build(config)
Garoozis.RemoteStorage.PublishToS3(config)
//Garoozis.WebServer.Start 8085 config.SourceDir false
printfn "source: %s" configSource
printfn "outputdir: %s" config.OutputDir




