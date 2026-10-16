using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using WSRVoice.Core;

[BepInPlugin("wsr.englishvoice", "WSR English Voice", "0.1.0")]
public class WSRVoicePlugin : BasePlugin
{
    static string _logPath;

    public override void Load()
    {
        var packDir = Config.Bind("General", "PackDirectory",
            Path.Combine(Paths.BepInExRootPath, "english-voice"),
            "Folder holding <id>.ogg replacement clips.").Value;

        var enabled = Config.Bind("General", "Enabled", true,
            "Master switch. When false the game plays its original Korean audio.").Value;

        // Off by default: this is a diagnostic for building the next batch, and
        // on a released mod the expected misses are the 1,544 lines the game
        // never voiced plus every action-voice id, none of which a player can
        // act on. Turn it on to produce a generation priority list.
        var logMisses = Config.Bind("Diagnostics", "LogUnmatchedIds", false,
            "Log each id the game asks for that the pack does not have, once per id. "
            + "Off by default; turn on to produce a generation priority list.").Value;

        // Off by default: a released mod has no reason to copy the game's log
        // into ours. Turn it on to pin a spoken line to a scene marker, which
        // is how a reported pronunciation error gets located.
        var mirrorGameLog = Config.Bind("Diagnostics", "MirrorGameLog", false,
            "Copy the game's own Unity log messages into this plugin's log, so "
            + "scene markers and clip ids share one timestamped file.").Value;

        _logPath = Path.Combine(Paths.BepInExRootPath, "wsr-english-voice.log");
        Write("=== WSR English Voice ===");

        if (mirrorGameLog)
        {
            BepInEx.Logging.Logger.Listeners.Add(new GameLogMirror(Write));
            Write("mirroring the game's Unity log into this file (GAME lines)");
        }

        if (!enabled) { Write("disabled by config"); return; }

        VoicePatch.Index = ClipIndex.FromDirectory(packDir);
        VoicePatch.Log = Write;
        VoicePatch.LogMisses = logMisses;
        Write($"indexed {VoicePatch.Index.Count} clips from {packDir}");

        var target = AccessTools.TypeByName("ResourceManager");
        if (target == null) { Write("FATAL: type ResourceManager not found"); return; }

        var method = AccessTools.Method(target, "GetVoice", new[] { typeof(string) });
        if (method == null) { Write("FATAL: ResourceManager.GetVoice(string) not found"); return; }

        new Harmony("wsr.englishvoice").Patch(method,
            prefix: new HarmonyMethod(typeof(VoicePatch).GetMethod(
                nameof(VoicePatch.Prefix), BindingFlags.Static | BindingFlags.Public)));

        Write("patched ResourceManager.GetVoice(string)");

        // The prologue narration is served by its own method, so patching
        // GetVoice alone left the opening in Korean and logged nothing - the
        // plugin was never consulted. Matched by name rather than signature
        // because the parameter type is not knowable from metadata; a failure
        // here must not take the working GetVoice patch down with it.
        var prologue = AccessTools.GetDeclaredMethods(target)
            .FirstOrDefault(m => m.Name == "GetVoice_Prologue");
        if (prologue == null)
        {
            Write("note: ResourceManager.GetVoice_Prologue not found - prologue stays Korean");
        }
        else
        {
            try
            {
                new Harmony("wsr.englishvoice.prologue").Patch(prologue,
                    prefix: new HarmonyMethod(typeof(VoicePatch).GetMethod(
                        nameof(VoicePatch.ProloguePrefix),
                        BindingFlags.Static | BindingFlags.Public)));
                var args = string.Join(", ",
                    prologue.GetParameters().Select(p => p.ParameterType.Name));
                Write($"patched ResourceManager.GetVoice_Prologue({args})");
            }
            catch (Exception e)
            {
                Write($"could not patch GetVoice_Prologue: {e.Message}");
            }
        }
    }

    static void Write(string line)
    {
        try { File.AppendAllText(_logPath, DateTime.Now.ToString("HH:mm:ss.fff") + "  " + line + Environment.NewLine); }
        catch { /* logging must never break playback */ }
    }
}
