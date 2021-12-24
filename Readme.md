# Rescued by Recolic

This project is recovered from <https://web.archive.org/web/20210609033716/https://github.com/gmastergreatee/ExternalCrosshairOverlay> and <https://github.com/tweakrrr/ECO>. gmastergreatee has deleted his whole github account, RIP. 

**Thanks to @tweakrrr**. Without his fork, we would have lost this awesome crosshair overlay. This is almost the **only tool** to allow you to overlay a custom PNG, without any scaling. This allows you to create a high-quality firing table overlay, to use your sniper rifle like mortar! 

I will help releasing latest binaries, and accepting issues and pull requests, just like the original project. 

```
9 URLs have been captured for this URL prefix.
URL MIME Type   From    To  Captures    Duplicates  Uniques
https://github.com/gmastergreatee/ExternalCrosshairOverlay  text/html   Nov 11, 2020    Jun 9, 2021 2   0   2   
https://github.com/gmastergreatee/ExternalCrosshairOverlay/commit/1e7a18c1d39fc2dd60eb0d4b5a33ed25707bf3ce/rollup?direction=sw  text/html   Jun 9, 2021 Jun 9, 2021 1   0   1   
https://github.com/gmastergreatee/ExternalCrosshairOverlay/contributors_list?count=2¤t_repository=ExternalCrosshairOverlay&items_to_show=2  text/html   Jun 9, 2021 Jun 9, 2021 1   0   1   
https://github.com/gmastergreatee/ExternalCrosshairOverlay/file-list/master text/html   Jun 9, 2021 Jun 9, 2021 1   0   1   
https://github.com/gmastergreatee/ExternalCrosshairOverlay/overview_actions/master  text/html   Jun 9, 2021 Jun 9, 2021 1   0   1   
https://github.com/gmastergreatee/ExternalCrosshairOverlay/releases/latest  text/html   Jun 9, 2021 Jun 9, 2021 1   0   1   
https://github.com/gmastergreatee/ExternalCrosshairOverlay/releases/tag/0.2.18  text/html   Jun 9, 2021 Jun 9, 2021 1   0   1   
https://github.com/gmastergreatee/ExternalCrosshairOverlay/security/overall-count   text/fragment+html  Jun 9, 2021 Jun 9, 2021 1   0   1   
https://github.com/gmastergreatee/ExternalCrosshairOverlay/used_by_list text/fragment+html  Jun 9, 2021 Jun 9, 2021 1   0   1   
```

Let's go! 

----

# External Crosshair Overlay

[![Build status](https://ci.appveyor.com/api/projects/status/3d1t03v8dpuncpi0?svg=true)](https://ci.appveyor.com/project/gmastergreatee/externalcrosshairoverlay)
[![PayPal Donate](https://img.shields.io/badge/donate-PayPal-orange.svg?style=flat-square&logo=paypal)](https://www.paypal.me/RajarshiVaidya)

Credits : gmastergreatee

Simple external crosshair overlay, requires .Net v4.5

In some games like CSGO, this will work on fullscreen-windowed/windowed mode only.
Whereas in games like UT2004, all display modes will be supported.

Download [here](https://github.com/gmastergreatee/ExternalCrosshairOverlay/releases/latest).

## Features

- [x] custom sized crosshair
- [x] choose crosshair color(for primary crosshair only)
- [x] change crosshair transparency
- [x] display crosshair on selected process
- [x] entirely in C# (no DirectX dependencies)
- [x] supports custom crosshair option(png/jpeg images - other formats experimental)
- [x] set the crosshair offset manually(__Hotkey : "-"(Minus key) [Movement: Arrows/WASD]__)
- [x] saving of configuration based on the target game
- [x] toggle display of crosshair while in-game using "=" key
- [x] DPI aware

#### Note : Configs are stored at `%APPDATA%\CrosshairOverlay` folder. Delete files to hard-reset the config.

### Crosshair Scaling by lower values

To make minor adjustments, focus the slider by clicking it, then press the buttons left/right arrow keys(or up/down) 
It should change by 0.01 points in that case, whereas clicking on either side of the slider with mouse should change by 0.1 points

### Notice (16 Jan, 2019)
__ECO__ was developed as a prototype. Therefore, there will be no further enhancements to the app __AFTER__ milestone 0.2.2 is reached.

__Feature requests are still welcome.__
