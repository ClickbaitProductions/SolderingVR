using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CreasingPen : MonoBehaviour
{

    public Observer<bool> IsActive = new Observer<bool>(false);
    public Observer<bool> IsGrabbing = new Observer<bool>(false);

    XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.activated.AddListener((ActivateEventArgs eventArgs) => OnActivate());
        grabInteractable.deactivated.AddListener((DeactivateEventArgs eventArgs) => OnDeactivate());
        grabInteractable.selectEntered.AddListener((SelectEnterEventArgs eventArgs) => OnGrab());
        grabInteractable.selectExited.AddListener((SelectExitEventArgs eventArgs) => OnDrop());
    }

    void OnActivate()
    {
        IsActive.Value = true;
    }

    void OnDeactivate()
    {
        IsActive.Value = false;
    }

    void OnGrab()
    {
        IsGrabbing.Value = true;
    }

    void OnDrop()
    {
        IsGrabbing.Value = false;
    }

}