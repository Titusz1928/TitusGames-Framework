
namespace TitusGames.Framework{
public interface ICancelInputHandler
{
    /// <summary>
    /// Processes the cancel/escape request. 
    /// Returns TRUE if it handled the event (stops the chain). 
    /// Returns FALSE to pass it down.
    /// </summary>
    bool HandleCancel();
}
}