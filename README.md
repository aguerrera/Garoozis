Create a blog with static HTML files using Razor Templates, HTML, and Markdown
==============================================================================

### Then post your site to S3 (or anywhere you want it published) ###

Garoozis is for simple website hosting or blogging. With it you can:

* create static html pages by transforming markdown or html with Razor templates
* process files in a special /_posts directory into a blog (with rss)
* copy these static files to Amazon S3 Windows Azure Blob storage for website hosting
* use the built in development server to edit your markdown and templates and see what the output looks like in your browser (change the .cshtml and hit F5!)
* use the built in development server to preview your static site

This is kind of like [Jekyll](https://github.com/mojombo/jekyll), but for .NET.  And without the Git integration (so it's not really that much like Jekyll (there are no SCM dependencies)).  So it is what it is.  
The way I use it is that I point Garoozis at a source repository and run it.

A full model is generated based on your markdown or html files, and then this model is passed to the Razor engine for templating. Here are some snippets from a sample Razor template:

    CREATE AN INDEX OF ALL POSTS
	<ul>
	@foreach (var post in Model.Posts) {
		<li><a href="/@post.Url">@post.Title</a></li>   
	}
	</ul>

    SHOW NEXT AND PREVIOUS POSTS
	<div>
	@if (Model.NextPost.Title != "")
	{
		<div>Next: <a href="../@Model.NextPost.Url">@Model.NextPost.Title</a></div>
	}
	@if (Model.PrevPost.Title != "")
	{
		<div>Prev: <a href="../@Model.PrevPost.Url">@Model.PrevPost.Title</a></div>
	}

    CREATE INDEX OF ALL POSTS
    <ul>
    @foreach (var post in Model.Posts.OrderByDescending(x => x.Created) )
    {
        <li><a href="/@post.Url">@post.Title</a></li>   
    }
    </ul>

Note that the @Html extensions or any ASP.NET dependencies are not available for use in the .cshtml. But you can use good old C# in there.  In the future, it could be
possible to exend the Garoozi's default page model.

To set up your site, your source directory should be have the following structure:

     Layout
     _layouts/   <- your .cshtml Razor templates will be here.
     _posts/     <- your blog posts will be here.  if you name your file 2011-4-20-My-First-Post.md it 
	             will get transformed into the file /2011/4/20/My-First-Post.html  
   	.

     You can then have any other html or markdown files. These will be treated as pages (not blog posts). For example:
     index.html 
     error.md
     about.html


In each of your files, you will want to add a special configuration Front Matter.  In Jekyll, this is done with YAML.  Here it's JSON because YAML in .NET is beyond my current skill set.

	{
	  title: "Page One", /* Title of Page */
	  layout: "post"     /* name of Razor template to use.  For instance, this would use a template _layouts/post.cshtml */
	}
        
    HERE Is where I would start typing the content of my Page!

	
Once you have your folder structured properly, you are ready to go.  You will need to set up a config.js file (see the sample app.js file).  Then you can set up Build.fsx, Publish.fsx, and Dev.fsx files.  
See the sample folder for examples.  You can then right click on these files to run via F# Interactive, or you can go to the command line and type:

	> fsi dev.fsx  // to start a server at http://localhost:8088  
                   // you can then browse to this as you write your content in markdown or update your
				   // Razor templates
	> fsi build.fsx   // to build
	> fsi publish.fsx  // to publish to s3
	

Some Notes:
-----------
* json front matter is used in all of your posts/pages for additional configuration
* your blog posts are saved in the special _posts folder in a format, e.g. yyyy-m-d-page-name.md
* all html or markdown files are processed and spat out as static html
* garoozis builds a model that is passed to the Razor engine that you can use (all page info, Next/Previous Posts, all Posts, all non-blog post Pages, etc)
* this static output is written to a staging folder, and is optionally compressed
* an rss feed is generated from the files in the /_posts directory
* all assets in your source folder are copied to the output directory
* all files in the output directory are pushed to S3 (and soon Windows Azure) (the diff is copied)


Hosting a website at S3
-----------------------
The S3 documentation is useful for setting up a website.  [s3](http://docs.amazonwebservices.com/AmazonS3/latest/dev/index.html?WebsiteHosting.html)
You must first create an S3 bucket and then enable it for use as a website.  Garoozis then pushes the output files to that bucket, which you can then browse. The actual URL that you access this website-enabled bucket with 
will be provided in the In the AWS Managment Console. See http://garoozis.guerrera.org for the sample output.


DNS
---
You will need to go to your domain registrar or DNS provider to map a CNAME or forward your URL to that bucket.  See the AWS Management Console to get the URL.

