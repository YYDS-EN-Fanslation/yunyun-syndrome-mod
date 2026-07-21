using System.Collections;
using BepInEx;

namespace YunyunLocalePatcher;

[BepInPlugin("YunyunLocalePatcher", "YunyunLocalePatcher", "1.5.0")]
public class LocalePatcherBepInEx : BaseUnityPlugin, ILocalePatcher
{
    public HarmonyLib.Harmony harmony = null;

    public void Awake()
    {
        LocalePatcherCore.OnInitialize(this);
    }

    public HarmonyLib.Harmony GetHarmony()
    {
        if (this.harmony == null) this.harmony = new HarmonyLib.Harmony("com.funmaker.yunyunpatch");
        return this.harmony;
    }

    public new void StartCoroutine(IEnumerator routine)
    {
        base.StartCoroutine(routine);
    }

    public string GetUserDataDir()
    {
        return Path.Combine(Paths.GameRootPath, "UserData");
    }

    public void Log(string message)
    {
        Logger.LogMessage(message);
    }

    public void LogWarning(string message)
    {
        Logger.LogWarning(message);
    }

    public void LogError(string message)
    {
        Logger.LogError(message);
    }
}
