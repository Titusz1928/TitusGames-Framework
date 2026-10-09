<p align="center">
  <img src="Media~/logo1.png" width="300" alt="Framework Logo">
</p>

# TitusGames Framework (v2.2.0)

A modular, enterprise-ready Unity package designed for rapid **2D and 3D** game assembly. It includes streamlined subsystems for lifecycle initialization, scene routing, nested window management, deep localization streaming, dynamic audio mixing, and queue-driven messaging overlays.

Version 2.0.0 migrates the entire framework core code to an optimized, decoupled UPM package architecture running custom Assembly Definitions (`.asmdef`) and native namespaces to insulate package logic from user land scripts.

---

## Update 2.2.0:

### ⚡ Core Subsystem Overhaul & Direct Asset Playback

Version 2.2.0 focuses on robustness, developer experience, and native asset integration across core services.

#### Key Highlights
* **Service Locator Modernization**: Added native `TryGet<T>` support (eliminating external extension workarounds and exception overhead), Unity object lifetime checks (auto-cleaning destroyed `MonoBehaviour` references), and `GetOrRegister<T>` factory routines.
* **Smart Window Stack & Duplicate Prevention**: `WindowManager` now prevents duplicate window instantiation. Opening an already active window prefab safely moves it to the top of the stack and brings it to the front visually (`SetAsLastSibling()`).
* **Managed UI Overlay Snapshots**: Added automatic HUD/overlay snapshot and restore capability (`RegisterManagedOverlay()`), hiding active gameplay UI elements when menus open and restoring their exact state when closed.
* **Native `AudioClip` Playback**: `AudioManager` and `IAudioService` now accept direct `AudioClip` parameters alongside string resource names, removing the need for custom adapter wrapper scripts.
* **Window Event Callbacks**: `OnWindowOpened` and `OnWindowClosed` now emit the specific `GameObject` instance for precise event handling.

#### Migration Workflow
If you are upgrading from 2.1.x, replace all instances of static access with the new service registry:

Before (Singleton):

```c#
MessageManager.Instance.ShowMessage("Hello World");
```
After (Service Locator):

```c#
// Safe, exception-free service resolution
if (ServiceLocator.Current.TryGet<IMessageService>(out var messageService))
{
    messageService.ShowMessage("Hello World");
}
```

## ⚙ Installation & Setup

### 1. Install via UPM (Unity Package Manager)
1. Open your Unity Project (Unity 2022.3 LTS or Unity 6+ recommended).
2. Navigate to **Window > Package Manager**.
3. Click the **+** button in the top-left corner and select **Add package from git URL...**
4. Paste the repository Git URL:
```text
   [https://github.com/Titusz1928/TitusGames-Framework.git](https://github.com/Titusz1928/TitusGames-Framework.git)
```

### 2. Import Dimensional Starters (Samples)
1. In the Package Manager, select **TitusGames Framework**.
2. Find the **Samples** section. You will see two options:
    * **2D Starter Framework:** Optimized for 2D projects (Orthographic cameras, 2D lighting).
    * **3D Starter Framework:** Optimized for 3D projects (Perspective cameras, Skybox, 3D lighting).
3. Click **Import** next to your preferred version.

### 3. Basic Configuration

Namespaces: All scripts are decoupled into the framework layer. You must include the following line at the top of any game script accessing framework APIs:
```csharp
using TitusGames.Framework;
```

**Build Settings**: Remember to go to File > Build Settings and add your scenes to the Scenes in Build list.

**Boot Scene**: Always start your game from the scene containing the Boot manager (or sandbox initializer) to ensure all systems initialize correctly.

## 🚀 Features

- **BootManager** – initializes all other managers automatically; no need to manually place managers in new scenes.
- **2D & 3D Support** – Includes pre-configured sample scenes for both dimensions.
- **Sandbox Mode** – A dedicated testing environment that mirrors the Boot sequence for isolated development.  
- **SceneManager** – easily navigate between scenes and exit the game.  
- **WindowManager** – create UI windows and open them from buttons.  
- **LocalizationManager** – localize any TextMeshPro UI element with JSON files.  
- **AudioManager** – play SFX and music stored inside the framework’s audio folder.
- **MessageManager** – make messages appear.

---

