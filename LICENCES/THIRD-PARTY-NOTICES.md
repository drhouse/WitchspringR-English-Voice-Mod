# Third-party notices

This mod **redistributes** the two components so that installation is a
single extract. Both are licensed under the **GNU Lesser General Public License,
version 2.1**, whose full text is in `LICENSE.LGPL-2.1.txt` beside this file.

Neither component is modified. Both are shipped exactly as published by their
authors.

## BepInEx

- Project: https://github.com/BepInEx/BepInEx
- Version bundled: **6.0.0-be.788** (Unity IL2CPP), build commit `5b766a3b`
- Licence: LGPL-2.1 — https://github.com/BepInEx/BepInEx/blob/master/LICENSE
- Files: `BepInEx/core/`, `BepInEx/patchers/`, `dotnet/`, `changelog.txt`
  (`changelog.txt` is BepInEx's own changelog, not this mod's)

## UnityDoorstop

- Project: https://github.com/NeighTools/UnityDoorstop
- Version bundled: **4.5.0** (see `.doorstop_version`)
- Licence: LGPL-2.1 — "Doorstop 4 is licensed under LGPLv2.1", per the project's
  README
- Files: `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version`

## Obtaining the source

LGPL-2.1 requires that the source for these components be available to anyone
who receives the binaries. Neither is modified here, so the authoritative source
is upstream at the URLs above, at the versions named. Both repositories publish
the source and the build scripts for these exact releases.

## Components used but NOT redistributed

- **VoxCPM2** (Apache-2.0) — https://github.com/OpenBMB/VoxCPM — generated the
  English audio. Not shipped; it ran during production.
- **NVorbis** (MIT) — https://github.com/NVorbis/NVorbis — decodes Ogg Vorbis
  inside the game. Shipped as `BepInEx/plugins/NVorbis.dll`, so MIT requires its
  copyright and permission notice be preserved: included as
  `LICENSE.NVorbis-MIT.txt` (Copyright (c) 2020 Andrew Ward).

## The game

*WitchSpring R* and all of its original assets — script, artwork and the Korean
voice performances the English clips are derived from — are the property of
**ZeiGames / Kiwiwalks**. No game asset is redistributed here. The mod requires
an owned copy of the game.
