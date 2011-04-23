﻿#r @"System.Web.Razor.dll"
#r @"..\packages\MarkdownSharp.1.13.0.0\lib\35\MarkdownSharp.dll"
#r @"..\packages\Newtonsoft.Json.4.0.2\lib\net40-full\Newtonsoft.Json.dll"
#r @"..\packages\RazorEngine.2.1\lib\.NetFramework 4.0\RazorEngine.dll"
#load "Models.fs"
#load "Utils.fs"
#load "Transformer.fs"

open System
open System.Collections.Generic
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions
open RazorEngine
open Newtonsoft.Json

let source_dir = @"c:\source\Garoozis\sample"
let output_dir = @"C:\Staging\www\"

Garoozis.Transformer.build_pages output_dir source_dir
