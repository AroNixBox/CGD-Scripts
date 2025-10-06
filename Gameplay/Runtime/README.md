Purpose
All gameplay domain logic.

Put here

Input mapping → domain actions (no Unity UI here)
Player/NPC controllers, state machines, interaction systems
Combat, inventory, quests, progression, world rules
Data models/events that UI can observe or subscribe to

Can communicate with

Depends on: Core.Runtime, Common.Runtime
Used by: UI.Runtime (reads/presents)
Must not depend on: UI