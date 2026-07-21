using System.Collections;

public interface ILocalePatcher {
    public HarmonyLib.Harmony GetHarmony();
    public void StartCoroutine(IEnumerator routine);
    public string GetUserDataDir();
    public void Log(string message);
    public void LogWarning(string message);
    public void LogError(string message);
}
