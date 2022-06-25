This repository contains my mods for VRChat. Join the [VRChat Modding Group discord](https://discord.gg/rCqKSvR) for support and more mods!

## Installation

Requirements:
- [MelonLoader](https://github.com/LavaGang/MelonLoader#how-to-use-the-installer)
- [knah's UIExpansionKit](https://github.com/knah/VRCMods/)

Then download the .dll mods you want [from here in the release section](https://github.com/dakyneko/DakyMods/releases) which you must place into your `Mods` folder of your game directory (check Steam installation directory).

![screenshot](dakymods1.jpg?raw=true "Title")

## Warning
Using mods is not allowed by the VRChat Terms of Service and can lead to your account being banned. **Mods are provided without any warranty and as-is**, you have been warned.

## Camera★Remote

Allows to control the camera like a drone, it will fly under your control remotely. To enable this, there is a button "Remote" under the Camera QuickMenu page, a cube will spawn, can grab it and move it to move the camera.

## Camera★Instants

Spawn little vignettes of photo you take in-game. Mimicking the old good instant camera which gave you the photo in a few seconds. Also mimicking Neos VR.

## PickupLib

For developpers. Library that helps spawn and control VRC Pickup.

## Dakytils

For developpers. Library with lots of useful utilities.

## Building
To build yourself, copy all required .dll libraries listed in `Directory.Build.props` into Libs/ folder. Basically all from `<vrchat dir>/MelonLoader/Managed` and also Melonloader.dll above it. Then use Visual Studio 2019 or your IDE of choice to build.

## License
With the following exceptions, all mods here are provided under the terms of [GNU GPLv3 license](LICENSE)
