# houseCARL for Codex

houseCARL gives Codex comprehensive, data-layer access to a Skyrim Special Edition Mod
Organizer 2 load order. Its local MCP server uses Mutagen to read the true load-order winner,
inspect conflicts, and create reviewable patches without changing source mods by default.

## Requirements

- Windows x64.
- .NET Runtime 9.0 and ASP.NET Core Runtime 9.0.
- Mod Organizer 2 with a configured modlist. MO2 does not need to be running.
- OpenAI Codex with plugin support.

## Install

From the marketplace root containing `.agents/plugins/marketplace.json`:

```powershell
codex plugin marketplace add .
codex plugin add housecarl@housecarl
```

Restart Codex after installation. The plugin provides:

- the `housecarl` MCP server;
- the `$housecarl` routing skill;
- 14 specialist Skyrim skills for records, Papyrus, SKSE, distributors, dialogue, facegen,
  animations, bulk jobs, and generated-tool output.

On first use, set the MO2 instance folder containing `ModOrganizer.ini`. houseCARL stores the
selection in its writable plugin data directory and lets you switch instances later with
`housecarl_set_mo2_instance`.

## Safety

- Reads resolve the true load-order winner and can include the full conflict tree.
- Writes create a new MO2 mod by default.
- In-place edits require `in_place="X.esp"` and persistent consent for that plugin.
- Missing record coverage, invalid schemas, and unsupported operations fail loudly.

See the repository [README](https://github.com/Avick3110/houseCARL) for the complete feature list,
build instructions, license, and credits.
