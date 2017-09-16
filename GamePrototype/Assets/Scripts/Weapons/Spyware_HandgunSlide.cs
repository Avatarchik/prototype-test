using UnityEngine;

namespace Spyware
{
    public class Spyware_HandgunSlide : Spyware_Interactable
    {
        public Spyware_Handgun Handgun;
        public CurrentSlideState currentState;
        public CurrentPosition currentPosition;
        public CurrentPosition previousPosition;
        private AudioSource audSource;
        public AudioClip slideRearwardLock;
        public AudioClip slideForwardLock;

        protected override void Awake()
        {
            base.Awake();
            audSource = GetComponent<AudioSource>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Handgun.Slide.transform.localPosition.z == Handgun.slideMax.z)
            {
                Debug.Log("Current Slide Position = Max");
                this.currentPosition = CurrentPosition.Max;
            }
            else if (Handgun.Slide.transform.localPosition.z == Handgun.slideMin.z)
            {
                Debug.Log("Current Slide Position = Min");
                this.currentPosition = CurrentPosition.Min;
            }
            else
            {
                Debug.Log("Current Slide Position = Mid");
                this.currentPosition = CurrentPosition.Mid;
            }
            if (this.currentPosition == CurrentPosition.Min && this.previousPosition != CurrentPosition.Min)
            {
                this.audSource.PlayOneShot(this.slideForwardLock, 0.1f);
            }
            if (this.currentPosition == CurrentPosition.Max && this.previousPosition != CurrentPosition.Max)
            {
                if (currentState == CurrentSlideState.Held)
                {
                    audSource.PlayOneShot(slideRearwardLock, 0.1f);
                }
            }
            switch (this.currentState)
            {
                /* For Later Logic
                case CurrentSlideState.Free:
                    if ()
                    {
                        break;
                    }
                    break;
                    */
                case CurrentSlideState.Held:
                    if (this.currentPosition != CurrentPosition.Max)
                    {
                        Vector3 grabHand = this.transform.InverseTransformPoint(attachPointTransform.transform.position);
                        this.transform.localPosition = Handgun.ClosestPointOnLine(Handgun.slideMin, Handgun.slideMax, -grabHand);
                        break;
                    }
                    break;
            }

            this.previousPosition = this.currentPosition;
        }

        public override void BeginInteraction(Spyware_Hand hand)
        {
            base.BeginInteraction(hand);
            currentState = Spyware_HandgunSlide.CurrentSlideState.Held;
        }

        public override void EndInteraction(Spyware_Hand hand)
        {
            base.EndInteraction(hand);
            this.currentState = Spyware_HandgunSlide.CurrentSlideState.Free;
        }

        public enum CurrentSlideState
        {
            Free,
            Held,
        }

        public enum CurrentPosition
        {
            Min,
            Mid,
            Max,
        }
    }
}