
module Utils

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
        files |> List.iter (fun f -> File.Delete(f))
        let dirs = get_dirs path |> Seq.sort |> Seq.toList |> List.rev
        dirs |> List.iter (fun d -> Directory.Delete(d))
        ()
