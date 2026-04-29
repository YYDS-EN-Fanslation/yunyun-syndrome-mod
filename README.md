# YunyunLocalePatcher

[![Release](https://github.com/funmaker/YunyunLocalePatcher/actions/workflows/release.yaml/badge.svg)](https://github.com/funmaker/YunyunLocalePatcher/releases)

This mod allows you to patch locale strings in order to tweak localization, translation texts, etc.

## How to Install

1. Install [Melonloader](https://github.com/LavaGang/MelonLoader.Installer/tree/master#melonloader-installation)
    1. Go to [Releases](https://github.com/LavaGang/MelonLoader.Installer/releases)
    2. Download the newest **Melonloader.Installer.exe**
    3. Choose "Yunyun Syndrome!? Rhythm Psychosis"
    4. Press "Intall"
2. Install [YunyunLocalePatcher](https://github.com/funmaker/YunyunLocalePatcher)
    1. Go to [Releases](https://github.com/funmaker/YunyunLocalePatcher/releases)
    2. Download the newest **YunyunLocalePatcher.zip**
    3. Extract files
    4. Copy contents into game folder, make sure `Mods` and `UserData` folders are merged and the `YunyunLocalePatcher.dll` ended up
       in `Yunyun_Syndrome\Mods` directory. 
3. Put any locale patches in `Yunyun_Syndrome\UserData\LocalePatches` directory. For example:
    - Google Translate MTL: [JP -> EN](https://raw.githubusercontent.com/funmaker/YunyunLocalePatcher/refs/heads/master/examples/20-english-mtl.csv)
    - [Moshi Moshi](https://github.com/lIllIIlI)'s: [more faithful JP -> EN](https://raw.githubusercontent.com/funmaker/YunyunLocalePatcher/refs/heads/master/examples/20-faithful-english.csv) ([#2](https://github.com/funmaker/YunyunLocalePatcher/pull/2))

## How to make patches?

- Clone [this spreadsheet](https://docs.google.com/spreadsheets/d/1nKseRzV7VLbYQeV79oiWpTRxfSj_n8xqap94Bf6t2I4/edit?gid=0#gid=0&fvid=826278446).
  It contains all the translation strings as of **Ver.1.0.6**.
- (optional) Once you have mod installed, you can run the game with `--localepatcher.dumpstrings` launch option. This will create `00-base.csv`
  file in `Yunyun_Syndrome\UserData\LocalePatches` which will contain all the translation related strings. You can update the spreadsheet with
  newer strings by importing it in the spreadsheet.
    - Make sure to select `A1` cell(contains "TableName"), `Replace data at selected cell` in `Import location`, `Comma` in `Separator type` 
      and uncheck `Convert text to numbers, dates and formulas`
- **Make sure to remove `--localepatcher.dumpstrings` flag from launch options and delete/move `00-base.csv` file!** YunyunLocalePatcher will
  not load any patches if that flag is present in launch options!
- Make your changes by editing `New Text` column in `Editor` tab. All your changes should be reflected in `Patch Export` tab. In order to create
  a patch, go to `Patch Export` tab and save it as `Comma-Separated Values (CSV)`.
- Technical note: YunyunLocalePatcher expects patches to be in CSV format([RFC 4180](https://www.rfc-editor.org/rfc/rfc4180.html)).
  It should contain exactly 3 columns(TableName, Key, Text), just like the `00-base.csv` file generated in first step.

## How does it work?

This mod simply hooks into Unity Localization using ITablePostprocessor interface and modifies StringTables as they are loaded.
The patches loaded from `Yunyun_Syndrome\UserData\LocalePatches\*.csv` are applied in alphabetical order, so you can use names
like `10-initial.csv`, `50-common.csv`, `90-extra.csv`, etc to control the order in which patches are applied.
