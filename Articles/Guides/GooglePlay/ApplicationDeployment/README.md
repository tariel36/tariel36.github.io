# Table of Contents

* [Table of Contents](#table-of-Contents);
* [Abstract](#abstract);
* [Legal](#legal);
* [Create Google Play account](#create-google-play-account);
* [Google Play Console](#google-play-console);
  * [Payments profile](#payments-profile);
  * [API access](#api-access);
    * [Create service account](#create-service-account);
    * [Activate APIs](#activate-apis);
* [Create an API key](#create-an-api-key);
* [Add your application / game](#add-your-application--game);
  * [Releases and tests](#releases-and-tests);
* [Deployment process](#deployment-process);
  * [Building the application](#building-the-application);
  * [Preparing release and signing the application](preparing-release-and-signing-the-application);
  * [CI/API C# examples](ci-api-c-examples);
  * [NUKE BUILD tips](#nuke-build-tips);
* [Afterword](#afterword);
* [Additional reading](#additional-reading);

# Abstract

This article describes how to configure your Google Play account and use their API to deploy your game or application to Google Play Store. The motivation behind this article is that the majority of articles and documentation are trash and useless.

Various numbers, information, API versions, GUI screenshots are up to date for 2022-04-22. In future, things may behave or look differently.

# Legal

Everything explained here is based on my experience and may compromise your security or in extreme cases, you may even break the law. You should **always** consult a lawyer, tax specialist and security specialist when interacting with access to your account, taxes and licenses or related.

**Whatever you do, you do it on your own responsibility.**

The codes used in examples are under [MIT license](https://opensource.org/licenses/MIT), until stated otherwise.

# Create Google Play account

This step requires:

* That you already have a Google account created;
* You must have 2FA enabled for your account;

> You would want to create a separate account just for development purposes. If for some reason Google makes it difficult to create another account on your PC, it usually works when you try again from your phone. Since you need 2FA anyway, it's a good option.

Login to your account and navigate to Google Console. You'll be presented with a choice, whether you want a developer account for yourself or for an organization or company. If you're not creating it for the latter and you don't have a sole proprietorship, choose personal account, otherwise it's the latter one.

Provide the details Google asks for. **You need a valid phone number to continue**. When you're done with the form, you'll have to pay for the developer license. It's single time payment of 25 USD. License is valid for a whole developer account and allows you to publish unlimited amount of games and applications (in contrast to Steam, which requires payment for each app).

> There may be some steps missing at this point since by the time I wrote this article, I already had a developer account.

When you're done with account creation you'll be allowed to Google Play Console.

# Google Play Console

GPC allows you to do a lot of stuff. The most important pages are:

* All apps - this is dashboard-like page with all your applications;
* Inbox - there are notifications directly from Google related to the development and publication;
* Policy status - whether there are any terms-like issues with your account, they're listed here;
* Order management - all the orders, refunds and other payment related things are listed here;
* Developer page - configuration page for your developer's business card;
* Game projects - all your games;

The pages that you should look at least once:

* Users and permissions - there are listed all user accounts with their permissions - here you can configure an account for your CI too;
* Associated developer accounts - this shows with which developer accounts you're associated with - this page is used by Google to determine if you're eligible for lesser service fee;
* Email lists - various collections of emails, used for promotion, or for example, internal tests;
* Pricing templates - if you have multiple apps with similar pricing, or any other reason to have templates with fixed prices you can use this page. **It is also useful, because you can set a price with included tax, while you can't do so on game's pricing page**;

> Later in this article, you'll see that you can set a price for your app or game. On its' page, the price can be set only without tax included, which is annoying when you want to have a particular price with the tax already included. To achieve that, just create proper pricing template and use it instead of fixed price for your game.

* Payment profiles - here you can manage your payouts (earnings) and other payment related things, like lesser service fee;
* API access - here you can configure API access to your console, this is very important page if you plan to integrate with Google's APIs;

## Payments profile

On this page you can enroll for the 15% service fee (**it's 30% otherwise**). Whether you're eligible for that, it's for Google to decide. To do so you need to configure account groups. Click to enroll and you'll be redirected back to **Associated developer accounts** page. If you already configured your group, start the process. Google will ask you some questions. If that's your only developer account then it's straightforward. Answer the questions and click save. Then back on the ADAs page click `Review and enroll` , then confirm. Remember to carefully read the terms and consult your lawyer or other specialist, if necessary or in doubt.

## API access

If you want to access your console with Google's APIs, you need to activate them here. This process is a bit complex and separated into multiple steps.

1. Create service account;
2. Activate API types - for the sake of deployment, activate the `Play Android Developer API` and `Play Games Services Publishing API`;

### Create service account

Start with clicking the `Create new service account`. You'll be presented with a popup redirecting you to Google Cloud Console. Follow the instructions from the popup.

When you're in the `Google Cloud Platform`, `Service Accounts` page, click the `Create service account` button. Provide the account name, service account id and description. You can set the service account roles and users' access to this account.

> You should setup proper roles for this account by trial and error. But if you don't really care you can set the Owner (it's like Super Admin) role and you're good to go.

Now you can generate this account's keys if you want to go with the CI later on. Go to this account's details and then the `Keys` tab. Then `Add key` and `Create new key`, select `JSON` type and then click `Create`.

While you're still on the Google Cloud Platform and want to use CI later on, you might want to jump to the [Create an API key](#create-an-API-key) step and then get back here.

When you're done with all that, go back to the Google Play Console and then `Setup/API access` tab. In the `Service accounts` region click `Refresh service accounts` and add previously created service account. **Remember to set proper permissions**.

## Activate APIs

To activate specific APIs, just click the `Turn on` button next to the selected API.

# Create an API key

This step is only relevant if you want to use some kind of CI and/or APIs.

Go to the Google Cloud Platform, move to the `APIs & Services` and then `Credentials` tab. Click the `Create Credentials`, then `API key`. You'll be presented with a popup and your API key. Copy it for later use.

# Add your application / game

Go to the `All apps` tab and click `Create app` in top right corner. Provide required information in form and click `Create app`.

Then move to the app's page by clicking `View app` next to the app in apps table.

App's dashboard is similar to the previous one in terms of layout. There are several new options in the menu, though. On the dashboard page there is nice `Set up your app` step list that will walk you through the whole process.

The tricky steps are:

* Data safety - during the `Target audience` step, Google informs you that you'll need **privacy policy** if you wish to target audience of age lower than 13. You may think that it's only required if you wish to do so, but it's required for all applications even if you don't collect any data. **For this steps I highly recommend to contact a lawyer**;
* Set the price of your app - if you read the whole article up to this moment, you'll know that setting application specific price has its draw backs, otherwise you're good to go;

## Releases and tests

To release the application, you'll need to finish all the steps on the step list presented on the dashboard page. When you're done with that you can create production release. To do so move to the Production tab and follow the instructions.

If you don't want to go public with your app yet, just make internal tests you don't have to finish all the steps from the dashboard list. Go to the `Testing/Internal testing` tab and follow the instructions.

It is important to mention that since year 2019, you have to prepare the binaries package in the `aab` format, not `apk`. The package has to be signed.

# Deployment process

This chapter provides examples for `Visual Studio 2022` and `C#`. If you use other tools it may be similar for you, but it may not be useful at all. On the other hand, the theory will be useful.

## Building the application

To properly build the project, remember to set following properties:

* Application -> Minimum required Android's version;
* Android Manifest
  * Remember to set `Version number` to unique value - each deployment has to have bigger number than previous;
  * Proper `Minimum Android version`;
  * Proper `Target Android version`;
* Android options -> Android Package Format -> bundle - so the application will be built as `aab` not `apk`;
* Remember to set proper package name, version and version code in `AndroidManifest.xml`:
  * Package name is required to have the format `com.somename.somename` - `com` is required, name should be all lowercase;

## Preparing release and signing the application

To sign your application in Visual Studio, right click on the Android project and click `Archive`. New tab will open and VS will begin the process. This will prepare the release package for you. Then you have to sign it. Click the `Distribute` button in the lower part of the tab. If you want to distribute the application on your own, that is upload through your own CI script, or just send to friends then select `Ad Hoc`. If you want VS to do the job for you, select `Google Play`.

You will be asked to select the Signing Identity. If there is none, create one. Select the proper identity and proceed (for example `Save As` in `Ad Hoc` process). That's all, you "distributed", or rather prepared the distribution - one way or another.

> Remember, once you upload the signed application, you're bound to selected key and keystore for this particular application on Google Play. So, if you're fiddling and learning and upload application signed with debug key, then you're bound to debug key.
> Also, once you upload binaries to Google Play, you no longer can delete this application card. Best you can do is changing its name to some glibberish.
> The package name must also be unique, so if you're still testing all the things out, set some random package name in case you lock yourself with random Google Application page and still want to use particular package name in future.

## CI/API C# examples

You will need following packages:

* Google.Apis.AndroidPublisher.v3;

```cs
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PublishPipes
{
    public class GooglePlayPublishPipe
    {
        public async Task PublishAsync()
        {
            // Path to the file with service account secrets.
            const string pathToServiceAccountCredentials = "";

            // Generated API key.
            const string googleApiKey = "";

            // Yes, both API key and service account secrets are required.

            // Use:
            // * internal - internal tests;
            // * production - public release;
            // Other values: alpha, beta, internal, production;
            // See also: https://developers.google.com/android-publisher/tracks
            const string trackName =  "internal";

            // Use:
            // * completed - to make it available to everyone (defined by other means, like mailing lists, or track type).
            // Se also: https://developers.google.com/android-publisher/api-ref/rest/v3/edits.tracks#status
            const string releaseStatus = "completed";

            // Path to the AAB file.
            const string apkFilePath = "";

            // The name of the release visible to you in console.
            // This parameter is not required and will use version number (not version code!) if not provided.
            const string releaseName = "";

            // Set the proper version code based on your android manifest.
            const long? versionCode = 0;

            // The package name from AndroidManifest.xml.
            const string packageName = "";

            // The change log for this release. The actual value must be within the localization tag.
            // So if your release notes are in english (US), then use `en-US` tag like in the example below.
            // Be careful with the length, because release notes may have only up to 500 characters per language.
            const string releaseNotes = "<en-US>Some notes</en-US>";

            GoogleCredential credentials = GoogleCredential.FromFile(pathToServiceAccountCredentials);

            using (AndroidPublisherService service = new AndroidPublisherService(new BaseClientService.Initializer
            {
                ApplicationName = "NUKE Deploy script",
                HttpClientInitializer = credentials,
                ApiKey = googleApiKey
            }))
            {
                // Google docs specify that you should increase HTTP timeout.
                service.HttpClient.Timeout = TimeSpan.FromMinutes(10);

                // Each edit is transactional, so untill everything succeeds the edit will not be aplied.
                // Start with creating the edit (release).
                AppEdit edit = await service.Edits.Insert(new AppEdit(), packageName).ExecuteAsync();

                // Upload the binary file.
                using (FileStream fs = new FileStream(apkFilePath, FileMode.Open))
                {
                    EditsResource.BundlesResource.UploadMediaUpload uploader = service.Edits.Bundles.Upload(packageName, edit.Id, fs, "application/octet-stream");
                    // This is required even if you don't plan or using pausable upload.
                    Uri uri = await uploader.InitiateSessionAsync();

                    // You don't have to subscribe to this event.
                    uploader.ProgressChanged += BundleRequest_ProgressChanged;

                    // Execute the upload.
                    IUploadProgress progress = await uploader.UploadAsync();
                  
                    // Unsubscribe.
                    uploader.ProgressChanged -= BundleRequest_ProgressChanged;
                }

                // Prepare the release details.
                TrackRelease release = new TrackRelease
                {
                    // This parameter is optional.
                    Name = releaseName,
                    // This is required
                    VersionCodes = new List<long?> { versionCode },
                    ReleaseNotes = releaseNotes,
                    Status = releaseStatus,
                };

                // Assign to the track.
                Track trackBody = new Track
                {
                    Releases = new[] { release },
                };

                // Execute the track assignment.
                Track track = await service.Edits.Tracks.Update(trackBody, packageName, edit.Id, trackName).ExecuteAsync();

                // Validate the change. If null is returned, then validation failed (for some reason).
                AppEdit validatedEdit = (await service.Edits.Validate(packageName, edit.Id).ExecuteAsync()) ?? throw new InvalidOperationException("Failed to validate edit.");

                // Apply the change. This step actualy executes everything what you did so far and makes it public (one way or another).
                // If null is returned, then the commit failed.
                AppEdit commitedEdit = await service.Edits.Commit(packageName, edit.Id).ExecuteAsync() ?? throw new InvalidOperationException("Failed to commit edit.");
            }
        }
      
        private void BundleRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress obj)
        {
            Console.WriteLine($"Status: {obj.Status}, Bytes: {obj.BytesSent}, Error: {obj.Exception?.Message ?? ""}");
        }
    }
}

```

## NUKE BUILD tips

If you have migrated, or will migrate your project/s from the older versions of `Visual Studio` to the newest one and used the `NUKE BUILD` to deploy your application you might want to know about few things:

* Update your NUKE installation;
* Update your scripts, etc. related to NUKE - NUKE CLI provides you tools to do so;
* Ensure to change `-host` argument to new proper value, (for example, `-host VisualStudio` if you're deploying locally);
* If you have more than one VS installed (like 2017 and 2022) you might want to set explicitly the following properties of the MSBuild instance:
  * MSBuildVersion;
  * MSBuildToolPath;

```cs
private const string MsBuildToolPath = @"<path to ms build>/msbuild.exe"; 
private const MSBuildVersion BuildVersion = MSBuildVersion.VS2022;

public Target Restore => target => target
    .DependsOn(Clean)
    .Executes(() =>
    {
        MSBuildTasks.MSBuild(s => s.SetTargetPath(Solution)
            .SetConfiguration(Configuration)
            .SetMSBuildVersion(BuildVersion)
            .SetProcessToolPath(MsBuildToolPath)
            .SetTargets("Restore"));
    });
```

# Afterword

As I said, the majority of existing documentation, articles and examples are utterly trash. This one is too, I suppose. However, I think it's good to share my own experiences with that, without typical bullshit like majority of articles have, like history of android, google, or other not related trash.

# Additional reading

More or less useful articles to read:

* [https://stackoverflow.com/questions/66007292/](https://stackoverflow.com/questions/66007292/);
* [https://developers.google.com/android-publisher/api-ref/rest/v3/](https://developers.google.com/android-publisher/api-ref/rest/v3/);
* [https://stackoverflow.com/questions/11071127/google-play-app-description-formatting](https://stackoverflow.com/questions/11071127/google-play-app-description-formatting);
