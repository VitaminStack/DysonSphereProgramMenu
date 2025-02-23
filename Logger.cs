using BepInEx.Logging;
using System.Collections.Generic;
using UnityEngine;

public static class VitaminLogger
{
    private static ManualLogSource _logger;
    private static HashSet<string> loggedMessages = new HashSet<string>(); // Einmalige Meldungen
    private static Dictionary<string, float> lastLoggedTime = new Dictionary<string, float>(); // Zeitbasierte Meldungen
    private static int lastGameTick = -1; // GameTick-Tracking

    static VitaminLogger()
    {
        _logger = BepInEx.Logging.Logger.CreateLogSource("VitaminLogger");
    }

    /// <summary>
    /// Gibt eine Log-Info aus. Kann auf einmalig, pro Tick oder nach Zeitintervall begrenzt werden.
    /// </summary>
    public static void LogInfo(string message, bool once = false, bool perTick = false, float minInterval = 0f)
    {
        if (ShouldSkipMessage(message, once, perTick, minInterval)) return;
        _logger.LogInfo(message);
    }

    public static void LogWarning(string message, bool once = false, bool perTick = false, float minInterval = 0f)
    {
        if (ShouldSkipMessage(message, once, perTick, minInterval)) return;
        _logger.LogWarning(message);
    }

    public static void LogError(string message, bool once = false, bool perTick = false, float minInterval = 0f)
    {
        if (ShouldSkipMessage(message, once, perTick, minInterval)) return;
        _logger.LogError(message);
    }

    /// <summary>
    /// Prüft, ob eine Nachricht basierend auf den gewählten Parametern übersprungen werden soll.
    /// </summary>
    private static bool ShouldSkipMessage(string message, bool once, bool perTick, float minInterval)
    {
        int currentGameTick = (int)GameMain.gameTick;
        float currentTime = Time.time;

        // Verhindert mehrfaches Logging derselben Nachricht
        if (once && loggedMessages.Contains(message)) return true;

        // Verhindert mehrfaches Logging pro Tick
        if (perTick && lastGameTick == currentGameTick) return true;

        // Verhindert mehrfaches Logging innerhalb eines bestimmten Zeitintervalls
        if (minInterval > 0f && lastLoggedTime.TryGetValue(message, out float lastTime) && (currentTime - lastTime < minInterval))
            return true;

        // Falls "once" aktiviert ist, merken wir uns die Nachricht
        if (once) loggedMessages.Add(message);

        // Falls "perTick" aktiviert ist, merken wir uns den aktuellen Game-Tick
        if (perTick) lastGameTick = currentGameTick;

        // Falls minInterval gesetzt ist, speichern wir den Zeitpunkt der letzten Ausgabe
        if (minInterval > 0f) lastLoggedTime[message] = currentTime;

        return false;
    }

    /// <summary>
    /// Löscht gespeicherte einmalige Logs und Zeit-Logs.
    /// </summary>
    public static void ClearLoggedMessages()
    {
        loggedMessages.Clear();
        lastLoggedTime.Clear();
    }
}
