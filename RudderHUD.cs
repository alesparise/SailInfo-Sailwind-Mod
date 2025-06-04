using UnityEngine;

namespace SailInfo
{
    public class RudderHUD : MonoBehaviour
    {
        private BoatInfo boatInfo;
        private GoPointerButton button;
        public void Awake()
        {
            boatInfo = gameObject.AddComponent<BoatInfo>();
            button = GetComponent<GoPointerButton>();
        }
        public void Update()
        {
            if (button.IsLookedAt() || button.IsStickyClicked() || button.IsCliked())
            button.description = boatInfo.RudderHUD();
        }
    }
}
