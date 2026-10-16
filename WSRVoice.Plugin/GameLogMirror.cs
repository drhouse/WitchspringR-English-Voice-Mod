using System;
using BepInEx.Logging;

/// <summary>
/// Mirrors the game's own Unity log messages into the plugin's log file.
///
/// Diagnosing a pronunciation error means knowing which line was speaking at
/// the moment something sounded wrong, and the game's scene markers are the
/// only landmark a listener can actually report ("it happened when CameraSync :
/// cameraPoint_picture_5_1 appeared"). Those markers go to BepInEx's own log,
/// which carries no timestamps, while the HIT lines that name the clip are in
/// this plugin's log, which does. Correlating two files on one missing clock is
/// guesswork.
///
/// So the Unity messages are copied into the plugin's log instead: one file,
/// one clock, scene markers and clip ids interleaved in the order they
/// happened. A marker then pins the spoken line to the millisecond.
///
/// Implemented as a BepInEx log listener rather than by subscribing to
/// Application.logMessageReceived, because BepInEx is already listening
/// (UnityLogListening) and a managed listener needs no delegate marshalled
/// across the Il2CppInterop boundary.
/// </summary>
public sealed class GameLogMirror : ILogListener
{
    readonly Action<string> _write;

    public GameLogMirror(Action<string> write)
    {
        _write = write;
    }

    // Everything the game says, at whatever level it says it: the camera and
    // scene markers that make this useful arrive as plain Message entries, so
    // filtering by severity would discard exactly the lines we want.
    public LogLevel LogLevelFilter => LogLevel.All;

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        try
        {
            // Only the game's own messages. Without this the plugin's own log
            // lines, which BepInEx also carries, would be written back into the
            // file a second time.
            var source = eventArgs?.Source?.SourceName;
            if (source != "Unity") return;

            var data = eventArgs.Data?.ToString();
            if (string.IsNullOrEmpty(data)) return;

            _write($"GAME {data}");
        }
        catch
        {
            // A diagnostic must never be what breaks playback.
        }
    }

    public void Dispose() { }
}
