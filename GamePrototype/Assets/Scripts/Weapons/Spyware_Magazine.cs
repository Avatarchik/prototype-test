using UnityEngine;

namespace Spyware
{
    public class Spyware_Magazine : Spyware_PhysInteractable
    {
        public int currentAmount = 30;
        public int maxCapacity = 30;
        public bool IsExtractable = true;
        public Transform roundEjectTransform;
        public Spyware_RoundCalibre magRoundCalibre;
        public MagazineType magazineType;
        public MagazineState currentState;
        public Spyware_FireArm currentFireArm;
        public bool CanMagazineBeDroppedIn;
        public GameObject loadedMagDisplay;
        public GameObject bulletPrefab;
        private float timeSinceRoundAdded;
        //public GameObject[] StaticBullets;

        public override bool IsInteractable()
        {
            return this.currentState == MagazineState.Free;
        }

        public float TimeSinceRoundAdded
        {
            get
            {
                return this.timeSinceRoundAdded;
            }
        }

        public bool IsFull()
        {
            return currentAmount >= maxCapacity;
        }

        protected override void Awake()
        {
            base.Awake();
            //audioSource = this.GetComponent<AudioSource>();
        }

        public bool IsLoaded()
        {
            return this.currentAmount > 0 && this.IsExtractable;
        }
        /*
        public override void EndInteraction(Spyware_Hand hand)
        {
            base.EndInteraction(hand);
        }
        */
        public void AddRound(Spyware_Round round, bool Sound, bool UpdateStaticBullets)
        {
            if (this.currentAmount < this.maxCapacity)
            {
                timeSinceRoundAdded = 0.0f;
                ++currentAmount;
                /*
                if ((UnityEngine.Object)this.AudClip_RoundInsert != (UnityEngine.Object)null && Sound)
                    this.m_aud.PlayOneShot(this.AudClip_RoundInsert, 0.1f);
                    */
            }
            if (!UpdateStaticBullets)
                return;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            Debug.Log(currentAmount);
            if (currentAmount == 0)
                loadedMagDisplay.gameObject.SetActive(false);
            if (currentAmount > 0)
                loadedMagDisplay.gameObject.SetActive(true);
            if (timeSinceRoundAdded < 5.0)
                this.timeSinceRoundAdded += Time.deltaTime;
            if (!this.IsHeld || (!(this.currenthand.otherHand.CurrentInteractable != null) || !(this.currenthand.otherHand.CurrentInteractable is Spyware_FireArm)))
                return;
            Spyware_FireArm currentInteractable = this.currenthand.otherHand.CurrentInteractable as Spyware_FireArm;
            if (currentInteractable.MagazineType != this.magazineType)
                return;
        }

        public void RemoveRound()
        {
            if (currentAmount > 0)
            {
                --currentAmount;
            }
        }

        public GameObject RemoveRound(bool b)
        {
            GameObject prefab = bulletPrefab;
            --currentAmount;
            return prefab;
        }

        /*
        private void UpdateStaticBullets()
        {
            if (currentAmount <= 0)
            {
                loadedMagDisplay.SetActive(false);
            }
            else
            {
                loadedMagDisplay.SetActive(true);
            }
            
            var roundsInMag = StaticBullets.Length;
            Debug.Log(roundsInMag);
        }
  */

        public override void UpdateInteraction(Spyware_Hand hand)
        {
            base.UpdateInteraction(hand);
            if (!(roundEjectTransform != null) || !this.IsLoaded() || (!hand.Input.TouchPadDown || Vector2.Angle(hand.Input.TouchPadAxes, Vector2.up) >= 45.0))
                return;
            if (hand.otherHand.CurrentInteractable == null && Vector3.Distance(this.roundEjectTransform.position, hand.otherHand.transform.position) < 0.150000005960464)
            {
                Spyware_Round component = Instantiate(this.RemoveRound(false), this.roundEjectTransform.position, this.roundEjectTransform.rotation).GetComponent<Spyware_Round>();
                hand.otherHand.ForceSetInteractable(component);
                component.BeginInteraction(hand.otherHand);
            }
            else if (hand.otherHand.CurrentInteractable is Spyware_Round && (((Spyware_Round)hand.otherHand.CurrentInteractable).calibre == this.magRoundCalibre && Vector3.Distance(hand.transform.position, hand.otherHand.transform.position) < 0.150000005960464))
            {
                this.RemoveRound();
            }
            else
            {
                GameObject gameObject = Instantiate(this.RemoveRound(false), this.roundEjectTransform.position, this.roundEjectTransform.rotation);
                gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 0.5f);
            }
        }

        public enum MagazineState
        {
            Loaded,
            Free,
        }
    }
}