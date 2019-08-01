using TMPro;

namespace XFramework.UI
{
    [UnityEngine.RequireComponent(typeof(TMP_Dropdown))]
    public class GUTMPDropdown : BaseGUI
    {
        public TMP_Dropdown dropdown;

        private void Reset()
        {
            dropdown = transform.GetComponent<TMP_Dropdown>();
        }

        public void AddListener(UnityEngine.Events.UnityAction<int> call)
        {
            dropdown.onValueChanged.AddListener(call);
        }
    }
}