using UnityEngine;

namespace Spyware
{
    public class Spyware_Interactable : MonoBehaviour
    {
        [HideInInspector]
        public Spyware_Hand hand;
        [Tooltip("The Rotation And Position The Interactable Item Is Attached/Positioned To")]
        protected Transform attachPointTransform;
        public float EndInteractionDistance = 0.25f;
        public bool EndInteractionIfDistant = true;

        private bool isHovered;
        private bool isHeld;

        public bool EnableGravityOnDetach;

        protected Collider[] colliders;

        public bool IsHovered
        {
            get
            {
                return isHovered;
            }
            set
            {
                bool isHovered = this.isHovered;
                this.isHovered = value;
                if (this.isHovered && !isHovered)
                {
                    OnHoverStart();
                }
                else
                {
                    if (!this.isHovered || !isHovered)
                        return;
                    OnHoverEnd();
                }
            }
        }

        public bool IsHeld
        {
            get
            {
                return isHeld;
            }
            set
            {
                isHeld = value;
            }
        }

        public virtual bool IsInteractable()
        {
            return true;
        }

        protected virtual void OnHoverStart()
        {
            SteamVR_Controller.Device controller = hand.GetComponent<SteamVR_Controller.Device>();
            controller.TriggerHapticPulse(300);
        }

        protected virtual void OnHoverEnd()
        {

        }

        protected virtual void OnHoverStay()
        {

        }

        protected virtual void Awake()
        {
            colliders = GetComponentsInChildren<Collider>(true);
        }

        public void UpdateGrabPointTransform()
        {
            if (hand == null)
                return;
            attachPointTransform.position = hand.transform.position;
            attachPointTransform.rotation = hand.transform.rotation;
        }

        public virtual void BeginInteraction(Spyware_Hand hand)
        {
            if (IsHeld && this.hand != hand && this.hand != null)
                this.hand.EndInteractionIfHeld(this);
            IsHeld = true;
            this.hand = hand;
            if (attachPointTransform == null)
                attachPointTransform = new GameObject("attachPointTransform").transform;
            attachPointTransform.SetParent(transform);
            attachPointTransform.position = this.hand.transform.position;
            attachPointTransform.rotation = this.hand.transform.rotation;
        }

        public virtual void UpdateInteraction(Spyware_Hand hand)
        {
            IsHeld = true;
            this.hand = hand;
        }

        public virtual void EndInteraction(Spyware_Hand hand)
        {
            this.hand = null;
            IsHeld = false;
            Destroy(attachPointTransform.gameObject);
        }

        public virtual void Test(Spyware_Hand hand)
        {

        }

        public virtual void ForceBreakInteraction()
        {
            Spyware_Hand hand = this.hand;
            if (hand == null)
                return;
            hand.EndInteractionIfHeld(this);
            EndInteraction(hand);
            hand.currentHandState = Spyware_Hand.HandState.Empty;
        }

        public void OnDestroy()
        {
            if (!IsHeld || hand == null)
                return;
            hand.ForceSetInteractable(null);
        }

        protected virtual void Update()
        {
            if (!IsHovered)
                return;
            OnHoverStay();
        }

        protected virtual void FixedUpdate()
        {
            if (!IsHeld)
                return;
            TestHandDistance();
        }

        public virtual void TestHandDistance()
        {
            if (!EndInteractionIfDistant)
                return;
            if (transform == null)
            {
                if (Vector3.Distance(hand.transform.position, transform.position) < (double)EndInteractionDistance)
                    return;
                ForceBreakInteraction();
            }
            else
            {
                if (Vector3.Distance(hand.transform.position, transform.position) < EndInteractionDistance)
                    return;
                ForceBreakInteraction();
            }
        }
    }
}