using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spyware
{
    public class Spyware_Hand : MonoBehaviour
    {

        public Rigidbody m_rb;
        public Transform cameraRig;
        public GameObject interactionSphere;
        public InteractionStyle interactionStyle = InteractionStyle.Hold;

        public HandState currentHandState = HandState.Empty;
        private Spyware_Interactable currentInteractableItem;
        private SteamVR_TrackedObject trackedObj;
        private SteamVR_Controller.Device Controller
        {
            get { return SteamVR_Controller.Input((int)trackedObj.index); }
        }

        Spyware_Interactable closestInteractableItem;

        public Spyware_Interactable ClosestInteractable
        {
            get
            {
                return closestInteractableItem;
            }
            set
            {
                closestInteractableItem = value;
                if (closestInteractableItem == null)
                    return;
            }
        }

        public Spyware_Interactable CurrentInteractable
        {
            get
            {
                return currentInteractableItem;
            }
            set
            {
                currentInteractableItem = value;
                if (value != null)
                    ClosestInteractable = null;
                currentInteractableItem = value;
            }
        }

        public void ForceSetInteractable(Spyware_Interactable interactable)
        {
            if (CurrentInteractable != null)
                CurrentInteractable.EndInteraction(this);
            CurrentInteractable = interactable;
            if (interactable == null)
            {
                CurrentInteractable = null;
                ClosestInteractable = null;
                currentHandState = HandState.Empty;
            }
            else
                currentHandState = HandState.Interacting;
        }

        public void EndInteractionIfHeld(Spyware_Interactable interactable)
        {
            if (interactable != CurrentInteractable)
                return;
            CurrentInteractable = null;
            ClosestInteractable = null;
            //currentHandState = HandState.Empty;
        }

        public void Awake()
        {
            trackedObj = GetComponent<SteamVR_TrackedObject>();
        }

        public void FixedUpdate()
        {
            Debug.Log(currentHandState);
            if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                SceneManager.LoadScene("Interactiontesting");
            }
            if (closestInteractableItem != null)
            {
                if (currentHandState == HandState.Empty && interactionStyle == InteractionStyle.Hold)
                {
                    if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip) == true)
                    {
                        CurrentInteractable = ClosestInteractable;
                        currentHandState = HandState.Interacting;
                        CurrentInteractable.BeginInteraction(this);
                    }
                }

                if (currentHandState == HandState.Empty && interactionStyle == InteractionStyle.Toggle)
                {
                    if (Controller.GetPress(SteamVR_Controller.ButtonMask.Grip) == true)
                    {
                        CurrentInteractable = ClosestInteractable;
                        currentHandState = HandState.Interacting;
                        CurrentInteractable.BeginInteraction(this);
                    }
                }
                Controller.TriggerHapticPulse(200);
            }

            if (currentHandState == HandState.Interacting && interactionStyle == InteractionStyle.Hold)
            {
                if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip) == true)
                {
                    CurrentInteractable.EndInteraction(this);
                    currentHandState = HandState.Empty;
                    CurrentInteractable = null;
                }
            }

            if (currentHandState == HandState.Interacting && interactionStyle == InteractionStyle.Toggle)
            {
                if (Controller.GetPress(SteamVR_Controller.ButtonMask.Grip) == true)
                {
                    CurrentInteractable.EndInteraction(this);
                    currentHandState = HandState.Empty;
                    CurrentInteractable = null;
                }
            }
        }

        private void Collider(Collider collider, bool isEnter)
        {
            if (isEnter)
            {
                if (!(collider.gameObject.GetComponent<Spyware_Interactable>() != null))
                    return;
                collider.gameObject.GetComponent<Spyware_Interactable>().Test(this);
            }
            else
            {
                if (currentHandState != HandState.Empty || !(collider.gameObject.GetComponent<Spyware_Interactable>() != null))
                    return;
                Spyware_Interactable interactable = collider.gameObject.GetComponent<Spyware_Interactable>();
                if (interactable == null)
                    return;
                if (ClosestInteractable == null)
                {
                    ClosestInteractable = interactable;
                }
                else
                {
                    if (!(ClosestInteractable != interactable) || Vector3.Distance(collider.transform.position, transform.position) >= (double)Vector3.Distance(ClosestInteractable.transform.position, transform.position))
                        return;
                    ClosestInteractable = interactable;
                }
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            Collider(collider, true);
        }

        private void OnTriggerStay(Collider collider)
        {
            Collider(collider, false);
        }

        private void OnTriggerExit(Collider collider)
        {
            if (!(collider.gameObject.GetComponent<Spyware_Interactable>() != null) || !(ClosestInteractable == collider.GetComponent<Spyware_Interactable>()))
                return;
            ClosestInteractable = null;
        }

        public enum InteractionStyle
        {
            Hold,
            Toggle
        }

        public enum HandState
        {
            Empty,
            Interacting
        }
    }
}