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
        }

        protected override void Start()
        {
            velocityEstimator = GetComponent<Spyware_VelocityEstimator>();
            velocityEstimator.BeginEstimatingVelocity();
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

                if (interactionPoint != null)
                {
                    Vector3 handPosition = currenthand.transform.position;
                    Vector3 interactablePosition = interactionPoint.transform.position;
                    Quaternion handRotation = currenthand.transform.rotation;
                    Quaternion interactableRotation = interactionPoint.transform.rotation;
                    Vector3 positionDelta = handPosition - interactablePosition;
                    Quaternion rotationDelta = handRotation * Quaternion.Inverse(interactableRotation);

                    if (SecondGrip != null && SecondGrip.IsEnabled)
                    {
                        Vector3 forward = transform.InverseTransformPoint(m_secondGrip.interactionPoint.position);
                        forward.y = -forward.y;
                        Vector3 vector3_1 = transform.TransformDirection(forward);
                        Quaternion quaternion_1 = Quaternion.LookRotation((m_secondGrip.currenthand.transform.position + vector3_1 - currenthand.transform.position).normalized, Vector3.Cross(m_secondGrip.currenthand.transform.position - currenthand.transform.position, currenthand.transform.right)) * interactionPoint.localRotation;
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

                else if (interactionPoint == null)
                {
                    Vector3 positionDelta;
                    Quaternion rotationDelta;

                    positionDelta = attachPointTransform.position - transform.position;
                    rotationDelta = attachPointTransform.rotation * Quaternion.Inverse(transform.rotation);

                    rotationDelta.ToAngleAxis(out angle, out axis);

                    if (angle > 180)
                        angle -= 360;

                    if (angle != 0)
                    {
                        Vector3 AngularTarget = angle * axis;
                        rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, AngularTarget, 10f * (Time.fixedDeltaTime * 1000));
                    }

                    Vector3 VelocityTarget = positionDelta / Time.fixedDeltaTime;
                    rb.velocity = Vector3.MoveTowards(rb.velocity, VelocityTarget, 10f);
                }

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
            BeginInteraction(hand);
            this.currenthand.ForceSetInteractable(this);
        }

        public override void EndInteraction(Spyware_Hand hand)
        {
            rb.useGravity = true;
            if (this.currenthand != null)
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
            if (!IsHeld || currenthand == null)
                return;
            currenthand.ForceSetInteractable(null);
            Destroy(attachPointTransform);
        }

        public void SetParent(Transform t)
        {
            transform.SetParent(t);
        }
    }
}