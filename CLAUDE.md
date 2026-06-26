# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Minesweeper (Сапёр) game built in Unity 2022.3.62. Uses Zenject/Extenject for DI, UniTask for async operations, and Unity Addressables for asset management.

## Architecture

### Namespace Structure

- `Installers.Project` / `Installers.Scene` — Zenject bindings (project-level vs scene-level)
- `Model.Entities` — Data structs (`Cell`, `GameFieldModel`)
- `Model.Processors` — Pure game logic (`GameFieldProcessor`)
- `Model.Signals` — SignalBus event definitions
- `Model.Configs` — ScriptableObject configs
- `Services` — Application-level services (input, windows, game cycle, assets)
- `Controller` — Game logic controllers
- `View` — MonoBehaviour views
- `UI.Windows` / `UI.Elements` / `UI.Base` — UI layer
- `Common` — Base classes, factories

### Key Patterns

**Dependency Injection (Zenject)**
All dependencies resolved through DI container. `DiControllerFactory` creates controllers at runtime. Bindings live in `Installers/` using `AsSingle` for singletons.

**SignalBus (Event Bus)**
Signals in `Model.Signals` decouple components. Fire with `_signalBus.Fire(new SomeSignal(...))`, subscribe in `Initialize()`. Key signals: `OnPointerSignal`, `OnCellPointerSignal`, `OnCellStateChangedSignal`, `OnGameOverSignal`.

**State Machine**
`GameCycleService` transitions between `MenuGameState` and `GameplayGameState`. States inherit `BaseGameState` and implement async `Enter`/`Exit` with `CancellationToken` for lifecycle.

**Window System**
Windows follow `BaseWindowController<TView, TData>` / `BaseWindowView`. `WindowService` manages a `LinkedList`-based stack. Open windows via `WindowService.OpenWindow<TController>(data)`.

**Async Initialization**
`AsyncBootstrapper` runs the startup chain sequentially: `UiCameraService` → `WindowService` → `GameCycleService`. All async code uses `UniTask` + `CancellationToken` (provided by `AppLifetimeTokenService`).

### Game Logic Flow

1. `AsyncBootstrapper` initializes services
2. `GameCycleService` enters `MenuGameState` → opens `MainMenuWindow`
3. On play: transitions to `GameplayGameState`, creates `GameFieldModel`, spawns `CellView` prefabs
4. `InputService` polls mouse → fires `OnPointerSignal` → `GameInputProcessorService` converts to cell coords
5. First click places mines; `GameFieldProcessor` handles flood-fill reveal (stack-based, no recursion)
6. Win/lose: `OnGameOverSignal` fired → `GameOverWindow` shown
7. Return to menu: `GameCycleService` exits gameplay state (disposes controllers/views)

### Performance Conventions

- `Cell` is a **struct** accessed via `ref` (`ref Cell GetCell(int col, int row)`) — do not copy without intent
- Flood fill uses an explicit `Stack<>` to avoid recursion overhead
- `AssetService` caches loaded Addressable assets in a `Dictionary` — do not load the same asset path twice
- Controllers implement `IDisposable`; always dispose via `CancellationToken` or explicit `Dispose()` in state `Exit`

## Dependencies

| Package | Purpose |
|---|---|
| `com.svermeulen.extenject` | Zenject DI container |
| `com.cysharp.unitask` | async/await without allocations |
| `com.unity.addressables 1.22.3` | Runtime asset loading |
| `com.unity.textmeshpro 3.0.9` | UI text |
| `com.unity.feature.2d 2.0.1` | 2D rendering |

## Development Notes

- Open project in Unity 2022.3.x; use Rider or Visual Studio (configured in Packages)
- Scene: `Assets/Scenes/SampleScene.unity` (single scene)
- Config: `Assets/Configs/GameFieldSettings.asset` — grid size and mine count
- No automated tests are configured despite `com.unity.test-framework` being present
- No CI/CD pipelines

## Output format (apply to every code response)
- Только готовый C# код, без объяснений до/после
- Несколько файлов — каждый в отдельном блоке с именем файла в заголовке
- Пояснения внутри кода — только короткими `//` комментариями

## New code conventions
- DI: зависимости через `[Inject]` на полях + `[Inject] void Construct()`, **не конструктор**
- Легаси в проекте: местами конструкторы — не трогать без явной задачи
- Тики: только `UniTask` loop, **не `ITickable`**
- View: только публичные поля, никаких методов — вся логика в контроллере
- Assets: загрузка только через `AssetService`, ссылки на префабы в конфигах, не во View
- `OnAfterShow` / `OnBeforeHide`: оверрайдить полностью, `base` не вызывать (пустая)
- Подписки на кнопки/сигналы — в `Initialize()`, через `Disposables` коллекцию

## Gotchas
- `LoadingWindowData` — пустой класс, без параметров и экшенов
- `GameFieldController` — при рестарте пересоздавать полностью (Reset или Dispose+new), не переиспользовать
- `Cell` — struct, доступ через `ref`, не копировать без умысла