## 🧪 Sandbox & Team Workflow

The framework includes a dedicated Sandbox Scene located within the sample layers, engineered to streamline team workflows and mitigate Git merge conflicts.

Why use the Sandbox?
In a collaborative environment, editing core scenes such as "Main" or "Boot" frequently leads to structural merge conflicts. The Sandbox provides a safer alternative:

**Isolated Testing**: Build out modular mechanics in a standalone scene environment without altering the primary production flow.

**Full System Access**: The Sandbox automatically spawns standard instances of the Audio, Window, Localization, and Scene routers instantly upon runtime instantiation.

**Zero Configuration Overload**: Simply branch a local duplicate of the Sandbox environment to prototype new mechanics using the pre-wired framework backbone.

[!TIP]
Best Practice: Reserve the Boot scene for the final game flow and structural master staging, while utilizing the Sandbox (or local duplicates of it) for daily development, feature prototyping, and isolated logic testing.


In a team, editing the "Main" or "Boot" scene frequently leads to Git merge conflicts. The Sandbox allows you to:

* **Isolated Testing:** Create your own "Test" scene to build a specific mechanic without touching the production flow.
* **Full System Access:** The Sandbox automatically loads the Audio, Window, Localization, and Scene managers, just like the real Boot scene.
* **Zero Setup:** Simply open the Sandbox scene and start dragging your new prefabs or scripts in; the framework backbone is already initialized and running.

> [!TIP]
> **Best Practice:** Keep the `Boot` scene reserved for the final game flow and use `Sandbox` (or duplicates of it) for daily development, prototyping, and isolated feature testing.

---

## 📂 Project Structure
The framework is bifurcated into two distinct zones: the Package Scope (Core Logic) and the Asset Scope (User Implementation/Samples).

### 1. The Package (Core Logic)
Located in Packages/com.titusgames.framework/, this area contains the source code. You should not modify these files directly if you intend to maintain portability.

```text
com.titusgames.framework/
├── Runtime/
│   └── Scripts/               # Core framework services (ASMDEF-bound)
│       ├── Managers/          # Service implementations (Audio, Boot, Localization, etc.)
|       ├── Others/            # Other scripts
│       └── ThirdParty/        # External utilities (e.g., MiniJSON)
└── Media~/                    # Branding and internal icons
```
### 2. The Assets (Samples & Implementation)

Located in Assets/Samples/TitusGamesFramework/, this folder contains the content that your end-users will actually edit. When you import a sample, Unity copies these files into your Assets folder so they can be easily modified without altering the core package.

```text
Assets/Samples/TitusGamesFramework/
├── TitusGamesFramework2D/      # 2D-specific implementation templates
│   ├── Prefabs/               # UI/Game instance templates
│   ├── Resources/             # Audio, Languages, and UI/Messaging definitions
│   ├── Scenes/                # Testing sandbox and boilerplate scenes
│   └── UI/                    # Sprites, Window layouts, and UI components
│
└── TitusGamesFramework3D/      # 3D-specific implementation templates
    └── (Same structure as 2D)
```
Key Structural Concepts
~ (Tilde) Folders: Folders like Samples~ and Media~ are ignored by Unity’s import process, preventing unnecessary overhead in your production builds.

ASMDEF Boundaries: The code in Runtime/Scripts is strictly separated from your Assembly-CSharp.dll using Assembly Definitions (.asmdef). This ensures that your game code cannot accidentally become dependent on internal manager implementation details, forcing you to use the ServiceLocator interface approach.

Resource Management: All Resources/ folders are kept inside your Samples directory. This ensures that the framework remains "clean"—it doesn't pollute the global Resources path until you explicitly import the samples into your Assets folder.

---

# 🎮 SceneManager

The `SceneManager` lets you load scenes by name and exit the game. Every new scene that you create should have a canvas which needs a WindowRoot and a MessageContainer gameobject.

### ✔ Load a Scene
```csharp
if (ServiceLocator.Current.TryGet<ISceneService>(out var sceneService))
{
    sceneService.LoadScene("GameScene");
}
```

### ✔ Exit the Game
```csharp
if (ServiceLocator.Current.TryGet<ISceneService>(out var sceneService))
{
    sceneService.ExitGame();
}
```

### ✔ Add a Button That Loads a Scene

