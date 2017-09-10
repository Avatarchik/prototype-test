using UnityEngine;

namespace Spyware
{
    public class Spyware_Handgun : Spyware_FireArm
    {
        public float slideMaxz;
        public float slideMinz;
        public Spyware_HandgunSlide Slide;
        private AudioSource m_aud;

        protected override void Awake()
        {
            base.Awake();
            m_aud = GetComponent<AudioSource>();
        }

        public override void UpdateInteraction(Spyware_Hand hand)
        {
            base.UpdateInteraction(hand);
        }
    }
}