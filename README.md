Garoozis let's you publish static HTML files using F# and Razor Templates.
==========================================================================

Use this tool for simple website hosting. With it you can
* create static html pages by transforming markdown or html with Razor templates
* copy these static files to Amazon S3 Windows Azure Blob storage for website hosting


This is kind of like [Jekyll](https://github.com/mojombo/jekyll), but more for .NET.

** THIS IS VERY EARLY STAGE. USE JEKYLL IF YOU NEED SOMETHING LIKE IT **

Some Notes:
-----------
* like in Jekyll, some info is placed at the top of your text files. it's Json in this case, because using Yaml in .NET is beyond my skills
* project is organized in Modules.  I'd like to make it more .NET friendly
* this was originally an executable.  I changed it to a DLL. That way you can set up an .fsx to publish the site.  Nice.


Here is what this is going to do:
--------------------------------
* can source directory for html or markdown, transform with razor.  Top level search only, and special folders _posts, _layouts
* intelligently process posts - specifically have next/previous links
* write output to some \www staging folder
* copy other folders such as \static or \images to staging folder
* run YUI compressor on all files in \www staging folder
* push changeset up to Windows Azure

Items to Do:
------------
* Create intelligent models - Next/Previous, Table of Contents, etc
* Push to Azure


Pushing to S3 is somewhat implemented.  the S3 documentation is useful for setting up a website.  [s3](http://docs.amazonwebservices.com/AmazonS3/latest/dev/index.html?WebsiteHosting.html)
