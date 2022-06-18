# Table of Contents

* [Table of Contents](#table-of-Contents);
* [Abstract](#abstract);
* [Legal](#legal);
* [NAS and host machine configuration](#nas-and-host-machine-configuration);
* [Install Container Station](#install-container-station);
* [Prepare your NAS to install new docker applications](#prepare-your-nas-to-install-new-docker-applications);
* [[Optional] Create user for docker private image registry](#optional-create-user-for-docker-private-image-registry);
* [Install `Registry` application](#install-registry-application);
* [Add registry in container station](#add-registry-in-container-station);
* [Connect to NAS through SSH as administrator](#connect-to-nas-through-ssh-as-administrator);
* [Create certificate for our registry](#create-certificate-for-our-registry);
* [Install certificate on your machine](#install-certificate-on-your-machine);
* [Configure docker on your host machine](#configure-docker-on-your-host-machine);
* [Pushing images to your registry](#pushing-images-to-your-registry);
* [Pulling images from your repository](#pulling-images-from-your-repository);
* [Executing own your `docker-compose.yml` files](#executing-own-your-docker-compose.yml-files);
* [[Optional] Enable you VPN services.](#optional-enable-you-vpn-services.);

# Abstract

This article describes how to setup your QNAP NAS to host dockerized apps, including your own private image registry and example application.

This article focuses on the windows host machine, so linux users may find part of this guide unusable.

# Legal

Everything explained here is based on my experience and may compromise your security or break your devices. You do it on your own risk.

**Whatever you do, you do it on your own responsibility.**

The codes used in examples are under [MIT license](https://opensource.org/licenses/MIT), until stated otherwise.

# NAS and host machine configuration

## Install Container Station

Open `App Center` and find `Container Station`. Install it. This process is pretty strightforward.

## Prepare your NAS to install new docker applications

Several errors may occur on our way when installing docker applications. The one I had to resolve was related to the VPN services, and the message is:

```
Background task error for application registry: Creating network "<network name>" with the default driver could not find an available, non-overlapping IPv4 address pool among the defaults to assign to the network
```

If you have any VPN service running on your NAS, like `OpenVPN`, `QVPN`, `ZeroTier` or others, stop them through `App Center` for time being. You can enable them later, when you're done with installing docker applications. You have to repeat this step each time you want to add new docker application.

## [Optional] Create user for docker private image registry

This step is optional. If you want you can use `admin` or any other account with sufficient permissions.

## Install `Registry` application

Open `Container Station`, select `Create` tab and search for `Registry` application. Click on `Create` button. You can copy additional instructions for later use.

## Add registry in container station

Open `Container Station`, select `Preferences` tab and then `Registry` tab. There, click `Add` button and provide the data:

| Property | Value |
|----------|-------|
| Name     | Any name you want |
| URL      | Your NAS address, including the `6088` port (see [registry](#install-registry-application) instalation), for example: `https://192.168.0.1:6088` |
| Username | User related to registry |
| Password | Password related to registry |
| Trust SSL Self-Signed Certificate | Check |
| Set to default | Check |

Check the connection with `Test` button, then if everything is fine click `Add`.

## Connect to NAS through SSH as administrator

You can enable SSH in `ControlPanel`. The two good enough apps to work with SSH (for windows) is [PuTTY](https://www.putty.org) and [WinSCP](https://winscp.net/eng/index.php). I recommend you get both of them.

## Create certificate for our registry

Run `mkdir -p /etc/docker/certs.d/<YOUR-NAS-IP>:6088` command.

Copy the certificate file from above path to your windows machine ([WinSCP](https://winscp.net/eng/index.php) will be good enough for that purpose).

## Install certificate on your machine

When the certificate file is on your host machine, double click itand install to `Trust Root Certification Authorities`.

## Configure docker on your host machine

Move to docker resource path (by default `C:\Program Files\Docker\Docker\resources`) and edit `linux-daemon-options` and `windows-daemon-options` files. Add following values to the `insecure-registries` key:

* `https://<YOUR-NAS-IP>:6088`
* `<YOUR-NAS-IP>:6088`

Remember to use quote marks, since those are strings: `"insecure-registries": ["https://<YOUR-NAS-IP>:6088","<YOUR-NAS-IP>:6088"],`

After all that, restart your docker service - stop `Docker Desktop` and in services restart `Docker Desktop Service`, then run `Docker Desktop` again.

# Pushing images to your registry

To push image to your own registry created in one of the previous steps, you have to tag it properly. The command for that is:

```
docker tag SOURCE_NAME REPOSITORY_ADDRESS/TARGET_NAME
```

or 

```
docker tag SOURCE_NAME REPOSITORY_ADDRESS/USER/TARGET_NAME
```

I used the former with success. So for example: `docker tag myimagename_part1/myimagename_part2 192.168.0.1:6088/myimagename_part1/myimagename_part2`.

Use following command to push image to your registry:

```
docker push TAG_NAME
```

Using the same example as above: `docker push 192.168.0.1:6088/myimagename_part1/myimagename_part2`.

Remember, this is pushing the image to the registry, if you want to use it in `Container Station` you have to pull it there.

# Pulling images from your repository

By default, following command should be sufficient: 

```
 docker pull 192.168.0.110:6088/moviedb/backend
```

Using above example: `docker pull 192.168.0.1:6088/myimagename_part1/myimagename_part2`.

However, I had many issues when pulling through `Container Station` so I recommend to login through SSH and use `docker` CLI to do so.

After your image has been pulled, you can retag it to previous name with `docker tag` command again, for example `docker tag 192.168.0.1:6088/myimagename_part1/myimagename_part2 myimagename_part1/myimagename_part2`.

# Executing own your `docker-compose.yml` files

`Container Station` has GUI to execute your own `docker-compose.yml` files but it just doesn't work properly. It ignores the custom registries (or fails to login, I don't know which, it just doesn't work), generates random errors. It's just not worth the time to work it in GUI. So the best is to login through SSH again and use `docker-compose` CLI (pay attention to the `-`).

The `Container Station` (or `docker-compose`) I used was pretty old and did not support the `name` parameter, but it uses the directory name to group containers together. So just copy your `docker-compose.yml` file to the properly named directory and run it with `docker-compose up -d` command.

It should create proper application group on your NAS.

# [Optional] Enable you VPN services.

When you're done with installing new containers, you can enable back your VPN services.