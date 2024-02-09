using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HapticInteractable : MonoBehaviour
{

    [SerializeField] AshTrayStateManager stateManager;

    [Range(0, 1)]
    [SerializeField] float intensity;
    [SerializeField] float duration;

    bool vibrate = false;

    XRBaseControllerInteractor controllerInteractor = null;

    void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.activated.AddListener(OnActivate);
        grabInteractable.deactivated.AddListener((DeactivateEventArgs eventArgs) => OnDeactivate());
    }

    void OnActivate(ActivateEventArgs eventArgs)
    {
        if (eventArgs.interactorObject is XRBaseControllerInteractor _controllerInteractor)
        {
            controllerInteractor = _controllerInteractor;
        }
    }

    void OnDeactivate()
    {
        controllerInteractor = null;
    }

    public void OnUseToggle(bool isBeingUsed)
    {
        if (isBeingUsed)
        {
            if (controllerInteractor != null)
            {
                InvokeRepeating("TriggerRepeating", 0, duration);
            }
        } else
        {
            CancelInvoke("TriggerRepeating");
        }
    }

    void TriggerRepeating()
    {
        TriggerHaptic(controllerInteractor.xrController);
    }

    public void TriggerHaptic(XRBaseController controller)
    {
        if (intensity > 0)
        {
            controller.SendHapticImpulse(intensity, duration);
        }
    }

}
