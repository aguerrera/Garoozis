module Garoozis.RemoteStorage

open System
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
        printfn "s3: deleting item %s" item.Name
        service.DeleteObject(bucketName,item.Name)

// delete items from your bucket then republish all
let publish_to_s3 (config:Config)= 
    let service = new LitS3.S3Service(AccessKeyID = config.StorageKey, SecretAccessKey = config.StoragePass)
    let output_dir = config.OutputDir
    let bucketName = config.StorageContainer

    delete_all_items_from_s3 service bucketName |> ignore

    let files = Garoozis.Utils.get_files(config.OutputDir) 
                |> Seq.toList

    files 
        |> List.map (fun f -> (f, get_url(f, output_dir), Garoozis.Utils.get_contentType(f)) )
        |> List.iter (fun (f,u,c) -> 
            printfn "adding file: %s url: %s content-type:%s" f u c
            service.AddObject(f, bucketName, u, c, LitS3.CannedAcl.PublicRead) 
            )

