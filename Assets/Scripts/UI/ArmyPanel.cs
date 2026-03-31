using UnityEngine;
namespace Settlers.UI
{
    public class ArmyPanel : MonoBehaviour
    {
        public bool IsVisible { get; private set; }
        public void Show() { gameObject.SetActive(true); IsVisible = true; }
        public void Hide() { gameObject.SetActive(false); IsVisible = false; }
        public void Toggle() { if (IsVisible) Hide(); else Show(); }
    }
}
