namespace Garoozis.Models

open System
open System.Collections.Generic

// config type
type Config() =
    let mutable m_outputdir = ""
    let mutable m_sourcedir = ""
    let mutable m_storagecontainer = ""
    let mutable m_storagekey = ""
    let mutable m_storagepass = ""
    let mutable m_url = ""
    let mutable m_blogtitle = ""
    let mutable m_blogdesc = ""
    let mutable m_compressoutput = true
    let mutable m_deleteexistingstorage = false

    member x.OutputDir          with get () = m_outputdir
                                and set outputdir = m_outputdir <- outputdir
    member x.SourceDir          with get () = m_sourcedir
                                and set sourcedir = m_sourcedir <- sourcedir
    member x.StorageContainer   with get () = m_storagecontainer
                                and set container = m_storagecontainer <- container
    member x.StorageKey         with get () = m_storagekey
                                and set skey = m_storagekey <- skey
    member x.StoragePass        with get () = m_storagepass
                                and set spass = m_storagepass <- spass
    member x.Url                with get () = m_url
                                and set url = m_url <- url
    member x.BlogTitle          with get () = m_blogtitle
                                and set title = m_blogtitle <- title
    member x.BlogDesc           with get () = m_blogdesc
                                and set desc = m_blogdesc <- desc
    member x.CompressOutput     with get () = m_compressoutput
                                and set compressoutput = m_compressoutput <- compressoutput
    member x.DeleteExistingStorage     with get () = m_deleteexistingstorage
                                       and set deleteexistingstorage = m_deleteexistingstorage <- deleteexistingstorage

                                
// each web page is represented as a Page
[<AllowNullLiteral>]
type Page() = 
    let mutable m_title = ""
    let mutable m_content = ""
    let mutable m_filename = ""
    let mutable m_layout = ""
    let mutable m_created = DateTime.MinValue
    let mutable m_url = ""
    member x.Title with get () = m_title
                        and set title = m_title <- title
    member x.Content with get () = m_content
                        and set content = m_content <- content
    member x.FileName with get () = m_filename
                        and set fileName = m_filename <- fileName
    member x.Layout with get () = m_layout
                        and set layout = m_layout <- layout
    member x.Url with get () = m_url
                        and set url = m_url <- url
    member x.Created with get () = m_created
                        and set created = m_created <- created

// the Model will represent the currently processed page as well as a list of all pages.
type Model() = 
    let mutable m_page = new Page()
    let mutable m_next = new Page()
    let mutable m_prev = new Page()
    let mutable m_posts = new List<Page>()
    let mutable m_pages = new List<Page>()
    member x.Page with get () = m_page
                        and set page = m_page <- page
    member x.NextPost with get () = m_next
                        and set next = m_next <- next
    member x.PrevPost with get () = m_prev
                        and set prev = m_prev <- prev
    member x.Posts with get () = m_posts
                        and set posts = m_posts <- posts
    member x.Pages with get () = m_pages
                        and set pages = m_pages <- pages
    member x.Title with get () = m_page.Title
    member x.Content with get () = m_page.Content
    member x.FileName with get () = m_page.FileName
    member x.Layout with get () = m_page.Layout
    member x.Created with get () = m_page.Created
