using ANovel.Core;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace YunyunLocalePatcher;

[HarmonyPatch(typeof(TextAsset), "text", MethodType.Getter)]
public static class TextAssetPatcher
{
    static void Postfix(TextAsset __instance, ref string __result)
    {
        if (LocalePatcherCore.patches == null) return;
        if (!__instance.name.EndsWith(".lang")) return;

        try
        {
            MelonLogger.Msg($"Patching text asset: {__instance.name}...");

            LocalizeData localizeData = JsonUtility.FromJson<LocalizeData>(__result);

            int count = 0;
            foreach (var locale in localizeData.List)
            {
                for (var i = 0; i < locale.Lines.Length; i++)
                {
                    string patchedText = LocalePatcherCore.patches[__instance.name, locale.Language + "/" + i];
                    if (patchedText != null)
                    {
                        locale.Lines[i] = patchedText;
                        count += 1;
                    }
                }
            }

            MelonLogger.Msg($"Patched {count} entries in {__instance.name}.");

            if (count > 0)
            {
                __result = JsonUtility.ToJson(localizeData);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error(ex);
        }
    }
}
