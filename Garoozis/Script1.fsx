#r @"System.Web.Razor.dll"
#r @"..\packages\MarkdownSharp.1.13.0.0\lib\35\MarkdownSharp.dll"
#r @"..\packages\Newtonsoft.Json.4.0.2\lib\net40-full\Newtonsoft.Json.dll"
#r @"..\packages\RazorEngine.2.1\lib\.NetFramework 4.0\RazorEngine.dll"
#r @"..\packages\LitS3.1.0.1\lib\LitS3.dll"


#r "bin\debug\Garoozis.dll"

(*
#load "Models.fs"
#load "Utils.fs"
#load "Transformer.fs"
*)

open System
open System.Collections.Generic
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions
open RazorEngine
open Newtonsoft.Json
open LitS3
open System.Linq

(*
http://docs.amazonwebservices.com/AmazonS3/latest/dev/index.html?WebsiteHosting.html
*)

let configSource = __SOURCE_DIRECTORY__  + @"\test.js"
let config = Garoozis.Utils.get_config(configSource)
Garoozis.Transformer.Build config.OutputDir config.SourceDir
Garoozis.RemoteStorage.PublishToS3(config)

(*
type GG() = 
    let mutable m_title = ""
    let mutable m_created = DateTime.MinValue
    member x.Title with get () = m_title
                        and set title = m_title <- title
    member x.Created with get () = m_created
                        and set created = m_created <- created

let ggs = [
             new GG(Title="title 1", Created=DateTime.Parse("4/20/2011 8:00 AM") );
             new GG(Title="title 2", Created=DateTime.Parse("4/21/2011 8:00 AM") );
             new GG(Title="title 3", Created=DateTime.Parse("4/22/2011 8:00 AM") )
          ]

let gg1 = new GG(Title="title 1", Created=DateTime.Parse("4/20/2011 8:00 AM") );
let gg2 = new GG(Title="title 2", Created=DateTime.Parse("4/21/2011 8:00 AM") );
let gg3 = new GG(Title="title 3", Created=DateTime.Parse("4/22/2011 8:00 AM") )


let acc_next (g1:GG) (g2:GG) = 
    match g1.Created > g2.Created with
    | true -> g1
    | false -> g2

let acc_prev (g1:GG) (g2:GG) = 
    match g1.Created < g2.Created with
    | true -> g1
    | false -> g2

ggs |> List.fold acc_prev gg1

acc_prev gg2 gg3
acc_next gg2 gg1
*)
