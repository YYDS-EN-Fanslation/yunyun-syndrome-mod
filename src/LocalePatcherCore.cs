using System.Collections;
using System.Text;
using ANovel.Core;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

[assembly: MelonInfo(typeof(YunyunLocalePatcher.LocalePatcherCore), "YunyunLocalePatcher", "1.3.0", "FunMaker", null)]
[assembly: MelonGame("AllianceArts", "Yunyun_Syndrome")]

namespace YunyunLocalePatcher;

public class LocalePatcherCore : MelonMod
{
    public static PatchFile patches;
    public static string patchesRoot = Path.Combine(MelonEnvironment.UserDataDirectory, "LocalePatches");
    private HarmonyLib.Harmony _harmony;

    public override void OnInitializeMelon()
    {
        string[] args = Environment.GetCommandLineArgs();
        if (args.Contains("--localepatcher.dumpstrings"))
        {
            string dumpName = "00-base";
            MelonLogger.Msg($"--localepatcher.dumpstrings has been passed to the game. Dumping all translation strings to ${Path.Combine(patchesRoot, dumpName)}.csv");
            MelonCoroutines.Start(DumpAllStrings(dumpName));
            return;
        }

        var patches = LocalePatcherCore.patches = LoadAllPatches();
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

        TablePatcher tablePatcher = new TablePatcher();

        settings.GetStringDatabase().TablePostprocessor = tablePatcher;
        MelonLogger.Msg("StringTable postprocessor registered.");

        settings.GetAssetDatabase().TablePostprocessor = tablePatcher;
        MelonLogger.Msg("AssetTable postprocessor registered.");

        this._harmony = new HarmonyLib.Harmony("com.funmaker.yunyunpatch");
        this._harmony.PatchAll();
        MelonLogger.Msg("TextAsset patch registered.");

        MelonLogger.Msg("Intialization complete.");
    }

    private PatchFile LoadAllPatches()
    {
        MelonLogger.Msg($"Loading patches from {patchesRoot}");

        var patches = new PatchFile();
        int fileCount = 0;
        if (Directory.Exists(patchesRoot))
        {
            var patchFiles = Directory.GetFiles(patchesRoot, "*");
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
            Directory.CreateDirectory(patchesRoot);
        }

        MelonLogger.Msg($"Loaded {fileCount} patch files, {patches.Count} entries in total");

        return patches;
    }

    private IEnumerator DumpAllStrings(string dumpName)
    {
        yield return LocalizationSettings.InitializationOperation;

        Directory.CreateDirectory(Path.Combine(patchesRoot, dumpName));

        var locales = LocalizationSettings.AvailableLocales.Locales;
        var entries = new List<string[]>();

        foreach (var locale in locales)
        {
            MelonLogger.Msg($"Dumping StringTables for Locale: {locale.LocaleName}");

            var handle = LocalizationSettings.StringDatabase.GetAllTables(locale);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                MelonLogger.Warning($"Failed to load string tables for {locale.LocaleName}");
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
                            entry.KeyId.ToString(),
                        ]);
                    }
                }
            }
        }

        foreach (var locale in locales)
        {
            MelonLogger.Msg($"Dumping AssetTables for Locale: {locale.LocaleName}");

            var handle = LocalizationSettings.AssetDatabase.GetAllTables(locale);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                MelonLogger.Warning($"Failed to load asset tables for {locale.LocaleName}");
                continue;
            }

            foreach (var table in handle.Result)
            {
                if (table is AssetTable at)
                {
                    foreach (var entry in at.Values)
                    {
                        var assetHandle = at.GetAssetAsync<UnityEngine.Object>(entry.Key);
                        yield return assetHandle;

                        if (assetHandle.Status != AsyncOperationStatus.Succeeded)
                        {
                            MelonLogger.Warning($"Failed to load asset for key '{entry.Key}' in table '{at.name}'");
                            continue;
                        }

                        UnityEngine.Object asset = assetHandle.Result;
                        if (asset == null) continue;

                        Texture2D texture = null;
                        if (asset is Texture2D tex) texture = LocalePatcherCore.GetReadableTexture(tex);
                        else if (asset is Sprite sprite) texture = LocalePatcherCore.GetReadableSprite(sprite);
                        else
                        {
                            MelonLogger.Msg($"Asset '{entry.Key}' is not a Texture2D or Sprite, skipping.");
                            continue;
                        }

                        byte[] pngData = ImageConversion.EncodeToPNG(texture);

                        string path = $"{dumpName}/{at.name}_{entry.Key}.png";
                        File.WriteAllBytes(Path.Combine(patchesRoot, path), pngData);

                        entries.Add([
                            at.name,
                            entry.Key,
                            path,
                            entry.KeyId.ToString(),
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
                                localizeData.Keys[i] + "/" + locale.Language,
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

        File.WriteAllText(Path.Combine(patchesRoot, $"{dumpName}.csv"), dump.ToString());

        MelonLogger.Msg($"Dumped {entries.Count} entries to {dumpName}.csv");

        MelonLogger.Warning($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        MelonLogger.Warning($"!              YunyunLocalePatcher will not patch any locales!              !");
        MelonLogger.Warning($"! Remove --localepatcher.dumpstrings from launch options to enable patching !");
        MelonLogger.Warning($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }

    private static Texture2D GetReadableTexture(Texture2D source)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture rt = null;

        try
        {
            rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(source, rt);
            RenderTexture.active = rt;

            Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readable.Apply();

            return readable;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
        }
    }

    // From https://gamedev.stackexchange.com/a/214819
    private static Texture2D GetReadableSprite(Sprite source)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture rt = null;
        GameObject spriteGO = null;
        GameObject camGO = null;

        try
        {
            int width = (int)source.rect.width;
            int height = (int)source.rect.height;
            int renderLayer = 30; // Assuming layer 30 is unused, we use it to mask out our sprite

            // Setup temporary GameObject with SpriteRenderer
            spriteGO = new GameObject("TempSpriteRenderer");
            var spriteRenderer = spriteGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = source;
            spriteGO.layer = renderLayer;
            spriteGO.transform.position = Vector3.zero;

            // Setup temporary camera - orthographic, so we can control size easily
            camGO = new GameObject("TempCamera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.orthographic = true;
            cam.cullingMask = 1 << renderLayer;
            cam.orthographicSize = height / source.pixelsPerUnit / 2f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.tag = "MainCamera";

            // Create RenderTexture and render
            rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            rt.filterMode = FilterMode.Point;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D readable = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readable.Apply();
            RenderTexture.active = previous;

            return readable;
        }
        finally
        {
            // Cleanup temporary objects
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            UnityEngine.Object.DestroyImmediate(spriteGO);
            UnityEngine.Object.DestroyImmediate(camGO);
        }
    }
}