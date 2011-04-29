module Garoozis.RemoteStorage

open System
open System.Collections.Generic
open System.IO
open LitS3
open Garoozis.Models

// this takes a file name and turns it into a usable url
let get_url (f:string, output_dir:string)  = 
        let url = f.ToLower().Replace(output_dir.ToLower(), "").Replace("\\", "/")
        match url.StartsWith("/") with
        | true  -> url.Substring(1)
        | _     -> url

// get items from s3 bucket
let get_items_from_s3 (service:S3Service) (bucketName:string) = 
    let items = service.ListAllObjects(bucketName)
    items

// delete all items from bucket
let delete_all_items_from_s3 (service:S3Service) (bucketName:string) = 
    let items = get_items_from_s3 service bucketName
    for item in items do
        printfn "   deleting item %s" item.Name
        service.DeleteObject(bucketName,item.Name)

// gets manifest
let get_manifest output_dir = 
    let dict = new System.Collections.Generic.Dictionary<string,string>()
    let manifest_file = Path.Combine(output_dir, "garoozis.manifest.txt")
    if File.Exists( manifest_file) = true then
        let lines = File.ReadAllLines(manifest_file)
        for line in lines do
            let arr = line.Split(',')
            dict.[arr.[0]] <- arr.[1]
    dict

// write the current file/hash manifest
let write_manifest (output_dir:string) (dict:Dictionary<string,string>) = 
    let manifest_file = Path.Combine(output_dir, "garoozis.manifest.txt")
    let arr = 
        dict
        |> Seq.map ( fun d -> d.Key + "," + d.Value )
        |> Seq.toArray
    File.WriteAllLines(manifest_file, arr)
    ()

// delete items from your bucket then republish all
let PublishToS3 (config:Config)= 
    let service = new LitS3.S3Service(AccessKeyID = config.StorageKey, SecretAccessKey = config.StoragePass)
    let output_dir = config.OutputDir
    let bucketName = config.StorageContainer

    if config.DeleteExistingStorage = true then
        printfn "S3: deleting items from bucket %s" bucketName
        delete_all_items_from_s3 service bucketName |> ignore

    let manifest = get_manifest output_dir

    let is_new_or_changed (f:string) = 
        if manifest.ContainsKey(f) = false then
            true
        else
            let old_hash = manifest.[f]
            let new_hash = Utils.md5(File.ReadAllBytes(f))
            old_hash <> new_hash

    let files = Garoozis.Utils.get_files(config.OutputDir) 
                |> Seq.filter (fun f -> Path.GetFileName(f).ToLower() <> "garoozis.manifest.txt")
                |> Seq.toList

    printfn "S3: uploading to bucket %s" bucketName
    files 
        |> List.map (fun f -> (f, get_url(f, output_dir), Garoozis.Utils.get_contentType(f)) )
        |> List.iter (fun (f,u,c) -> 
                            if config.DeleteExistingStorage = true || is_new_or_changed(f) = true then
                                printfn "   adding file: %s url: %s content-type:%s" f u c
                                service.AddObject(f, bucketName, u, c, LitS3.CannedAcl.PublicRead) 
            )

    for f in files do
        manifest.[f] <- Utils.md5(File.ReadAllBytes(f))

    write_manifest output_dir manifest

    ()