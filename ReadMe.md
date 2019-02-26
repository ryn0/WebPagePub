# WebPagePub
-----
WebPagePub is a blogging platform. It was written in C# and runs on .NET Core.


Deployment
-----
You will need to add a SQL Server and Azure Storage connection string to the appsettings.json file for the web and data projects. Both should have the same values.

The website and database can be deployed using the CI.bat file which needs parameters for the IIS server it should be deployed to. 


How to add a database migration
-----
entity framework commands

Add-Migration -Name NAMEOFMIGRATION -Project WebPagePub.Data -StartUpProject WebPagePub.Data 

Update-Database -Project WebPagePub.Data -StartUpProject WebPagePub.Data -Verbose


Features
-----
-Create/ edit blog entries with preview mode that has images and rich text editing on each
-Manage static files for use in the site 
-All static files uploaded work with a CDN for high speed
-Tagging of blogs for filtering
-Email subscribe to collect emails
-Sitemap of blog posts for Google
-Create/ edit snippets of text for different site sections
-Create/ edit link redirections (a link shortener)
-Login system for single admin


Workflow
-----
git remote set-url origin https://github.com/ryn0/WebPagePub.git
git remote set-url --push origin [YOUR_FORK_URL]

Azure Service Principal
------
Create a user for deployments and associate them with the correct subscription.
https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#required-permissions
https://docs.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-how-subscriptions-associated-directory

### CI Deployment 

To deploy to a custom server and database, in PowerShell (change variables for your settings):
```
$MsDeployLocation            = "https://YOURSITE.com:8172"
$webAppHost                  = "YOUR_DOMAIN.com"
$contentPathDes              = "C:\Sites\YOUR_DOMAIN.com\"
$msDeployUserName            = 'USER';
$msDeployPassword            = 'PASSWORD';
$dbConnectionString          = 'CONNECTION_STRING'

# repo path
cd "C:\repos\WebPagePub"

ci DeployWebApp  -properties "@{'NeutrinoApiUserId'='$NeutrinoApiUserId';'NeutrinoApiApiKey'='$NeutrinoApiApiKey';'MsDeployLocation'='$MsDeployLocation';'webAppHost'='$webAppHost';'contentPathDes'='$contentPathDes';'msDeployUserName'='$msDeployUserName';'msDeployPassword'='$msDeployPassword';'dbConnectionString'='$dbConnectionString';}"
```
