using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SolderingGun : MonoBehaviour
{

    XRGrabInteractable grabInteractable;

    public Observer<bool> IsGrabbing = new Observer<bool>(false);
    public Observer<bool> IsActivated = new Observer<bool>(false);

    [HideInInspector] public Collider trayColBeingDrawnOn;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.activated.AddListener((ActivateEventArgs eventArgs) => OnActivate());
        grabInteractable.deactivated.AddListener((DeactivateEventArgs eventArgs) => OnDeactivate());
        grabInteractable.selectEntered.AddListener((SelectEnterEventArgs eventArgs) => OnGrab());
        grabInteractable.selectExited.AddListener((SelectExitEventArgs eventArgs) => OnDrop());

        trayColBeingDrawnOn = null;
    }

    void OnGrab()
    {
        IsGrabbing.Value = true;
    }

    void OnDrop()
    {
        IsGrabbing.Value = false;
    }

    void OnActivate()
    {
        IsActivated.Value = true;
    }

    void OnDeactivate()
    {
        IsActivated.Value = false;
    }

}
