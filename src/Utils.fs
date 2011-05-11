
module Garoozis.Utils

    open System.IO
    open System.Text
    open System.Security.Cryptography
    open MarkdownSharp

    // grab md5 has.  thanks, fsnippets.net
    let md5 (data : byte array) : string =
        use md5 = MD5.Create()
        (StringBuilder(), md5.ComputeHash(data))
        ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
        |> string
    //let hash = md5 "hello world"B;

    // text conversion
    let md_to_html md = 
        let markdown = new Markdown()
        markdown.Transform(md)

    // recursively get directories (thanks F# book!)
    let rec get_dirs basepath = 
        seq {
            yield! Directory.GetDirectories(basepath)
            for subdir in Directory.GetDirectories(basepath) do
                yield! get_dirs subdir
        }

    // recursively get files (thanks F# book!)
    let rec get_files basepath = 
        seq {
            yield! Directory.GetFiles(basepath)
            for subdir in Directory.GetDirectories(basepath) do
                yield! get_files subdir
        }

    // recursively delete all files and directories
    let delete_files_and_directories path = 
        let files = get_files path |> Seq.toList
        files |> List.filter (fun f -> f.IndexOf("garoozis.manifest.txt") = -1)  |> List.iter (fun f -> File.Delete(f))
        let dirs = get_dirs path |> Seq.sort |> Seq.toList |> List.rev
        dirs |> List.iter (fun d -> Directory.Delete(d))
        ()

    // copy a directory
    let rec copy_directory source dest = 
        let newdest = Path.Combine(dest, Path.GetFileName(source))
        if Directory.Exists(newdest) = false then Directory.CreateDirectory(newdest) |> ignore
        printfn "copying dir: %s TO %s" source newdest
        Directory.GetFiles(source) 
        |> Seq.filter (fun f -> f.EndsWith("~") = false)
        |> Seq.iter (fun f -> 
                        printfn "   File %s" f
                        File.Copy(f, Path.Combine(newdest, Path.GetFileName(f)), true ) 
                      )
        for subdir in Directory.GetDirectories(source) do
            copy_directory subdir newdest

    // hit the registry to get the contentType
    let get_contentType_from_registry (ext:string) = 
        let rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext.ToLower())
        try
            rk.GetValue("Content Type").ToString()
        with
        | _ -> "application/unknown"

    // get the content-type of a file
    let get_contentType (f:string) = 
        let ext = System.IO.Path.GetExtension(f).ToLower()
        match ext with
        | ".rss" -> "application/rss+xml"
        | _ -> get_contentType_from_registry ext

    // read a js file and deserialize to a Config object
    let get_config (path:string) = 
        let text = File.ReadAllText(path)
        let config = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Config>(text)
        config
