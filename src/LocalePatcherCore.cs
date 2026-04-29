using System.Collections;
using System.Text;
using ANovel.Core;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

[assembly: MelonInfo(typeof(YunyunLocalePatcher.LocalePatcherCore), "YunyunLocalePatcher", "1.1.0", "FunMaker", null)]
[assembly: MelonGame("AllianceArts", "Yunyun_Syndrome")]

namespace YunyunLocalePatcher;

public class LocalePatcherCore : MelonMod
{
    public static PatchFile patches;
    private HarmonyLib.Harmony _harmony;

    public override void OnInitializeMelon()
    {
        string dataDir = Path.Combine(MelonEnvironment.UserDataDirectory, "LocalePatches");

        string[] args = Environment.GetCommandLineArgs();
        if (args.Contains("--localepatcher.dumpstrings"))
        {
            string dumpPath = Path.Combine(dataDir, "00-base.csv");
            MelonLogger.Msg($"--localepatcher.dumpstrings has been passed to the game. Dumping all translation strings to ${dumpPath}");
            MelonCoroutines.Start(DumpAllStrings(dumpPath));
            return;
        }

        var patches = LocalePatcherCore.patches = LoadAllPatches(dataDir);
        if (patches.Count == 0)
        {
            MelonLogger.Warning("Nothing to patch! Quitting.");
            return;
        }

        var settings = LocalizationSettings.Instance;
        if (settings == null || settings.GetStringDatabase().TablePostprocessor != null)
        {
            MelonLogger.Error("Table postprocessor is already registered. YunyunLocalePatcher will not work.");
            return;
        }

        settings.GetStringDatabase().TablePostprocessor = new StringTablePatcher();
        MelonLogger.Msg("StringTable postprocessor registered.");

        this._harmony = new HarmonyLib.Harmony("com.funmaker.yunyunpatch");
        this._harmony.PatchAll();
        MelonLogger.Msg("TextAsset patch registered.");

        MelonLogger.Msg("Intialization complete.");
    }

    private PatchFile LoadAllPatches(string dataDir)
    {
        MelonLogger.Msg($"Loading patches from {dataDir}");

        var patches = new PatchFile();
        int fileCount = 0;
        if (Directory.Exists(dataDir))
        {
            var patchFiles = Directory.GetFiles(dataDir, "*");
            Array.Sort(patchFiles);

            foreach (var file in patchFiles)
            {
                string fileName = Path.GetFileName(file);
                try
                {
                    if (fileName == "00-base.csv")
                    {
                        MelonLogger.Warning($"Skipping 00-base.csv. Have you forgotten to remove it?");
                        continue;
                    }

                    MelonLogger.Msg($"Loading {fileName}");
                    var patchFile = PatchFile.Load(file);
                    patches.Append(patchFile);
                    fileCount += 1;
                    MelonLogger.Msg($"Loaded {fileName} ({patches.Count} entries)");
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Couldn't load {fileName}: {ex.Message}");
                }
            }
        }
        else
        {
            MelonLogger.Warning($"Directory doesn't exist. Creating.");
            Directory.CreateDirectory(dataDir);
        }

        MelonLogger.Msg($"Loaded {fileCount} patch files, {patches.Count} entries in total");

        return patches;
    }

    private IEnumerator DumpAllStrings(string outputPath)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        var entries = new List<string[]>();

        foreach (var locale in locales)
        {
            MelonLogger.Msg($"Dumping StringTables for Locale: {locale.LocaleName}");

            var handle = LocalizationSettings.StringDatabase.GetAllTables(locale);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                MelonLogger.Warning($"Failed to load tables for {locale.LocaleName}");
                continue;
            }

            foreach (var table in handle.Result)
            {
                if (table is StringTable st)
                {
                    foreach (var entry in st.Values)
                    {
                        entries.Add([
                            st.name,
                            entry.Key,
                            entry.Value,
                            // entry.KeyId.ToString(),
                        ]);
                    }
                }
            }
        }

        UnityEngine.Object[] textAssets = Resources.LoadAll("/", typeof(TextAsset));
        foreach (var asset in textAssets)
        {
            if (asset is not TextAsset textAsset)
                continue;

            if (textAsset.name.EndsWith(".lang"))
            {
                MelonLogger.Msg($"Dumping Event lines for: {textAsset.name}");

                try
                {
                    LocalizeData localizeData = JsonUtility.FromJson<LocalizeData>((textAsset as TextAsset).text);

                    foreach (var locale in localizeData.List)
                    {
                        for (var i = 0; i < locale.Lines.Length; i++)
                        {
                            entries.Add([
                                textAsset.name,
                                locale.Language + "/" + i,
                                locale.Lines[i],
                                // localizeData.Keys[i] + "/" + locale.Language,
                            ]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error(ex);
                }
            }
        }

        entries.Sort((a, b) =>
        {
            int cmp = string.CompareOrdinal(a[0], b[0]);
            if (cmp != 0) return cmp;
            return string.CompareOrdinal(a[1], b[1]);
        });

        var dump = new StringBuilder();
        dump.AppendLine(Csv.SerializeLine(["TableName", "Key", "Text", /* "KeyId" */]));
        foreach (var row in entries)
            dump.AppendLine(Csv.SerializeLine(row));

        File.WriteAllText(outputPath, dump.ToString());

        MelonLogger.Msg($"Dumped {entries.Count} entries to {outputPath}");

        MelonLogger.Warning($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        MelonLogger.Warning($"!              YunyunLocalePatcher will not patch any locales!              !");
        MelonLogger.Warning($"! Remove --localepatcher.dumpstrings from launch options to enable patching !");
        MelonLogger.Warning($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }
}
