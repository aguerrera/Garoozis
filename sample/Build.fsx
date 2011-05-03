
#I @"..\src\bin\debug"
#r "Garoozis.dll"

open System
open System.Collections.Generic
open System.IO
open Garoozis


let configSource = __SOURCE_DIRECTORY__ + @"\config.js"
let config = Garoozis.Utils.get_config(configSource)

Garoozis.Transformer.Build(config)

printfn "DONE building your site  Press any key to end."
Console.ReadKey()