#r @"System.Web.Razor.dll"
#r @"..\packages\MarkdownSharp.1.13.0.0\lib\35\MarkdownSharp.dll"
#r @"..\packages\Newtonsoft.Json.4.0.2\lib\net40-full\Newtonsoft.Json.dll"
#r @"..\packages\RazorEngine.2.1\lib\.NetFramework 4.0\RazorEngine.dll"
#r @"..\packages\LitS3.1.0.1\lib\LitS3.dll"
#r @"..\packages\FSPowerPack.Community.2.0.0.0\Lib\Net40\FSharp.PowerPack.dll"
#r @"..\packages\FSPowerPack.Community.2.0.0.0\Lib\Net40\FSharp.PowerPack.Linq.dll"


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
open Microsoft.FSharp.Quotations

(*
http://docs.amazonwebservices.com/AmazonS3/latest/dev/index.html?WebsiteHosting.html
*)

let configSource = __SOURCE_DIRECTORY__  + @"\test.js"
let config = Garoozis.Utils.get_config(configSource)
Garoozis.Transformer.build_pages config.OutputDir config.SourceDir
Garoozis.RemoteStorage.publish_to_s3(config)