Add a Unity Button to the UI.

Add the UI_SceneButton script to it.

Enter the scene name into the Scene Name field.

Add an OnClick() event.

Drag the script into the event.

Select:

SceneManager → LoadScene()

# 🪟 WindowManager

The WindowManager provides a centralized, stack-based system for managing UI panels. It handles dynamic instantiation, automatic input mapping, time-scaling (pausing), and cursor state management.

### ✔ Architectural Highlights
* **Duplicate Window Safeguard**: Opening an already-open window prefab brings the existing instance to the visual front (`SetAsLastSibling()`) and moves it to the top of the interaction stack instead of spawning a duplicate.
* **Managed UI Overlay Snapshots**: Automatically snapshot and hide active gameplay UI elements (e.g., HUD, minimap, quest tracker) when the first window opens, automatically restoring their visibility when all windows close.
* **Window Instance Callbacks**: Subscribe to `OnWindowOpened` and `OnWindowClosed` events to receive direct references to the affected `GameObject` instance.
* **Chain of Responsibility**: Implements `ICancelInputHandler`, allowing input handling to pass smoothly to gameplay systems when no windows are active.
* **Settings-Driven Windows**: Attaching a `UIWindowSettings` component to your window prefab allows automatic configuration for time freezing, cursor visibility, and input modes.

### ✔ How to Create a New Window

**1. Prepare the Prefab**: Create your UI panel. For advanced control (like freezing time or showing the cursor), attach the UIWindowSettings script to the root of your prefab.

**2. Opening via Button**:

* Add a Button to your UI.

* Attach the UI_OpenWindow script to the button object.

* Assign your window prefab to the Window Prefab slot in the inspector.

* In the OnClick() event, drag the UI_OpenWindow component into the slot and select WindowManager.OpenWindow.

### ✔ Code Usage

```csharp
// Programmatically open or focus a window prefab
GameObject windowInstance = ServiceLocator.Current.Get<IWindowService>().OpenWindow(myWindowPrefab);

// Register a HUD overlay to auto-hide while menus are open
ServiceLocator.Current.Get<IWindowService>().RegisterManagedOverlay(hudGameObject);

// Close active top window or all windows
ServiceLocator.Current.Get<IWindowService>().CloseTopWindow();
ServiceLocator.Current.Get<IWindowService>().CloseAllWindows();

// Listen to window lifecycle events
IWindowService windowService = ServiceLocator.Current.Get<IWindowService>();
windowService.OnWindowOpened += (win) => Debug.Log($"Opened window: {win.name}");
windowService.OnWindowClosed += (win) => Debug.Log($"Closed window: {win.name}");
```

### ✔ Closing Windows
*   **Automatic:** The manager natively listens for `UI/Cancel` inputs. When a user presses "Escape" (or the cancel button), the top-most window in the stack is closed automatically.
*   **Programmatic:**
    ```csharp
    if (ServiceLocator.Current.TryGet<IWindowService>(out var windowService))
{
    windowService.CloseTopWindow();
}


# 🌍 LocalizationManager

Add localization to any text using simple JSON files.


### Example JSON (eng.json)
```json
{
  "play_button": "Play",
  "settings_button": "Settings",
  "exit_button": "Exit"
}
```

### ✔ How to Localize a Text Element

Add the LocalizedText component to a TextMeshPro UI object.

Enter the Key from your JSON file (example: "play_button").

The text will automatically update based on the selected language.

### ✔ Add or Edit Localization

Open the JSON files in:

/Resources/Languages

Add or modify your keys.

Save — the manager automatically reloads them at runtime.

### ✔ Add new language

Edit the language_manifest.json file inside /Resources/Languages and add the new language code. Create a new file with this language code as a name and add the key-value pairs inside, similar to how the other language files look. Dont forget to update the other languages files so that they have the new language code too.

# 🔊 AudioManager

The `AudioManager` supports both direct `AudioClip` reference playback and string-based dynamic loading from `Resources`. It manages pooling, track fading, randomized pitch/volume variation, and global volume settings without requiring custom wrapper adapters.

### ✔ Required Folder Structure
Place your files in these exact paths inside your Resources folder:

Resources/Audio/Music/ (for .mp3, .wav, .ogg music loops)

Resources/Audio/SFX/ (for sound effects)

### ✔ Playing Music (The Easy Way)
The framework includes a MusicTrigger script so you can set up music without writing code:

Create an Empty GameObject in your scene.

Attach the MusicTrigger script.

Type the Filename (without extension) in the Track Name field.

(Optional) Check Stop On Scene Exit if you want the music to silence when leaving this scene.

### ✔ Playing Music (Via Code)
Music automatically handles cross-fading when switching tracks.

```csharp
// Play using direct AudioClip or string name
ServiceLocator.Current.Get<IAudioService>().PlayMusic(mainMenuThemeClip, fade: true);
ServiceLocator.Current.Get<IAudioService>().PlayMusic("MainMenuTheme", fade: true);
```

### ✔ Playing Sound Effects (SFX)

```csharp
// Direct AudioClip reference playback (recommended for ScriptableObjects / Inspector references)
AudioClip clip = itemSO.useSound;
ServiceLocator.Current.Get<IAudioService>().PlaySFX(clip);

