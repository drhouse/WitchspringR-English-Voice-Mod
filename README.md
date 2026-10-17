<div align="center">

# WitchSpring R - English Voice Mod

[![DOWNLOAD](https://img.shields.io/badge/%E2%AC%87_DOWNLOAD-red?style=for-the-badge&logo=github)](https://github.com/drhouse/WitchspringR-English-Voice-Mod/releases)
[![Status](https://img.shields.io/badge/STATUS-WORKING_%F0%9F%9F%A2-brightgreen?style=for-the-badge)](#features)
[![Key System](https://img.shields.io/badge/KEY-NO_KEY_required-success?style=for-the-badge)](#features)
[![Platform](https://img.shields.io/badge/PLATFORM-Windows_10_%E2%80%A2_11-blue?style=for-the-badge)](#requirements)

</div>

English voice-over for every spoken line in *WitchSpring R*, produced by
cross-lingually cloning the game's own Korean voice actors so each character
keeps their own voice while speaking English.

**11,856 clips · 119 speakers · ~470 MB**

The game's Korean audio is untouched. The plugin intercepts the game's own
voice lookup and substitutes a matching English clip when one exists, so
anything the pack does not cover simply plays the original Korean.

---
YouTube video preview:  
[![Watch the video](https://img.youtube.com/vi/4TGWJj0U9vk/hqdefault.jpg)](https://www.youtube.com/watch?v=4TGWJj0U9vk)  
https://www.youtube.com/watch?v=4TGWJj0U9vk

## Requirements

| | |
|---|---|
| Game | *WitchSpring R* (tested against **v1.109**, Steam) |
| Loader | **BepInEx 6 (IL2CPP)** - included in [Release file](https://github.com/drhouse/WitchspringR-English-Voice-Mod/releases)
| Platform | Windows |

## Installing

1. Extract the Release archive into the game's main directory
2. Run game - booting will take a little longer first time, as it generates its directories
3. Game menu - set 'Settings > Sound > Voice Language: Korean'   
   (Japanese should also work, but English audio matches Korean voices)
4. Start the game - the first spoken line should be in English

Do not copy a config file from anywhere: the plugin writes
`BepInEx/config/wsr.englishvoice.cfg` on first run with paths correct for
*your* installation.

## Configuration

`BepInEx/config/wsr.englishvoice.cfg`, created on first launch:

| Setting | Default | Meaning |
|---|---|---|
| `Enabled` | `true` | `false` plays the original Korean audio |
| `PackDirectory` | `<BepInEx>/english-voice` | Where the `.ogg` clips live |
| `LogUnmatchedIds` | `false` | debug: Logs any clip id the pack lacks, once each |
| `MirrorGameLog` | `false` | debug: Copies the game's log lines into the plugin log |

Leave the two diagnostic settings off unless you are reporting a problem.

## Uninstalling

Delete the three DLLs from `BepInEx/plugins/` and the `BepInEx/english-voice/`
folder, or simply set `Enabled = false`. No game file is ever modified.

## What is and is not covered

- **Covered:** every line with a script entry and a speaking character.
- **Not covered:** nine lines from three non-speaking characters, and clips
  that are sound effects rather than speech (animal noises, Korean
  "translation not required" placeholders) — these deliberately keep the
  game's original audio, because hearing an English voice read "Oink!" aloud
  is worse than the original.
- Some character and place names are respelled phonetically so the voice says
  them the way the game's English text implies, rather than the way an English
  reader would guess.

## Known limitations

- Pronunciation is right roughly 98% of the time. The remaining cases are
  mostly invented names and interjections.
- A few interjections ("Hmph", "Pfft") are approximated, since a cloned voice
  reading a written-out grunt is not the same as a recorded grunt.
- Line delivery follows the text, not the original Korean performance's timing,
  so a clip occasionally runs longer or shorter than the Korean take.

## Credits and legal

- Original voice performances, script and audio: **ZeiGames / Kiwiwalks**,
  the creators of *WitchSpring R*. All rights to the original work remain
  theirs.
- The English clips are derived from the game's own audio using open-source
  speech models. This is an unofficial, non-commercial fan modification. 
- Ogg Vorbis decoding in-game: [NVorbis](https://github.com/NVorbis/NVorbis)
  (MIT).
- Mod loader: BepInEx 6.0.0-be.788, redistributed unmodified under LGPL-2.1

## Reporting a wrong line

The useful details are the **clip id** and what you heard. Set
`LogUnmatchedIds = true` and `MirrorGameLog = true`, reproduce the line, and
attach `BepInEx/wsr-english-voice.log`; the id appears next to the lookup.
