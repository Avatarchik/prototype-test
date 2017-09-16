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
        public Spyware_HandInputs Input;
        public Spyware_Hand otherHand;

        private HandState currentHandState;
        private HandMode currentHandMode;
        private Spyware_Interactable currentInteractable;
        private SteamVR_TrackedObject trackedObj;
        private SteamVR_Controller.Device Controller
        {
            get { return SteamVR_Controller.Input((int)trackedObj.index); }
        }

        Spyware_Interactable closestInteractable;

        public Spyware_Interactable ClosestInteractable
        {
            get
            {
                return closestInteractable;
            }
            set
            {
                closestInteractable = value;
                if (closestInteractable == null)
                    return;
            }
        }

        public Spyware_Interactable CurrentInteractable
        {
            get
            {
                return currentInteractable;
            }
            set
            {
                currentInteractable = value;
                if (value != null)
                    ClosestInteractable = null;
                currentInteractable = value;
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
            Input.GripUp = Controller.GetPressUp(4UL);
            Input.GripDown = Controller.GetPressDown(4UL);
            Input.GripPress = Controller.GetPress(4UL);
            Input.TriggerUp = Controller.GetPressUp(8589934592UL);
            Input.TriggerDown = Controller.GetPressDown(8589934592UL);
            Input.TriggerPress = Controller.GetPress(8589934592UL);
            Input.TriggerFloat = Controller.GetAxis(EVRButtonId.k_EButton_Axis1).x;
            Input.MenuUp = Controller.GetPressUp(2UL);
            Input.MenuDown = Controller.GetPressDown(2UL);
            Input.MenuPress = Controller.GetPress(2UL);
            Input.TouchPadUp = Controller.GetPressUp(4294967296UL);
            Input.TouchPadDown = Controller.GetPressDown(4294967296UL);
            Input.TouchPadPress = Controller.GetPress(4294967296UL);
            Input.TouchPadTouchUp = Controller.GetTouchUp(4294967296UL);
            Input.TouchPadTouchDown = Controller.GetTouchDown(4294967296UL);
            Input.TouchPadTouched = Controller.GetTouch(4294967296UL);
            Input.TouchPadAxes = Controller.GetAxis(EVRButtonId.k_EButton_Axis0);
        }

        public void Update()
        {
            if (Controller == null)
                return;

            //If Controller handleing handgun is 0.15f away from other hand dampen the recoil
            //var distance = Vector3.Distance(this.transform.position, otherHand.transform.position);

            if (currentHandMode == HandMode.Idle)
            {
                switch (currentHandState)
                {
                    case HandState.Empty:
                        if (Input.TriggerDown)
                        {
                            if (closestInteractable != null)
                            {
                                if (closestInteractable.IsSimpleInteract)
                                {
                                    closestInteractable.SimpleInteraction(this);
                                }
                                else
                                {
                                    CurrentInteractable = closestInteractable;
                                    currentHandState = HandState.Interacting;
                                    CurrentInteractable.BeginInteraction(this);
                                }

                                Controller.TriggerHapticPulse(300);
                            }
                        }
                        break;
                    case HandState.Interacting:
                        bool flag1 = false;
                        if (CurrentInteractable != null)
                        {
                            if (CurrentInteractable.interactionStyle == InteractionStyle.Hold)
                            {
                                if (Input.TriggerUp)
                                    flag1 = true;
                            }
                            else if (CurrentInteractable.interactionStyle == InteractionStyle.Toggle)
                            {
                                if (Input.GripPress)
                                {
                                    flag1 = true;
                                }
                            }
                            if (flag1)
                            {
                                CurrentInteractable.EndInteraction(this);
                                CurrentInteractable = null;
                                currentHandState = HandState.Empty;
                                break;
                            }
                            CurrentInteractable.UpdateInteraction(this);
                            break;
                        }
                        currentHandState = HandState.Empty;
                        break;
                }
            }

            UpdateInputs();

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

        public enum HandMode
        {
            Idle,
        }
    }
}