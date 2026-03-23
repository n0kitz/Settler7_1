using UnityEngine;
using UnityEngine.InputSystem;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Handles save/load keyboard shortcuts.
    /// F5 = Quick Save, F9 = Quick Load.
    /// Saves to Application.persistentDataPath.
    /// </summary>
    public class SaveLoadController : MonoBehaviour
    {
        private string SavePath => System.IO.Path.Combine(
            Application.persistentDataPath, "quicksave.sav");

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.f5Key.wasPressedThisFrame)
                QuickSave();

            if (Keyboard.current.f9Key.wasPressedThisFrame)
                QuickLoad();
        }

        private void QuickSave()
        {
            var gc = GameController.Instance;
            if (gc == null || gc.State == null) return;

            SaveSystem.SaveToFile(gc.State, SavePath);
            Debug.Log($"[Save] Quick saved to {SavePath}");
        }

        private void QuickLoad()
        {
            var gc = GameController.Instance;
            if (gc == null || gc.State == null) return;

            if (!System.IO.File.Exists(SavePath))
            {
                Debug.LogWarning("[Save] No quicksave found.");
                return;
            }

            SaveSystem.LoadFromFile(gc.State, SavePath);
            gc.RefreshAllOwnership();
            Debug.Log("[Save] Quick loaded.");
        }
    }
}
