# Table of Contents

* [Table of Contents](#table-of-Contents);
* [Abstract](#abstract);
* [Legal](#legal);
* [Create Steamworks Account](#create-steamworks-account);
* [Steamworks Console](#steamworks-console);
  * [CLI/API access](#cli-api-access);
* [Deployment process](#deployment-process);
* [Statistics and Achievements](#statistics-and-achievements);
* [Afterword](#afterword);
* [Additional reading](#additional-reading);

# Abstract

This article describes how to configure your Steamworks account and use provided tools to deploy your game or application to Steam Store. The motivation behind this article is that the [similar article exists for Google Play](https://github.com/tariel36/SnippetsRepository/blob/master/Guides/Steam/ApplicationDeployment/README.md). Although, in comparison to Google Play, Steamworks are much, much better and user friendly.

Various numbers, information, API versions, GUI screenshots are up to date for 2022-05-07. In future, things may behave or look differently.

# Legal

Everything explained here is based on my experience and may compromise your security or in extreme cases, you may even break the law. You should **always** consult a lawyer, tax specialist and security specialist when interacting with access to your account, taxes and licenses or related.

**Whatever you do, you do it on your own responsibility.**

The codes used in examples are under [MIT license](https://opensource.org/licenses/MIT), until stated otherwise.

# Create Steamworks Account

Go to (Steamworks)[https://partner.steamgames.com/] and create normal Steam account. This is one of the hardest steps, since Steam tends to panic a lot about bots or fake accounts so they may just block you for several days for no reason. If you see the captcha, especially when it fails for no reason then most probably you're already banned for few days. Using any form of default concealment methods like private browser windows, ad-blockers, cookie blockers and so on may only worsen the situation. It seems that good VPN services (like Proton) tend to do the job. It's also worth a try to attempt account creation from your smartphone.

You will be redirected to the dashboard where you will start the landing process. For most part it's pretty straightforward, except for tax form which may be overwhelming at first. You will have to pay for the `Steam Direct` license beforehand too. In contrast to Google Play, you have to pay for each game separately (I'm not sure how DLCs work), 100 USD. However Steam will refund that when you earn at least 1000 USD in sales after taxes.

Tax form is pretty overwhelming to fill so I recommend that you do it with assistance of bookkeeper. If you don't want to spend more money yet then just fill the form with the details that you think are valid. Those forms are later validated by humans so if something goes wrong they will inform you and ask to correct. Don't be scared when they ask you for some kind of identity confirmation through personal document scan. The whole process takes few days to complete.

> There may be some steps missing at this point since by the time I wrote this article, I already had a developer account.

When you're done with account creation you'll be allowed to Steamworks console.

# Steamworks Console

SC looks pretty old and tough but it's much easier than Google Play to move and fill all the required information. At first you will see your single app in the dashboard. Click on it. You will see a lot of menu options and two check lists on the right side. Those checklists are the most important part of this console as they tell you what you have to do to publish your game. When you click on majority of the items they will redirect you to the place where you have to do required actions or display they tip what to do.

There aren't many tricky pages to explain so let's just move forward.

## CLI/API access
Steam has no deployment API, but it as a CLI, which is pretty straightforward and easy to use. You can get it [here](https://partner.steamgames.com/doc/sdk).

When you download and unzip the archive, find the `steamcmd` application and run it. Login with your Steam credentials. Steamguard will ask you to confirm with the code sent to your email. That's all, CLI access is set.

```
steamcmd
login <user> <pass>
steamguard code]
```

# Deployment process
Deployment is little confusing at first so I recommend to read about whole process in Steamworks documentation and to watch their tutorials. However in tl;dr terms you have to provide at least 2 configuration files and use them with their tools.

First of all grab you game's id. It's displayed in console, store's URL and other places.

Go to your game's page in SC and then `Technical Tools` -> `Edit Steamworks Settings`. From the `SteamPipe` menu select `Depots`. By default you'll have created single depot. You can use it or create new depots. You can think of it as repository for binaries of your game. Write down the id(s) of depot(s).

Go to your game's page in SC and then `Technical Tools` -> `Edit Steamworks Settings`. From the `SteamPipe` menu select `Builds`. When you deploy your binaries to any of your depots, you will see them here. From there you can select which binaries will be deployed to one of your branches. Branches are channels deployed to end users (players, testers and so on). The `default` branch is default public branch, so whatever you deploy there will go to the players. If you want to deploy any build to this branch you have to do it manually. We will get back to this step later.

Create `app_build_<game_id>.vdf` and `depot_build_<depot_id>.vdf` files in the `sdk/tools/ContentBuilder/scripts` directory (you can probably choose different directory because you will have to provide the path to the scripts anyway). If you want to push your binaries to more than one depot then you have to create `vdf` file for each depot.

Example `app_build_<id>.vdf` file:
```
"AppBuild"
{
	"AppID" "<id>"
	"Desc" "Build description"
	"Preview" "0"
	"Local" ""
	"ContentRoot" "<path to binaries root directory>"
	"BuildOutput" "<path to logs output directory>"
	"Depots"
	{
		"<depot id 1>" "depot_build_<depot id 1>.vdf"
		"<depot id n>" "depot_build_<depot id n>.vdf"
	}
}
```
As you can see there are few properties here. You can set more, but those are sufficient to deploy the application.
* AppID - your app's id;
* Desc - description of your build, visible only to you;
* Preview - `0` - your binaries will not be uploaded to steam; `1` - your binaries will be uploaded;
* Local - This is path to your SteamPipe Local Content Server, the files will be uploaded here instead steam's server - used for local testing;
* ContentRoot - the root directory of your ready-to-deploy binaries; It has to be relative path to the script file;
* BuildOutput - directory where steam will write it's logs, the path may be absolute;
* Depots - collection of depot files;


Example `depot_build_<id>.vdf` file.
```
"DepotBuild"
{
	"DepotID" "<id>"
	"FileMapping"
	{
		"LocalPath" ".\Windows\Steam\bin\*"
		"DepotPath" "."
		"Recursive" "1"
  }
}
```
* DepotID - your depot's id;
* LocalPath - path to the binaries that you want to push to this particular depot; It can be full path or delative to the `ContentRoot` from `app_build` file;
* DepotPath - relative path to the binaries starting from the install folder of your game;
* Recursive - if your local path has wildcards then all the files based on those wildcards will be included or not, based on `1` or `0` value;

After you prepare your build scripts, open `steamcmd`, login and execute following command to push your binaries:
```
run_app_build <absolutepath>\app_build_<id>.vdf
```

After your files has been uploaded you can get back to the `Builds` tab in SC. You should see your build and included depots in the table. From `Set build live on branch` select box, select desired branch and click `Preview Change`. Steam will ask for confirmation. Provide the confirmation and you're done.

# Statistics and Achievements
This is not really related to the deployment process but I found this part the most confusing in Steamworks console so I decided to add few words about it.

Achievements system is separated into statistics and achievements tabs. Achievements are based on statistics so you should start there. Statistics have few fields that are pretty straightforward. The most confusing are:
* Max Change - it indicates the maximum change value for the statistic, so if you set this to `1` then even if your game request change by `100` it will still be changed by `1`;
* Display Name - this is not shown in game and can't be localized; I'm not sure where it's used;

When adding an achievement you will have to provide display name (which can be localized), what sets/grants the achievement (client or server), whether achievement should be hidden (for example, when the name or description contains spoilers), icons, API name and **progress stat**. The last one got me the most confused, because you have to provide minimum value and maximum value for the selected stat (remember that you provide similar values for the stat itself). It turns out that the min/default/max value set for statistic is not really related to achievement. If you want to grant achievement when your progress stat reaches certain value then you set it here - min and max value.
> For example, if you have games_played statistic, and an achievement for 100 games played, you will set the min and max of this progress stat for this achievement to 100-100.

# Afterword
Steamworks may look old and their documentation is slightly outdated but they provide a lot of information in structured manner, with plethora of examples and some videos. At first one may feel overwhelmed but if taken step by step with some experimentation it gets clear pretty fast.

# Additional reading
More or less useful articles to read and videos to watch:

* [https://partner.steamgames.com/doc/sdk](https://partner.steamgames.com/doc/sdk);
* [https://www.youtube.com/watch?v=SoNH-v6aU9Q&list=PLckFgM6dUP2jBeskIPG-7BVJR-I0vcSGJ&ab_channel=SteamworksDevelopment](https://www.youtube.com/watch?v=SoNH-v6aU9Q&list=PLckFgM6dUP2jBeskIPG-7BVJR-I0vcSGJ&ab_channel=SteamworksDevelopment);
