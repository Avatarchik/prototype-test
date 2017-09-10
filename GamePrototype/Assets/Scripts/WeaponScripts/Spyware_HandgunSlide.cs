using UnityEngine;

namespace Spyware
{
    public class Spyware_HandgunSlide : Spyware_PhysInteractable
    {
        public Spyware_Handgun handgun;
        public Spyware_HandgunSlide.CurrentSlideState State;
        private AudioSource aud;
        public AudioClip SlideSound1;
        public AudioClip SlideSound2;

        public override bool IsInteractable()
        {
            return true;
        }

        public void Fire()
        {

        }

        protected override void Awake()
        {
            base.Awake();
            aud = GetComponent<AudioSource>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (State == CurrentSlideState.Held)
            {
                float zPos = transform.InverseTransformPoint(attachPointTransform.transform.position).z;
                Vector3 position = transform.localPosition;
                position.z = Mathf.Clamp(zPos, handgun.slideMinz, handgun.slideMaxz);
            }
        }

        public override void BeginInteraction(Spyware_Hand hand)
        {
            base.BeginInteraction(hand);
            State = CurrentSlideState.Held;
        }

        public override void EndInteraction(Spyware_Hand hand)
        {
            base.EndInteraction(hand);
            State = CurrentSlideState.Free;
        }

        public enum CurrentSlideState
        {
            Free,
            Held,
        }
    }
}