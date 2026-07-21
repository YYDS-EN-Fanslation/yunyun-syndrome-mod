
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace YunyunLocalePatcher;

[Serializable]
public class TablePatcher : ITablePostprocessor
{
    public void PostprocessTable(LocalizationTable table)
    {
        LocalePatcherCore.Log($"Patching table: {table.name}...");

        if (LocalePatcherCore.patches == null) return;

        try
        {
            if (table is StringTable stringTable)
            {
                string tableName = stringTable.name;
                LocalePatcherCore.Log($"Patching string table: {tableName}...");

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

                LocalePatcherCore.Log($"Patched {count} entries in {tableName}.");
            }
            else if (table is AssetTable assetTable)
            {
                string tableName = assetTable.name;
                LocalePatcherCore.Log($"Patching asset table: {tableName}...");

                int count = 0;
                foreach (var entry in assetTable.Values)
                {
                    string assetPath = LocalePatcherCore.patches[tableName, entry.Key];
                    if (assetPath != null)
                    {
                        assetPath = Path.Combine(LocalePatcherCore.patchesRoot, assetPath);
                        if (!File.Exists(assetPath))
                        {
                            LocalePatcherCore.LogWarning($"File not found: {assetPath} (replacing {entry.Key} for {tableName})");
                            continue;
                        }

                        byte[] imageData = File.ReadAllBytes(assetPath);
                        Texture2D texture = new Texture2D(2, 2); // dimensions will be replaced by LoadImage
                        if (!texture.LoadImage(imageData))
                        {
                            LocalePatcherCore.LogWarning($"Failed to load image: {assetPath} (replacing {entry.Key} for {tableName})");
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

                LocalePatcherCore.Log($"Patched {count} entries in {tableName}.");
            }
        }
        catch (Exception ex)
        {
            LocalePatcherCore.LogError(ex.ToString());
        }
    }
}
