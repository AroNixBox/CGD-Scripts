Purpose
Project-wide systems and services that underpin the game.

Put here

Bootstrapper/entry points (lifecycle, composition root/DI)
Managers/services: SceneLoader, Save/Load, Audio, Time, Localization, Addressables wrapper, Event Bus/Message Hub, Config providers
Service interfaces exposed to higher layers

Can communicate with

Depends on: Common.Runtime
Used by: Gameplay.Runtime, UI.Runtime
Must not depend on: Gameplay or UI