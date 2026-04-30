# Yunyun Denpa Syndrome Translation Mod

[![Release](https://github.com/YYDS-EN-Fanslation/yunyun-syndrome-mod/actions/workflows/release.yaml/badge.svg)](https://github.com/YYDS-EN-Fanslation/yunyun-syndrome-mod/releases)

This is a mod for game [Yunyun Syndrome!? Rhythm Psychosis](https://store.steampowered.com/app/2914150/Yunyun_Syndrome_Rhythm_Psychosis/)
from [YYDS EN Fanslation Project](https://github.com/YYDS-EN-Fanslation). It loads and applies patches which can modify ingame texts and scenes in order
to tweak, change and fix game's localization. The current release version of the mod come with MTL translations, but will eventually be updated with
human translation provided by YYDS EN Fanslation Project.

Looking for something else?
- [Yunyun Syndrome Patch](https://github.com/YYDS-EN-Fanslation/yunyun-syndrome-patch) - Alternative patcher with simple installer
- [Yunyun Syndrome Translation](https://github.com/YYDS-EN-Fanslation/yunyun-syndrome-translation) - Repository with the newest translation files
- [YYDS EN Fanslation Project](https://github.com/YYDS-EN-Fanslation) - More information about YYDS EN Fanslation Project
- [YYDS EN Fanslation Discord](https://discord.com/invite/Pd3CWA8BfD) - Our Discord

## How to Install

1. Install [Melonloader](https://github.com/LavaGang/MelonLoader.Installer/tree/master#melonloader-installation)
    1. Go to [Releases](https://github.com/LavaGang/MelonLoader.Installer/releases)
    2. Download the newest **Melonloader.Installer.exe**
    3. Choose "Yunyun Syndrome!? Rhythm Psychosis"
    4. Press "Install"
2. Install [YunyunLocalePatcher](https://github.com/YYDS-EN-Fanslation/yunyun-syndrome-mod)
    1. Go to [Releases](https://github.com/YYDS-EN-Fanslation/yunyun-syndrome-mod/releases)
    2. Download the newest **YunyunLocalePatcher.zip**
    3. Extract files
    4. Copy contents into game folder, make sure `Mods` and `UserData` folders are merged and the `YunyunLocalePatcher.dll` ended up
       in `Yunyun_Syndrome\Mods` directory. 
3. (optional) Add/remove locale patches in `Yunyun_Syndrome\UserData\LocalePatches` directory. For example:
    - [Radish](https://github.com/Radish-sys)'s MTL: [JP -> EN](https://raw.githubusercontent.com/YYDS-EN-Fanslation/yunyun-syndrome-translation/refs/heads/master/YYDS%20EN%20Fanslation%20-%20MTL%20Patch.csv)
    - Google Translate MTL: [JP -> EN](https://raw.githubusercontent.com/funmaker/YunyunLocalePatcher/refs/heads/master/examples/20-english-mtl.csv) (no dialogues)
    - [Moshi Moshi](https://github.com/lIllIIlI)'s: [JP -> EN](https://raw.githubusercontent.com/funmaker/YunyunLocalePatcher/refs/heads/master/examples/20-faithful-english.csv) ([#2](https://github.com/funmaker/YunyunLocalePatcher/pull/2)) (no dialogues)

## How to make patches?

- (optional) Once you have mod installed, you can run the game with `--localepatcher.dumpstrings` launch option. This will create `00-base.csv`
  file in `Yunyun_Syndrome\UserData\LocalePatches` which will contain all the translation related strings.
- **Make sure to remove `--localepatcher.dumpstrings` flag from launch options and delete/move `00-base.csv` file!** YunyunLocalePatcher will
  not load any patches if that flag is present in launch options!
- You can import `00-base.csv` into this [spreadsheet](https://docs.google.com/spreadsheets/d/1nKseRzV7VLbYQeV79oiWpTRxfSj_n8xqap94Bf6t2I4/edit?gid=0#gid=0&fvid=826278446)
  to make it editing easier.
    - Make sure to select `A1` cell(contains "TableName"), `Replace data at selected cell` in `Import location`, `Comma` in `Separator type` 
      and uncheck `Convert text to numbers, dates and formulas`
- Make your changes by editing `New Text` column in `Editor` tab. All your changes should be reflected in `Patch Export` tab. In order to create
  a patch, go to `Patch Export` tab and save it as `Comma-Separated Values (CSV)`.
- Technical note: YunyunLocalePatcher expects patches to be in CSV format([RFC 4180](https://www.rfc-editor.org/rfc/rfc4180.html)).
  It should contain exactly 3 columns(TableName, Key, Text), just like the `00-base.csv` file generated in first step.

## How does it work?

This mod simply hooks into Unity Localization using ITablePostprocessor interface and modifies StringTables as they are loaded.
The patches loaded from `Yunyun_Syndrome\UserData\LocalePatches\*.csv` are applied in alphabetical order, so you can use names
like `10-initial.csv`, `50-common.csv`, `90-extra.csv`, etc to control the order in which patches are applied.
