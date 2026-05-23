<p align="center">
  <img src="Media~/logo1.png" width="300" alt="Framework Logo">
</p>

# TitusGames Framework (v2.0.0)

A modular, enterprise-ready Unity package designed for rapid **2D and 3D** game assembly. It includes streamlined subsystems for lifecycle initialization, scene routing, nested window management, deep localization streaming, dynamic audio mixing, and queue-driven messaging overlays.

Version 2.0.0 migrates the entire framework core code to an optimized, decoupled UPM package architecture running custom Assembly Definitions (`.asmdef`) and native namespaces to insulate package logic from user land scripts.

---

## Update 2.0:

### 🏗 Architectural Paradigm: The `Runtime` Layer

The framework utilizes a professional modular architecture. To maintain project cleanliness and prevent source-code collision, system logic is strictly decoupled from local assets. 

The core engine is structured cleanly inside the invisible package directory:

```text
com.titusgames.framework/
├── Runtime/
│   ├── Scripts/     # Pre-compiled managers bounded by TitusGames.Framework.asmdef
│   └── Resources/   # Package defaults (e.g., DefaultMessagePrefab, fallback language data)
├── Samples~/        # Hidden source files imported safely via the Package Manager UI
└── Media~/          # Static branding and logo assets
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

### Why use the Sandbox?
In a team, editing the "Main" or "Boot" scene frequently leads to Git merge conflicts. The Sandbox allows you to:

* **Isolated Testing:** Create your own "Test" scene to build a specific mechanic without touching the production flow.
* **Full System Access:** The Sandbox automatically loads the Audio, Window, Localization, and Scene managers, just like the real Boot scene.
* **Zero Setup:** Simply open the Sandbox scene and start dragging your new prefabs or scripts in; the framework backbone is already initialized and running.

> [!TIP]
> **Best Practice:** Keep the `Boot` scene reserved for the final game flow and use `Sandbox` (or duplicates of it) for daily development, prototyping, and isolated feature testing.

---

## 📂 Project Structure

_Framework/<br>
│<br>
├── Managers/<br>
│ ├── Boot.cs<br>
│ ├── SceneManager.cs<br>
│ ├── WindowManager.cs<br>
│ ├── LocalizationManager.cs<br>
│ └── AudioManager.cs<br>
│ └── MessageManager.cs<br>
│<br>
├── UI/<br>
│ └── Windows/<br>
│ └── (Your Window Scripts + Prefabs)<br>
│<br>
├── Resources/<br>
│ ├── Audio/<br>
│ │ └── (Place your .wav / .mp3 files here)<br>
│ └── Languages/<br>
│ └── (JSON files for each language)<br>
│<br>
├── Scenes/<br>

│ └── TestingScenes/<br>
│ │ └── Sandbox.unity<br>
│ ├── Boot.unity<br>
│ ├── MainMenu.unity<br>
│ ├── Game.unity<br>
│<br>
└── ThirdParty/<br>
└── Utils/<br>
└── MiniJSON (for localization)<br>



---

# 🎮 SceneManager

The `SceneManager` lets you load scenes by name and exit the game. Every new scene that you create should have a canvas which needs a WindowRoot and a MessageContainer gameobject.

### ✔ Load a Scene
```csharp
SceneManager.Instance.LoadScene("GameScene");
```

### ✔ Exit the Game
```csharp
SceneManager.Instance.ExitGame();
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
**Chain of Responsibility**: The manager now implements the ICancelInputHandler interface, allowing it to hand off input events to other game systems (e.g., gameplay pause menus) if no windows are currently open.

**Automatic Input Mapping**: Automatically switches between Player and UI Action Maps based on whether a window is active.

**Settings-Driven Windows**: By attaching a UIWindowSettings component to your window prefab, the WindowManager automatically respects your rules for that specific window (Time freezing, Cursor visibility, and Input modes).

**Universal Fallback**: If your project isn't using PlayerInput, the manager includes a hardware-level fallback to ensure the Escape key always closes the active window.

