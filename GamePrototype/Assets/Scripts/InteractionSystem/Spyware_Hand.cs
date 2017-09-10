using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace Spyware
{
    public class Spyware_Hand : MonoBehaviour
    {
        public Rigidbody m_rb;
        public Transform cameraRig;
        public GameObject interactionSphere;
        public Transform interactionSphereCenter;
        public HandState currentHandState = HandState.Empty;
        public Spyware_HandInputs Input;
        public Spyware_Hand otherHand;

        private float timeSinceGripPress;
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

        public void Awake()
        {
            trackedObj = GetComponent<SteamVR_TrackedObject>();
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
            currentHandState = HandState.Empty;
        }

        private void UpdateInputs()
        {
            if (Controller == null)
                return;
            Input.GripUp = Controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip);
            Input.GripDown = Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip);
            Input.GripPress = Controller.GetPress(SteamVR_Controller.ButtonMask.Grip);
            Input.TriggerUp = Controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger);
            Input.TriggerDown = Controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
            Input.TriggerPress = Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger);
            Input.TriggerFloat = Controller.GetAxis(EVRButtonId.k_EButton_Axis1).x;
            Input.MenuUp = Controller.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu);
            Input.MenuDown = Controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu);
            Input.MenuPress = Controller.GetPress(SteamVR_Controller.ButtonMask.ApplicationMenu);
            Input.TouchPadUp = Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);
            Input.TouchPadDown = Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);
            Input.TouchPadPress = Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad);
            Input.TouchPadTouchUp = Controller.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad);
            Input.TouchPadTouchDown = Controller.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad);
            Input.TouchPadTouched = Controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad);
            Input.ApplicationMenuPress = Controller.GetPress(SteamVR_Controller.ButtonMask.ApplicationMenu);
            Input.TouchPadAxes = Controller.GetAxis(EVRButtonId.k_EButton_Axis0);
        }

        public void Update()
        {
            if (Controller == null)
                return;

            //If Controller handleing handgun is 0.15f away from other hand dampen the recoil
            //var distance = Vector3.Distance(this.transform.position, otherHand.transform.position);

            if (ClosestInteractable != null || CurrentInteractable != null)
            {

                if (currentHandState == HandState.Empty)
                {
                    if (Input.TriggerDown && ClosestInteractable.interactionStyle == InteractionStyle.Hold)
                    {
                        CurrentInteractable = ClosestInteractable;
                        currentHandState = HandState.Interacting;
                        CurrentInteractable.BeginInteraction(this);
                    }

                    else if (Input.TriggerDown && ClosestInteractable.interactionStyle == InteractionStyle.Toggle)
                    {
                        CurrentInteractable = ClosestInteractable;
                        currentHandState = HandState.Interacting;
                        CurrentInteractable.BeginInteraction(this);
                    }

                    Controller.TriggerHapticPulse(150);

                }

                else if (currentHandState == HandState.Interacting)
                {
                    if (Input.TriggerUp && CurrentInteractable.interactionStyle == InteractionStyle.Hold)
                    {
                        CurrentInteractable.EndInteraction(this);
                        currentHandState = HandState.Empty;
                        CurrentInteractable = null;
                    }

                    else if (Input.GripDown && CurrentInteractable.interactionStyle == InteractionStyle.Toggle)
                    {
                        CurrentInteractable.EndInteraction(this);
                        currentHandState = HandState.Empty;
                        CurrentInteractable = null;
                    }
                }

                UpdateInputs();

                if (CurrentInteractable == null)
                {
                    currentHandState = HandState.Empty;
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

        public enum HandState
        {
            Empty,
            Interacting,
        }
    }
}