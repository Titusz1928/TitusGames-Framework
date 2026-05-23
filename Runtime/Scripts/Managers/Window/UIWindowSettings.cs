using UnityEngine;

namespace TitusGames.Framework{

public enum WindowInputMode
{
    GameplayAndUI, // Player can still move
    UIOnly         // Player movement is disabled (Debug/Pause style)
}

public class UIWindowSettings : MonoBehaviour
{
    public WindowInputMode inputMode = WindowInputMode.UIOnly;
    public bool showCursor = true;
    public bool freezeTime = false; // New field
}

}