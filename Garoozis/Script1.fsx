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

(*
http://docs.amazonwebservices.com/AmazonS3/latest/dev/index.html?WebsiteHosting.html
*)
let source_dir = @"c:\source\Garoozis\sample"
let output_dir = @"C:\Staging\www\"
let bucketName = "blog.guerrera.org"

Garoozis.Transformer.build_pages output_dir source_dir

let accessKeyID = ""
let secretAccessKeyID = ""

let service = new S3Service(AccessKeyID = accessKeyID, SecretAccessKey = secretAccessKeyID)
(*
try
    service.CreateBucket(bucketName)
finally
    ()
*)


let get_url (f:string) = 
        f.ToLower().Replace(output_dir.ToLower(), "").Replace("\\", "/")

let get_contentType (f:string) = 
    let regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(System.IO.Path.GetExtension(f).ToLower())
    try
        regKey.GetValue("Content Type").ToString()
    with
    | _ -> "application/unknown"

let files = Garoozis.Utils.get_files(output_dir) 
            |> Seq.toList

files 
    |> List.map (fun f -> (f, get_url(f), get_contentType(f)) )
    |> List.iter (fun (f,u,c) -> service.AddObject(f, bucketName, u, c, LitS3.CannedAcl.PublicRead) )

