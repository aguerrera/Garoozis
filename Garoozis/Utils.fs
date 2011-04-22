
module TextUtils

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
    let markdown = new Markdown()
    let md_to_html md = markdown.Transform(md)
