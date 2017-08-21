using UnityEngine;

namespace Spyware
{
    public class Spyware_PhysInteractable : Spyware_Interactable
    {
        public Spyware_VelocityEstimator velocityEstimator;
        public bool IsSecondHeld;

        [HideInInspector]
        public Rigidbody rb { get; set; }

        private Spyware_SecondGrip m_secondGrip;
        private Spyware_SecondGrip savedGrip;

        protected Quaternion quaternionZero = Quaternion.identity;

        [HideInInspector]
        public Spyware_SecondGrip SecondGrip
        {
            get
            {
                return m_secondGrip;
            }
            set
            {
                m_secondGrip = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody>();
            rb.maxAngularVelocity = 100f;
        }

        public void Start()
        {
            velocityEstimator = GetComponent<Spyware_VelocityEstimator>();
            velocityEstimator.BeginEstimatingVelocity();
        }

        protected virtual Vector3 GetHandPosition()
        {
            return hand.transform.position;
        }

        protected virtual Vector3 GetInteractablePosition()
        {
            Vector3 position = interactionPoint.transform.position;
            if (IsSecondHeld == true)
                return attachPointTransform.position;
            return position;
        }

        protected virtual Quaternion GetHandRotation()
        {
            return hand.transform.rotation;
        }

        protected virtual Quaternion GetInteractableRotation()
        {
            Quaternion rotation = interactionPoint.transform.rotation;
            if (IsSecondHeld == true)
                return attachPointTransform.rotation;
            return rotation;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (interactionPoint == null)
            {
                Debug.LogError(this + "Needs An Interaction Point");
                return;
            }
            if (IsHeld == true && interactionPoint != null)
            {
                Vector3 handPosition = GetHandPosition();
                Vector3 interactablePosition = GetInteractablePosition();
                Quaternion handRotation = GetHandRotation();
                Quaternion interactableRotation = GetInteractableRotation();

                float angle;
                Vector3 axis;

                Vector3 positionDelta = handPosition - interactablePosition;
                Quaternion rotationDelta = handRotation * Quaternion.Inverse(interactableRotation);

                if (SecondGrip != null && SecondGrip.IsEnabled)
                {
                    Vector3 forward = transform.InverseTransformPoint(m_secondGrip.interactionPoint.position);
                    forward.y = -forward.y;
                    Vector3 vector3_1 = transform.TransformDirection(forward);
                    Quaternion quaternion_1 = Quaternion.LookRotation((m_secondGrip.hand.transform.position + vector3_1 - hand.transform.position).normalized, Vector3.Cross(m_secondGrip.hand.transform.position - hand.transform.position, hand.transform.right)) * interactionPoint.localRotation;
                    float t = Mathf.Min(Quaternion.Angle(quaternionZero, quaternion_1) / 2f, 1f);
                    this.quaternionZero = Quaternion.Slerp(quaternionZero, quaternion_1, t);
                    rotationDelta = quaternion_1 * Quaternion.Inverse(interactableRotation);
                }

                rotationDelta.ToAngleAxis(out angle, out axis);

                if (angle > 180)
                    angle -= 360;

                if (angle != 0)
                {
                    Vector3 angTarget = Time.fixedDeltaTime * angle * axis * 60f;
                    rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, angTarget, 10f * (Time.fixedDeltaTime * 1000f));
                }

                Vector3 velTarget = positionDelta / Time.fixedDeltaTime;
                rb.velocity = Vector3.MoveTowards(rb.velocity, velTarget, 10f) * Time.fixedDeltaTime * 110f;
            }
        }

        public override void BeginInteraction(Spyware_Hand hand)
        {
            if (transform.parent == null || transform.parent != hand.cameraRig)
                SetParent(hand.cameraRig);
            rb.useGravity = false;
            rb.isKinematic = false;
            base.BeginInteraction(hand);
        }

        public virtual void BeginInteractionSecondGrip(Spyware_Hand hand, Spyware_SecondGrip grip)
        {
            IsSecondHeld = true;
            savedGrip = grip;
            BeginInteraction(hand);
            this.hand.ForceSetInteractable(this);
        }

        public override void EndInteraction(Spyware_Hand hand)
        {
            rb.useGravity = true;
            if (this.hand != null)
            {
                rb.velocity = velocityEstimator.GetVelocityEstimate();
                rb.angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
            }
            else
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            SetParent(null);
            base.EndInteraction(hand);
        }

        public override void ForceBreakInteraction()
        {
            base.ForceBreakInteraction();
            if (rb == null)
                return;
            rb.useGravity = true;
        }

        public override void UpdateInteraction(Spyware_Hand hand)
        {
            base.UpdateInteraction(hand);
        }

        public new void OnDestroy()
        {
            if (!IsHeld || hand == null)
                return;
            hand.ForceSetInteractable(null);
            Destroy(attachPointTransform);
        }

        public void SetParent(Transform t)
        {
            transform.SetParent(t);
        }
    }
}