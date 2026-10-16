using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using WSRVoice.Core;

public static class VoicePatch
{
    public static ClipIndex Index;
    public static Action<string> Log = _ => { };
    public static bool LogMisses;

    const int CacheLimit = 50;
    static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();
    static readonly Queue<string> CacheOrder = new Queue<string>();
    static MethodInfo _setDataStatic;

    // A miss is logged once per id, not once per lookup. The game asks for the
    // same id every time a line replays, and Write opens and closes the file on
    // each call, so logging every lookup put disk I/O on the dialogue path and
    // grew the log without limit. Once per id is also what makes the log the
    // generation priority list it claims to be: a set of ids, not a tally.
    static readonly HashSet<string> LoggedMisses = new HashSet<string>();

    /// <summary>
    /// Prefix for ResourceManager.GetVoice(string). Returning false replaces the
    /// result; returning true lets the original Korean lookup run.
    /// </summary>
    public static bool Prefix(string id, ref AudioClip __result)
    {
        try
        {
            if (Index == null || !Index.TryGet(id, out var path))
            {
                if (LogMisses && LoggedMisses.Add(id)) Log($"MISS {id}");
                return true;
            }

            var clip = GetOrCreate(id, path);
            if (clip == null) return true;

            __result = clip;
            return false;
        }
        catch (Exception e)
        {
            Log($"ERROR {id}: {e.Message}");
            return true;
        }
    }

    /// <summary>
    /// Prefix for ResourceManager.GetVoice_Prologue. The opening narration does
    /// not come through GetVoice at all, which is why patching only GetVoice
    /// left the prologue in Korean with no MISS logged - the plugin was never
    /// asked. Written against __args because the parameter's type is unknown
    /// from metadata alone: a string id is used as-is, and a numeric index is
    /// mapped to EventPrologue1_&lt;n&gt;, which is what the ids in TalkText and
    /// the Korean clip names (Eimhaed_EventPrologue1_0) both use.
    /// </summary>
    public static bool ProloguePrefix(ref AudioClip __result, object[] __args)
    {
        try
        {
            var raw = __args != null && __args.Length > 0 && __args[0] != null
                ? __args[0].ToString()
                : null;
            if (string.IsNullOrEmpty(raw)) return true;

            foreach (var candidate in PrologueCandidates(raw))
            {
                if (!Index.TryGet(candidate, out var path)) continue;
                var clip = GetOrCreate(candidate, path);
                if (clip == null) continue;
                __result = clip;
                return false;
            }

            // Logged unconditionally and once per argument: this is the only
            // record of what the method is actually called with, and the id
            // form was not knowable before seeing one.
            if (LoggedMisses.Add("prologue:" + raw)) Log($"PROLOGUE MISS arg={raw}");
            return true;
        }
        catch (Exception e)
        {
            Log($"PROLOGUE ERROR: {e.Message}");
            return true;
        }
    }

    static IEnumerable<string> PrologueCandidates(string raw)
    {
        yield return raw;
        int index;
        if (int.TryParse(raw, out index)) yield return "EventPrologue1_" + index;
    }

    static AudioClip GetOrCreate(string id, string path)
    {
        if (Cache.TryGetValue(id, out var cached))
        {
            if (cached != null) return cached;

            // Unity destroyed the native object (scene change), leaving a dead
            // wrapper. Rebuild in place so the id keeps its single queue slot.
            var rebuilt = Build(id, path);
            if (rebuilt == null) return null;
            Cache[id] = rebuilt;
            return rebuilt;
        }

        var clip = Build(id, path);
        if (clip == null) return null;

        Cache[id] = clip;
        CacheOrder.Enqueue(id);
        while (CacheOrder.Count > CacheLimit) Cache.Remove(CacheOrder.Dequeue());

        return clip;
    }

    static AudioClip Build(string id, string path)
    {
        var audio = PcmAudio.FromOggFile(path);
        if (audio == null)
        {
            Log($"DECODE FAILED {id}");
            return null;
        }

        var clip = AudioClip.Create(id, audio.SampleCountPerChannel,
                                    audio.Channels, audio.SampleRate, false);
        if (!SetData(clip, audio.Samples))
        {
            Log($"SETDATA FAILED {id}");
            return null;
        }

        Log($"HIT {id} ({audio.SampleCountPerChannel} samples, {audio.SampleRate} Hz)");
        return clip;
    }

    // Under Il2CppInterop, AudioClip.SetData must be invoked as a STATIC method
    // with signature (AudioClip, Il2CppStructArray<float>, int, int).
    static bool SetData(AudioClip clip, float[] samples)
    {
        if (_setDataStatic == null)
        {
            foreach (var m in typeof(AudioClip).GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                if (m.Name != "SetData") continue;
                var p = m.GetParameters();
                if (p.Length == 4 && p[0].ParameterType == typeof(AudioClip)
                                  && p[2].ParameterType == typeof(int)
                                  && p[3].ParameterType == typeof(int))
                {
                    _setDataStatic = m;
                    break;
                }
            }
            if (_setDataStatic == null) return false;
        }

        var il2cppSamples = new Il2CppStructArray<float>(samples.Length);
        for (var i = 0; i < samples.Length; i++) il2cppSamples[i] = samples[i];

        // Verified against UnityEngine.AudioModule.dll in BepInEx\interop:
        //   static bool SetData(AudioClip clip, Il2CppStructArray<float> data,
        //                       int numsamples, int samplesOffset)
        // numsamples and samplesOffset are both PER CHANNEL, while `samples` is
        // interleaved - hence the division by channel count.
        var perChannel = samples.Length / Math.Max(clip.channels, 1);
        var ok = _setDataStatic.Invoke(null, new object[] { clip, il2cppSamples, perChannel, 0 });
        return ok is bool b ? b : true;
    }
}
