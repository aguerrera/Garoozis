module Garoozis.Main

open System
open System.Collections.Generic
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions
open RazorEngine
open Newtonsoft.Json
open Garoozis.Models


// various configuration
let source_dir = Environment.CurrentDirectory + @"\..\..\..\sample"
let output_dir = @"C:\Staging\www\"

// read app.js, a new type of configuration.  
// dont' like app.config, and yaml ain't doing it for me.
let config_text = File.ReadAllText(Environment.CurrentDirectory + @"\app.js")
let config = JsonConvert.DeserializeObject<Config>(config_text)

Transformer.build_pages output_dir source_dir
