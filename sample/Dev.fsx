
#I @"..\src\bin\debug"
#r "Garoozis.dll"

open System
open System.Collections.Generic
open System.IO
open Garoozis


let configSource = __SOURCE_DIRECTORY__ + @"\config.js"
let config = Garoozis.Utils.get_config(configSource)

let args = Environment.GetCommandLineArgs()

let is_static = args |> Array.exists (fun s -> s.ToLower() = "--static")

if is_static = true then
    printfn "STARTING static server www.guerrera.org."
    Garoozis.WebServer.Start 8088 config.OutputDir true
else
    printfn "STARTING dev server www.guerrera.org."
    Garoozis.WebServer.Start 8088 config.SourceDir false


