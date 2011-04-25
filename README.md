Garoozis let's you publish static HTML files using F# and Razor Templates.
==========================================================================

Use this tool for simple website hosting. With it you can
* create static html pages by transforming markdown or html with Razor templates
* copy these static files to Amazon S3 Windows Azure Blob storage for website hosting

This is kind of like [Jekyll](https://github.com/mojombo/jekyll), but for .NET.  And without the Git integration (so it's not really that much like Jekyll).  So it is what it is.  
The way I use it is that I point Garoozis at a source repository and run it.

A full model is generated based on your markdown or html files, and then this model is passed to the Razor engine for templating. Here is a snipped from a sample Razor template:

    CREATE AN INDEX OF ALL POSTS
	<ul>
	@foreach (var post in Model.Posts) {
		<li><a href="/@post.Url">@post.Title</a></li>   
	}
	</ul>

    SHOW NEXT AND PREVIOUS POSTS
	<div>
	@if (Model.NextPost != null && Model.NextPost.Title != "")
	{
		<div>Next: <a href="../@Model.NextPost.Url">@Model.NextPost.Title</a></div>
	}
	@if (Model.PrevPost != null && Model.PrevPost.Title != "")
	{
		<div>Prev: <a href="../@Model.PrevPost.Url">@Model.PrevPost.Title</a></div>
	}

	@for (int i = 0; i < 10; i++)
	{
		<div>@i. Item</div>
	}
	</div>


Your source directory should be have the following structure:
_layouts/   <- your .cshtml Razor templates will be here.
_posts/     <- your blog posts will be here.  if you name your file 2011-4-20-My-First-Post.md it will get transformed into the file /2011/4/20/My-First-Post.html  Do not create any other directories in the /_posts directory.

You can then have any other html or markdown files. These will be treated as pages (not blog posts). For example:
index.html 
error.html
about.html



Some Notes:
-----------
* like in Jekyll, some info is placed at the top of your text files. it's Json in this case, because using Yaml in .NET is beyond my skills
* your blog posts are saved in the special _posts folder
* all html or markdown files are processed and spat out as static html
* garoozis builds a model that is passed to the Razor engine that you can use (all page info, Next/Previous Posts, all Posts, all non-blog post Pages, etc)
* this static output is written to a staging folder, and is optionally compressed
* an rss feed is generated from the files in the /_posts directory
* all assets in your source folder are copied to the output directory
* all files in the output directory are pushed to S3 (and soon Windows Azure)



Items to Do:
------------
* Push to Azure
* project is organized in Modules.  I'd like to make it more .NET friendly

Hosting a website at S3
The S3 documentation is useful for setting up a website.  [s3](http://docs.amazonwebservices.com/AmazonS3/latest/dev/index.html?WebsiteHosting.html)
You must first create an S3 bucket and then enable it for use as a website.  Garoozis then pushes the output files to that bucket, which you can then browse. The actual URL that you access this website-enabled bucket with 
will be provided in the In the AWS Managment Console. See http://test.guerrera.org for the sample output.

DNS
---
You will need to go to your domain registrar or DNS provider to map a CNAME or forward your URL to that bucket.  See the AWS Management Console to get the URL.

