using UnityEngine;

namespace TitusGames.Framework
{ 
public class UI_CloseWindow : MonoBehaviour
{
    public void Close()
    {
        WindowManager.Instance.CloseTopWindow();
    }
}
}
