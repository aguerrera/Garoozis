module Garoozis.RemoteStorage

open System
open System.IO
open LitS3
open Garoozis.Models

let get_url (f:string, output_dir:string)  = 
        f.ToLower().Replace(output_dir.ToLower(), "").Replace("\\", "/")

let publish_to_s3 (config:Config)= 
    let service = new LitS3.S3Service(AccessKeyID = config.StorageKey, SecretAccessKey = config.StoragePass)

    let output_dir = config.OutputDir
    let bucketName = config.StorageContainer

    let files = Garoozis.Utils.get_files(config.OutputDir) 
                |> Seq.toList
    files 
        |> List.map (fun f -> (f, get_url(f, output_dir), Garoozis.Utils.get_contentType(f)) )
        |> List.iter (fun (f,u,c) -> service.AddObject(f, bucketName, u, c, LitS3.CannedAcl.PublicRead) )