### ✔ How to Create a New Window

**1. Prepare the Prefab**: Create your UI panel. For advanced control (like freezing time or showing the cursor), attach the UIWindowSettings script to the root of your prefab.

**2. Opening via Button**:

* Add a Button to your UI.

* Attach the UI_OpenWindow script to the button object.

* Assign your window prefab to the Window Prefab slot in the inspector.

* In the OnClick() event, drag the UI_OpenWindow component into the slot and select WindowManager.OpenWindow.

**3. Opening via Code**:

```chsarp
// Programmatically open a window
WindowManager.Instance.OpenWindow(myWindowPrefab);
```

### ✔ Closing Windows
*   **Automatic:** The manager natively listens for `UI/Cancel` inputs. When a user presses "Escape" (or the cancel button), the top-most window in the stack is closed automatically.
*   **Programmatic:**
    ```csharp
    // Close the top-most active window
    WindowManager.Instance.CloseTopWindow();

    // Force close all active windows
    WindowManager.Instance.CloseAllWindows();


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

_Framework/Resources/Languages


Add or modify your keys.

Save — the manager automatically reloads them at runtime.

# 🔊 AudioManager

The AudioManager provides a generic, string-based interface for managing audio. You do not need to modify the underlying scripts to add new sounds; the system automatically resolves audio clips stored in your project's Resources folder using the filename as a unique identifier.

[!WARNING]
Manual File Placement Required: To keep the package lightweight, the framework does not include default audio. You must add your audio files to your /Resources/ directory for the manager to function.

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
// Plays "MainMenuTheme.mp3" located in Resources/Audio/Music
AudioManager.Instance.PlayMusic("MainMenuTheme");
```

### ✔ Playing Sound Effects (SFX)
```csharp
// Plays "click.wav" located in Resources/Audio/SFX
AudioManager.Instance.PlaySFX("click");
```

```csharp
// Play with a specific volume multiplier (e.g., 50% volume)
AudioManager.Instance.PlaySFX("explosion", 0.5f);
```

### ✔ Randomized SFX (Adding Variety)
You can now add dynamic variety to repetitive sounds (like footsteps or weapon fire) by randomizing pitch and volume to prevent "ear fatigue."
```csharp
// Randomize pitch (±0.1) and volume (within 0.1 of master volume)
AudioManager.Instance.PlayRandomizedSFX("jump", 0.1f, 0.1f);

// Pick a random clip from an array and play it with randomization
string[] footstepSounds = { "step1", "step2", "step3" };
AudioManager.Instance.PlayRandomSFXFromList(footstepSounds, 0.05f, 0.05f);
```

### ✔ Volume & Settings
```csharp
AudioManager.Instance.SetMusicVolume(0.7f); // Sets music to 70%
AudioManager.Instance.ToggleSFX(false);      // Mutes all sound effects
```

# 💬 MessageManager

The MessageManager provides a queue-based system to display transient UI notifications to the user (e.g., "Game Saved", "Insufficient Funds"). It handles dynamic prefab instantiation, localization resolution, and icon assignment automatically.

### Usage Examples

The manager supports three primary ways to display messages, depending on whether you are using localization, raw text, or custom Sprite references.

```chsarp
using TitusGames.Framework;
using UnityEngine;

// 1. Localized Message: Uses a JSON key for translation
MessageManager.Instance.ShowMessage("game_saved_key", "info_icon_name");

// 2. Direct Message: Displays raw string (bypasses localization)
MessageManager.Instance.ShowMessageDirectly("Connection Lost!", "error_icon_name");

// 3. Sprite Injection: Passes an explicit Sprite object directly
Sprite customIcon = Resources.Load<Sprite>("UI/Icons/special_event");
MessageManager.Instance.ShowMessageWithSprite("event_key", customIcon);
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

Containers: The manager will automatically register the MessageContainer found in your scene; however, you can manually override this via MessageManager.Instance.RegisterContainer(myRectTransform).


# 📘 License

MIT License — free for personal and commercial use.


---
