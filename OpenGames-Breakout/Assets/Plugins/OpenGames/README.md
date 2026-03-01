Jih.OpenGames Framework
---

This repository is a shared framework for developing the OpenGames series using the Unity engine.  
The primary goal of this framework is to consolidate, manage, and provide utility features frequently utilized in real-world game development.

> [!WARNING]
> This framework is currently under active development. As such, backward compatibility is not guaranteed, and **breaking changes** may occur without prior notice.

## Key Features

### Nullable Context

``` csharp
#nullable enable
```
All code included in this framework is written within a `nullable` context.  
`MonoBehaviour` scripts are no exception.  
This approach helps prevent chronic `NullReferenceException` issues and provides explicit null-state tracking for reference arguments and return values.

### Practical Extension Methods

* **MathEx**: Contains various mathematical methods, ranging from degree/radian conversions to finding the closest point between a point and a line. It provides extension methods like `IsNearly` and `Sq` that reduce code complexity and clearly convey intent without excessive comments.
* **EnumerableEx**: Includes extension methods for arrays and collections. For example, `IsValidIndex` makes the code's intent more explicit.
* **ObjectEx**: Contains extension methods for `UnityEngine.Object`. The `ThrowIfNull` method performs a `deep-null` check specific to `UnityEngine.Object`, in addition to standard system `null` checks.
* Static classes ending with the `Ex` suffix denote a collection of extension or utility methods.

### Standardized Random Number Generator

* **IRandomInt32**: Randomness is essential in gaming. `IRandomInt32` is the core interface for providing random numbers within this framework. Functions utilizing random numbers are standardized based on this interface.
* **RandomStream**: A random-access-capable generator based on the **Philox** algorithm. Unlike typical generators that rely on caching and cannot be restored to previous states, `RandomStream` supports random access and state restoration via the `Position` value. This is particularly useful for deterministic game design.

### FrameStack Paradigm for Global State Management

Unity provides static properties to set global states (e.g., `Time`, `Cursor`). While easy to use at a small scale, as the project grows, it becomes difficult to track where and why a state was changed, and restoring the original state becomes a challenge.  
To solve this, the **FrameStack** paradigm was introduced. It is inspired by stack pointer restoration (function frames) in low-level programming.
* State changes are recorded in data units called `Frame`s. These frames are `Push`ed or `Popp`ed through the `FrameStack` to modify or restore the engine's global state.
* Each `Frame` is designed to be utilized only within a scope that references a pre-designated `Holder` object, enhancing restoration stability. This system works neatly when integrated with state machines.
* The APIs are located in the `Jih.OpenGames.Runtime` namespace. `IState` and `StateBase` in the same namespace provide the archetypes for the state machine.
``` csharp
public override void Begin(IState? prev)
{
    base.Begin(prev);

    // Modify global settings when entering a state (Push)
    InputFrameStack.Push(new InputFrame(holder: this, ui: false, player: false));
    CursorFrameStack.Push(new CursorFrame(holder: this, lockMode: CursorLockMode.None, cursorVisible: true));
    TimeFrameStack.Push(new TimeFrame(holder: this, timeScale: 1f));
}

public override void End(IState? next)
{
    // Restore global settings when exiting a state (Pop)
    TimeFrameStack.Pop(holder: this);
    CursorFrameStack.Pop(holder: this);
    InputFrameStack.Pop(holder: this);

    base.End(next);
}
```

## Installation

### Git (Recommended)

The most common method is to clone this repository into the Assets folder of your Unity project using [Git](https://git-scm.com/).
``` shell
git clone https://github.com/jihworks/OpenGames.git
```

> [!NOTE]
> The standard for the OpenGames series is to manage this repository as a **Subtree** within the `Assets/Plugins` subdirectory of individual projects. Specific configurations can be found in the `sync_shared.bat` file of each repository.

### Manual Download

If you prefer to maintain a specific commit version or do not require Git's version control features, you can use GitHub's download feature.

  1. Click the green **Code** button at the top of the [OpenGames GitHub page](https://github.com/jihworks/OpenGames).
  2. Select **Download ZIP** from the dropdown menu.
  3. Extract the `zip` file into your project's `Assets` folder.

## Technical Information

* Unity Version: `Unity 6.3 LTS (6000.3.10f1)`
* Root Namespace: `Jih.OpenGames`

> [!WARNING]
> The Unity version may change based on the development status of individual series, and documentation updates may lag behind.

## Contribution

As this project is maintained by a single developer, contributions via Issues or Pull Requests are not currently accepted to minimize management overhead. For questions or feedback regarding the framework, please use the [Discussions](https://github.com/jihworks/OpenGames/discussions) tab.

## License

This framework is provided under the **MIT License**. You are free to use, modify, and distribute it.

`SPDX-License-Identifier: MIT`

© 2026 Jong-il Hong
