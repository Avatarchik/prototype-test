using UnityEngine;

namespace Spyware
{
    public class Spyware_SecondGrip : Spyware_Interactable
    {
        public Spyware_PhysInteractable mainObject;
        public bool IsEnabled;


        public override bool IsInteractable()
        {
            return !IsHeld && !mainObject.IsSecondHeld;
        }

        public override void BeginInteraction(Spyware_Hand hand)
        {
            if (!mainObject.IsHeld)
            {
                mainObject.BeginInteractionSecondGrip(hand, this);
            }
            else
            {
                base.BeginInteraction(hand);
                if (IsEnabled)
                    mainObject.SecondGrip = this;
            }
        }

        public override void EndInteraction(Spyware_Hand hand)
        {
            base.EndInteraction(hand);
            if (IsEnabled && mainObject.SecondGrip == this)
                mainObject.SecondGrip = null;
        }

        public void SetFunctionality(bool b)
        {
            IsEnabled = !IsEnabled;
            if (IsEnabled)
            {
                mainObject.SecondGrip = this;
            }
            else
            {
                if (!(mainObject.SecondGrip == this))
                    return;
                mainObject.SecondGrip = null;
            }
        }
    }
}