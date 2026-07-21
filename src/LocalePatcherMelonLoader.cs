using System.Collections;
using MelonLoader;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(YunyunLocalePatcher.LocalePatcherMelonLoader), "YunyunLocalePatcher", "1.5.0", "FunMaker", null)]
[assembly: MelonGame("AllianceArts", "Yunyun_Syndrome")]

namespace YunyunLocalePatcher;

public class LocalePatcherMelonLoader : MelonMod, ILocalePatcher
{
    public HarmonyLib.Harmony harmony = null;

    public override void OnInitializeMelon()
    {
        LocalePatcherCore.OnInitialize(this);
    }

    public HarmonyLib.Harmony GetHarmony()
    {
        if (this.harmony == null) this.harmony = new HarmonyLib.Harmony("com.funmaker.yunyunpatch");
        return this.harmony;
    }

    public void StartCoroutine(IEnumerator routine)
    {
        MelonCoroutines.Start(routine);
    }

    public string GetUserDataDir()
    {
        return MelonEnvironment.UserDataDirectory;
    }

    public void Log(string message)
    {
        MelonLogger.Msg(message);
    }

    public void LogWarning(string message)
    {
        MelonLogger.Warning(message);
    }

    public void LogError(string message)
    {
        MelonLogger.Error(message);
    }
}
