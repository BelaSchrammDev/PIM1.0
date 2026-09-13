# Production Inventory Manager

A Programmable Block script for [Space Engineers](https://www.spaceengineersgame.com/)
that keeps inventories, refineries and assemblers on a grid sorted and running without
you having to look after them.

Available on the Steam Workshop:
[Production Inventory Manager](https://steamcommunity.com/sharedfiles/filedetails/?id=2080547657)

I started this in 2017 and have been picking it back up on and off ever since. It is
still being worked on, so expect the odd rough edge.

## What it does

- Scans the grid and keeps track of every inventory on it
- Sorts items into the containers you assign them to, using several stacking strategies
- Feeds refineries and reprocessors, and keeps assembler queues topped up
- Auto-crafts components once stock drops below a threshold you set
- Keeps weapons supplied with the right ammunition
- Handles hydroponics farms and water recycling
- Writes status information to LCD panels
- Talks to SMS over the intergrid message bus

Supports a few mods as well, currently Sigma Draconis Core and Apex items.

## Requirements

- Space Engineers with a Programmable Block

## Setup

Paste the script into a Programmable Block and run it once. Configuration lives in the
block's Custom Data and is created with sensible defaults on first run. Tag containers
with keywords such as `(sms,food)` to tell the script what belongs where. The in-game
`Instructions.readme` covers the tags in detail.

## How it is built

A Programmable Block gets a hard instruction budget per tick, and going over it stops
the script. So the work is not done in one pass. Everything is split into jobs, each
one a small state machine — initialise, run, cool down — that does a slice of work and
then hands control back.

`Program.Main` walks through the job list and keeps calling the current one until the
instruction budget for this tick runs out, then picks up where it left off next tick.
The `LoopManager` watches how long a full cycle takes and adjusts the budget up or
down to stay near four seconds, between 300 and 5000 instructions. If several
Programmable Blocks on the same grid run the script, one takes over as master and the
rest drop into standby.

Roughly 7,000 lines across 70-odd files. C# 6 is the ceiling here — that is what the
in-game compiler accepts, not a preference. Built with
[MDK2](https://github.com/malforge/mdk2), which handles minification and deployment
into the game.

```
BlockClasses/    wrappers around refineries, assemblers, containers, guns
JobClasses/      the scheduled jobs, including the stacking strategies
Share/           job base classes and the networking layer
Tools/           inventory helpers, filters, config, LCD output
Definitions/     configuration, string tables, shared state
```

## Status

Working and in use, but not finished. Open points are tracked in the issues. The
`VanillaRefineryManager` is next in line for a rewrite, and not every block type has a
proper manager class yet.

## Licence

MIT, see LICENSE.txt.
