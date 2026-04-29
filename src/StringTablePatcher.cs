using MelonLoader;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace YunyunLocalePatcher;

[Serializable]
public class StringTablePatcher : ITablePostprocessor
{
    public void PostprocessTable(LocalizationTable table)
    {
        if (table is not StringTable stringTable) return;
        
        try
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
        catch (Exception ex)
        {
            MelonLogger.Error(ex);
        }
    }
}
