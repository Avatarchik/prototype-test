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

        protected Quaternion quaternionZero = Quaternion.identity;
        protected float AttachedPositionMagic = 2000f;

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
            velocityEstimator = GetComponent<Spyware_VelocityEstimator>();
        }

        protected override void Start()
        {

        }

        protected virtual Vector3 GetHandPosition()
        {
            Vector3 handPosition;
            if (interactionPoint != null)
                handPosition = currenthand.transform.position;
            else
                handPosition = this.attachPointTransform.position;
            return handPosition;
        }

        protected virtual Quaternion GetHandRotation()
        {
            Quaternion handRotation;
            if (interactionPoint != null)
                handRotation =currenthand.transform.rotation;
            else
                handRotation = this.attachPointTransform.rotation;
            return handRotation;
        }

        protected virtual Vector3 GetInteractablePosition()
        {
            Vector3 interactablePosition;
            if (interactionPoint != null)
                interactablePosition = interactionPoint.position;
            else
                interactablePosition = this.transform.position;
            return interactablePosition;
        }

        protected virtual Quaternion GetInteractableRotation()
        {
            Quaternion interactableRotation;
            if (interactionPoint != null)
                interactableRotation = interactionPoint.rotation;
            else
                interactableRotation = this.transform.rotation;
            return interactableRotation;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (IsHeld != true)
                return;
            if (IsHeld == true)
            {
                float angle;
                Vector3 axis;

                Vector3 handPosition = GetHandPosition();
                Vector3 interactablePosition = GetInteractablePosition();
                Quaternion handRotation = GetHandRotation();
                Quaternion interactableRotation = GetInteractableRotation();
                Vector3 positionDelta = handPosition - interactablePosition;
                Quaternion rotationDelta = handRotation * Quaternion.Inverse(interactableRotation);

                if (SecondGrip != null && SecondGrip.IsEnabled)
                {
                    Vector3 forward = transform.InverseTransformPoint(m_secondGrip.interactionPoint.position);
                    forward.y = -forward.y;
                    Vector3 vector3_1 = transform.TransformDirection(forward);
                    Quaternion quaternionLookRot = Quaternion.LookRotation((m_secondGrip.currenthand.transform.position + vector3_1 - currenthand.transform.position).normalized, Vector3.Cross(m_secondGrip.currenthand.transform.position - currenthand.transform.position, currenthand.transform.right)) * interactionPoint.localRotation;
                    float t = Mathf.Min(Quaternion.Angle(quaternionZero, quaternionLookRot) / 2f, 1f);
                    this.quaternionZero = Quaternion.Slerp(quaternionZero, quaternionLookRot, t);
                    rotationDelta = quaternionLookRot * Quaternion.Inverse(interactableRotation);
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
                rb.velocity = Vector3.MoveTowards(rb.velocity, velTarget, 10f) * Time.deltaTime * 110f;
            }
        }

        public override void BeginInteraction(Spyware_Hand hand)
        {
            if (transform.parent == null || transform.parent != hand.cameraRig)
                SetParent(hand.cameraRig);
            rb.useGravity = false;
            rb.isKinematic = false;
            base.BeginInteraction(hand);
            velocityEstimator.BeginEstimatingVelocity();
        }

        public virtual void BeginInteractionSecondGrip(Spyware_Hand hand, Spyware_SecondGrip grip)
        {
            IsSecondHeld = true;
            BeginInteraction(hand);
            this.currenthand.ForceSetInteractable(this);
        }

        public override void EndInteraction(Spyware_Hand hand)
        {
            rb.useGravity = true;
            SetParent(null);
            rb.velocity = velocityEstimator.GetVelocityEstimate();
            rb.angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
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
            if (!IsHeld || currenthand == null)
                return;
            currenthand.ForceSetInteractable(null);
        }

        public void SetParent(Transform t)
        {
            transform.SetParent(t);
        }
    }
}