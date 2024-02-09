using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHands : MonoBehaviour
{

    public Observer<bool> IsPinching = new Observer<bool>(false);
    public bool isFolding = false;

    [SerializeField] Animator handAnim;
    [SerializeField] InputActionProperty pinchAnimAction;
    [SerializeField] InputActionProperty grabAnimAction;

    float pinchThreshold = 0.7f;
    float pinchVal;

    void Update()
    {
        pinchVal = pinchAnimAction.action.ReadValue<float>();
        handAnim.SetFloat("Trigger", pinchVal);

        float grabVal = grabAnimAction.action.ReadValue<float>();
        handAnim.SetFloat("Grip", grabVal);

        if (IsPinching.Value)
        {
            if (pinchVal < pinchThreshold)
                IsPinching.Value = false;
        } else
        {
            if (pinchVal >= pinchThreshold)
                IsPinching.Value = true;
        }
    }

}
