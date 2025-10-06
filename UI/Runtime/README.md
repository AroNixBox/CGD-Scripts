Purpose
Presentation layer and UI bindings.

Put here

UI setup and composition (screens, HUD, popups)
UI input handling (buttons, sliders) → forward to Gameplay/Core via events
Lightweight UI animations and transitions

Can communicate with

Depends on: Gameplay.Runtime (read/subscribe), Core.Runtime, Common.Runtime
Must not be referenced by: Gameplay or Core (one-way dependency)