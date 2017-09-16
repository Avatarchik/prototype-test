using UnityEngine;

namespace Spyware
{
    public class Spyware_Interactable : MonoBehaviour
    {
        public Transform interactionPoint;
        public InteractionStyle interactionStyle;
        [HideInInspector]
        public Spyware_Hand currenthand;
        public float EndInteractionDistance = 0.25f;
        protected float triggerCooldown = 0.5f;

        public bool EndInteractionIfDistant = true;
        public bool IsSimpleInteract;

        [HideInInspector]
        public bool LastTimeTriggerWasPressed;

        private bool isHovered;
        private bool isHeld;

        protected Transform attachPointTransform;
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
            SteamVR_Controller.Device controller = currenthand.GetComponent<SteamVR_Controller.Device>();
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

        protected virtual void Start()
        {
        }

        public virtual void BeginInteraction(Spyware_Hand hand)
        {
            if (this.IsHeld && (Object)this.currenthand != (Object)hand && (Object)this.currenthand != (Object)null)
                this.currenthand.EndInteractionIfHeld(this);
            if (attachPointTransform == null)
                attachPointTransform = new GameObject("AttachPointTransform").transform;
            attachPointTransform.SetParent(hand.transform);
            attachPointTransform.position = transform.position;
            attachPointTransform.rotation = transform.rotation;
            IsHeld = true;
            currenthand = hand;
            triggerCooldown = 0.5f;
        }

        public virtual void UpdateInteraction(Spyware_Hand hand)
        {
            IsHeld = true;
            this.currenthand = hand;
            if (!LastTimeTriggerWasPressed && this.currenthand.Input.TriggerFloat < 0.150000006000)
                LastTimeTriggerWasPressed = true;
            if (triggerCooldown <= 0.0)
                return;
            triggerCooldown -= Time.deltaTime;
        }

        public virtual void EndInteraction(Spyware_Hand hand)
        {
            hand = null;
            IsHeld = false;
        }

        public virtual void SimpleInteraction(Spyware_Hand hand)
        {
        }

        public virtual void Test(Spyware_Hand hand)
        {

        }

        public void SetAllCollidersToLayer(bool triggersToo, string layerName)
        {
            if (triggersToo)
            {
                foreach (Collider collider in colliders)
                {
                    if (collider != null)
                        collider.gameObject.layer = LayerMask.NameToLayer(layerName);
                }
            }
            else
            {
                foreach (Collider collider in colliders)
                {
                    if (collider != null && !collider.isTrigger)
                        collider.gameObject.layer = LayerMask.NameToLayer(layerName);
                }
            }
        }

        public virtual void ForceBreakInteraction()
        {
            if (currenthand == null)
                return;
            currenthand.EndInteractionIfHeld(this);
            EndInteraction(currenthand);
        }

        public void OnDestroy()
        {
            if (!IsHeld || currenthand == null)
                return;
            currenthand.ForceSetInteractable(null);
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
                if (Vector3.Distance(currenthand.transform.position, transform.position) < (double)EndInteractionDistance)
                    return;
                ForceBreakInteraction();
            }
            else
            {
                if (Vector3.Distance(currenthand.transform.position, transform.position) < EndInteractionDistance)
                    return;
                ForceBreakInteraction();
            }
        }

        public Vector3 GetClosestValidPoint(Vector3 vA, Vector3 vB, Vector3 vPoint)
        {
            Vector3 rhs = vPoint - vA;
            Vector3 normalized = (vB - vA).normalized;
            float num1 = Vector3.Distance(vA, vB);
            float num2 = Vector3.Dot(normalized, rhs);
            if (num2 <= 0.0)
                return vA;
            if ((double)num2 >= (double)num1)
                return vB;
            Vector3 vector3 = normalized * num2;
            return vA + vector3;
        }
    }
}