using System.Collections;
using System.Text;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

[assembly: MelonInfo(typeof(YunyunLocalePatcher.Core), "YunyunLocalePatcher", "1.0.0", "FunMaker", null)]
[assembly: MelonGame("AllianceArts", "Yunyun_Syndrome")]

namespace YunyunLocalePatcher
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            string dataDir = Path.Combine(MelonEnvironment.UserDataDirectory, "LocalePatches");

            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("--localepatcher.dumpstrings"))
            {
                string dumpPath = Path.Combine(dataDir, "00-base.csv");
                MelonLogger.Msg($"--localepatcher.dumpstrings has been passed to the game. Dumping all translation strings to ${dumpPath}");
                MelonCoroutines.Start(DumpAllStringTables(dumpPath));
                MelonLogger.Warning($"YunyunLocalePatcher will not patch any locales!");
                MelonLogger.Warning($"Remove --localepatcher.dumpstrings from launch options to enable patching");
                return;
            }

            MelonLogger.Msg($"Loading patches from {dataDir}");

            var patches = new PatchFile();
            int count = 0;
            if (Directory.Exists(dataDir))
            {
                var patchFiles = Directory.GetFiles(dataDir, "*");
                Array.Sort(patchFiles);

                foreach (var file in patchFiles)
                {
                    string fileName = Path.GetFileName(file);
                    try
                    {
                        MelonLogger.Msg($"Loading {fileName}");
                        var patchFile = PatchFile.Load(file);
                        patches.Append(patchFile);
                        count += 1;
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
            MelonLogger.Msg($"Loaded {count} patch files, {patches.Count} entries in total");

            if (patches.Count == 0)
            {
                MelonLogger.Warning("Nothing to patch! Quitting.");
                return;
            }

            var settings = LocalizationSettings.Instance;
            if (settings != null && settings.GetStringDatabase().TablePostprocessor == null)
            {
                settings.GetStringDatabase().TablePostprocessor = new TablePatcher(patches);
                MelonLogger.Msg("Table postprocessor registered.");

                MelonLogger.Msg("Intialization successful.");
            }
            else
            {
                MelonLogger.Error("Table postprocessor is already registered. YunyunLocalePatcher will not work.");
            }
        }

        public static IEnumerator DumpAllStringTables(string outputPath)
        {
            yield return LocalizationSettings.InitializationOperation;

            var locales = LocalizationSettings.AvailableLocales.Locales;
            var entries = new List<string[]>();

            foreach (var locale in locales)
            {
                MelonLogger.Msg($"Dumping locale: {locale.LocaleName}");

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
                            ]);
                        }
                    }
                }
            }

            entries.Sort((a, b) =>
            {
                int cmp = string.CompareOrdinal(a[1], b[1]);
                if (cmp != 0) return cmp;
                return string.CompareOrdinal(a[0], b[0]);
            });

            var dump = new StringBuilder();
            dump.AppendLine(Csv.SerializeLine(["TableName", "Key", "Text"]));
            foreach (var row in entries)
                dump.AppendLine(Csv.SerializeLine(row));

            File.WriteAllText(outputPath, dump.ToString());

            MelonLogger.Msg($"Dumped {entries.Count} entries to {outputPath}");
        }
    }

    [System.Serializable]
    public class TablePatcher : ITablePostprocessor
    {
        private PatchFile patch;

        public TablePatcher(PatchFile patch)
        {
            this.patch = patch;
        }

        public void PostprocessTable(LocalizationTable table)
        {
            if (table is StringTable stringTable)
            {
                string tableName = stringTable.name;
                int count = 0;
                MelonLogger.Msg($"Patching string table: {tableName}...");

                foreach (var entry in stringTable.Values)
                {
                    string patchedText = this.patch[tableName, entry.Key];
                    if (patchedText != null)
                    {
                        entry.Value = patchedText;
                        count += 1;
                    }
                }

                MelonLogger.Msg($"Patched {count} entries in {tableName}.");
            }
        }
    }
}