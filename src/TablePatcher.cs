using MelonLoader;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace YunyunLocalePatcher;

[Serializable]
public class TablePatcher : ITablePostprocessor
{
    public void PostprocessTable(LocalizationTable table)
    {
        MelonLogger.Msg($"Patching table: {table.name}...");
        MelonLogger.Msg($"!!!!!!!!!!!!!!!!!");
        MelonLogger.Msg($"!!!!!!!!!!!!!!!!!");
        MelonLogger.Msg($"!!!!!!!!!!!!!!!!!");
        MelonLogger.Msg($"!!!!!!!!!!!!!!!!!");

        if (LocalePatcherCore.patches == null) return;

        try
        {
            if (table is StringTable stringTable)
            {
                string tableName = stringTable.name;
                MelonLogger.Msg($"Patching string table: {tableName}...");

                int count = 0;
                foreach (var entry in stringTable.Values)
                {
                    string patchedText = LocalePatcherCore.patches[tableName, entry.Key];
                    if (patchedText != null)
                    {
                        entry.Value = patchedText;
                        count += 1;
                    }
                }

                MelonLogger.Msg($"Patched {count} entries in {tableName}.");
            }
            else if (table is AssetTable assetTable)
            {
                string tableName = assetTable.name;
                MelonLogger.Msg($"Patching asset table: {tableName}...");

                int count = 0;
                foreach (var entry in assetTable.Values)
                {
                    string assetPath = LocalePatcherCore.patches[tableName, entry.Key];
                    if (assetPath != null)
                    {
                        assetPath = Path.Combine(LocalePatcherCore.patchesRoot, assetPath);
                        if (!File.Exists(assetPath))
                        {
                            MelonLogger.Warning($"File not found: {assetPath} (replacing {entry.Key} for {tableName})");
                            continue;
                        }

                        byte[] imageData = File.ReadAllBytes(assetPath);
                        Texture2D texture = new Texture2D(2, 2); // dimensions will be replaced by LoadImage
                        if (!texture.LoadImage(imageData))
                        {
                            MelonLogger.Warning($"Failed to load image: {assetPath} (replacing {entry.Key} for {tableName})");
                            UnityEngine.Object.Destroy(texture);
                            continue;
                        }

                        Sprite sprite = Sprite.Create(
                            texture,
                            new Rect(0, 0, texture.width, texture.height),
                            Vector2.one * 0.5f
                        );

                        entry.SetAssetOverride(sprite);
                        count += 1;
                    }
                }

                MelonLogger.Msg($"Patched {count} entries in {tableName}.");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error(ex);
        }
    }
}
