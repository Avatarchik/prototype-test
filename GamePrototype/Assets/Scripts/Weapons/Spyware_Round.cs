using UnityEngine;

namespace Spyware
{
    public class Spyware_Round : Spyware_PhysInteractable
    {
        public Spyware_RoundCalibre calibre;
        private bool m_isSpent;
        [HideInInspector]
        public bool isMagazineLoadable;
        private Spyware_MagazineReloadTrigger currentReloadTrigger;
        [HideInInspector]
        public Spyware_Round currentRound;
        public bool IsCaseless;


        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!(currentReloadTrigger != null) || !this.IsHeld && !this.currentReloadTrigger.currentMag.CanMagazineBeDroppedIn || (this.currentReloadTrigger.currentMag.magRoundCalibre != this.calibre || (double)this.currentReloadTrigger.currentMag.TimeSinceRoundAdded <= 0.5))
                return;
            currentReloadTrigger.currentMag.AddRound(this, true, true);
            this.ForceBreakInteraction();
            Destroy(this.gameObject);
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (this.isMagazineLoadable && collider.gameObject.tag == "MagazineReloadTrigger")
            {
                Spyware_MagazineReloadTrigger component = collider.gameObject.GetComponent<Spyware_MagazineReloadTrigger>();
                if (component != null && (UnityEngine.Object)component.currentMag != (UnityEngine.Object)null && (component.currentMag.magRoundCalibre == this.calibre && !component.currentMag.IsFull()) && ((UnityEngine.Object)component.currentMag.currentFireArm == (UnityEngine.Object)null || component.currentMag.CanMagazineBeDroppedIn))
                    this.currentReloadTrigger = component;
            }
            if (!collider.gameObject.CompareTag("FireArmRound"))
                return;
            Spyware_Round component1 = collider.gameObject.GetComponent<Spyware_Round>();
            if (component1.calibre != this.calibre)
                return;
            this.currentRound = component1;
        }

        public void OnTriggerExit(Collider collider)
        {
            if (this.isMagazineLoadable && collider.gameObject.tag == "ArmMagazineReloadTrigger" && ((UnityEngine.Object)this.currentReloadTrigger != (UnityEngine.Object)null && this.currentReloadTrigger.gameObject == collider.gameObject))
                this.currentReloadTrigger = null;
            if (!(collider != null) || (!(this.currentRound != null) || !collider.gameObject.CompareTag("FireArmRound")) || !(collider.gameObject == this.currentRound.gameObject))
                return;
            this.currentRound = null;
        }

    }
}