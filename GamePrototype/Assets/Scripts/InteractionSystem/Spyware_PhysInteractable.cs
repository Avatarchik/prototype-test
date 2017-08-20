using UnityEngine;

namespace Spyware
{
    public class Spyware_PhysInteractable : Spyware_Interactable
    {
        [Tooltip("If you have a specific point you'd like the object held at, create a transform there and set it to this variable")]
        public Transform interactionPoint;
        public Spyware_VelocityEstimator velocityEstimator;
        [HideInInspector]
        public Rigidbody rb { get; set; }

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
            return position;
        }

        protected virtual Quaternion GetHandRotation()
        {
            return hand.transform.rotation;
        }

        protected virtual Quaternion GetInteractableRotation()
        {
            Quaternion rotation = interactionPoint.transform.rotation;
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

                rotationDelta.ToAngleAxis(out angle, out axis);

                if (angle > 180)
                    angle -= 360;

                if (angle != 0)
                {
                    Vector3 angTarget = angle * axis;
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

        public override void EndInteraction(Spyware_Hand hand)
        {
            rb.useGravity = true;
            if (this.hand != null)
            {
                rb.velocity = velocityEstimator.GetVelocityEstimate();
                rb.angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
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