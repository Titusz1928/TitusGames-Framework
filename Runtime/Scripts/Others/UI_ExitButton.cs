using UnityEngine;

namespace TitusGames.Framework
{
    
public class UI_ExitButton : MonoBehaviour
{
    public void QuitGame()
    {
        SceneManagerEX.Instance.QuitGame();
    }
}
}