// String-based playback (from Resources/Audio/SFX)
ServiceLocator.Current.Get<IAudioService>().PlaySFX("click");
ServiceLocator.Current.Get<IAudioService>().PlaySFX("explosion", 0.5f); // 50% volume
```

### ✔ Randomized SFX (Adding Variety)
Prevents ear fatigue by applying dynamic pitch and volume variance to repetitive actions (footsteps, weapon swings, combat hits):

```csharp
// Randomized pitch (±0.1) and volume using direct AudioClip
ServiceLocator.Current.Get<IAudioService>().PlayRandomizedSFX(footstepClip, 0.1f, 0.1f);

// Pick a random clip from an array/list with randomization
AudioClip[] footstepClips = { step1, step2, step3 };
ServiceLocator.Current.Get<IAudioService>().PlayRandomSFXFromList(footstepClips, 0.05f, 0.05f);
```

### ✔ Volume & Settings
```csharp
var audioService = ServiceLocator.Current.Get<IAudioService>();
audioService.SetMusicVolume(0.7f); // Sets music to 70%
audioService.ToggleSFX(false);      // Mutes all sound effects
```

# 💬 MessageManager

The MessageManager provides a queue-based system to display transient UI notifications to the user (e.g., "Game Saved", "Insufficient Funds"). It handles dynamic prefab instantiation, localization resolution, and icon assignment automatically.

### Usage Examples

The manager supports four primary ways to display messages, depending on whether you are using localization, raw text, custom Sprite references, or custom message prefabs.

```csharp
using TitusGames.Framework;
using UnityEngine;

if (ServiceLocator.Current.TryGet<IMessageService>(out var messageService))
{
    // 1. Localized Message: Uses a JSON key for translation
    messageService.ShowMessage("game_saved_key", "info_icon_name");

    // 2. Direct Message: Displays raw string (bypasses localization)
    messageService.ShowMessageDirectly("Connection Lost!", "error_icon_name");

    // 3. Sprite Injection: Passes an explicit Sprite object directly
    Sprite customIcon = Resources.Load<Sprite>("UI/Icons/special_event");
    messageService.ShowMessageWithSprite("event_key", customIcon);
}
```
### ✔ How it Works

**1.Dynamic Resolution**: When you call a Show method, the manager looks for a prefab in /Resources/UI/Messaging/ (or your project's equivalent path).

**2.Queue System**: If multiple messages are triggered rapidly, they are added to an internal queue and displayed sequentially to prevent overlapping UI clutter.

**3.Automatic Cleanup**: Each message prefab is automatically instantiated, faded in, displayed for a configurable duration, faded out, and destroyed.

**4.Scene Independence**: The manager persists across scenes (DontDestroyOnLoad) and automatically searches for a MessageContainer (RectTransform) in the current active scene to act as the parent for new message instances.

### ✔ Customization
You can adjust the behavior of the manager via the Inspector on the MessageManager object:

Message Duration: How long the message remains visible (default is 3 seconds).

Fade Speed: The speed of the fade-in/fade-out animations.

Containers: The manager will automatically register the MessageContainer found in your scene; however, you can manually override this via RegisterContainer(myRectTransform).


# 📘 License

MIT License — free for personal and commercial use.


---