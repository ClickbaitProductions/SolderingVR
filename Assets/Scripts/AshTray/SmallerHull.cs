using UnityEngine;

public class SmallerHull : MonoBehaviour
{

    Transform trayParent;
    Transform smallerHullparentTransform;

    AnimateHands handInBounds = null;
    Collider handCol = null;

    bool isHorizontal;
    bool isPositiveAxis;

    bool startedPinching = false;

    Collider col;

    // Rotate around Z axis
    // SmallerHullParent's red arrow should always face outside(rotate using Y axis)

    void Update()
    {
        if (startedPinching)
        {
            Vector3 targetDir = handInBounds.transform.position - transform.position;
            Vector3 localDir = trayParent.InverseTransformDirection(targetDir);
            Vector3 targetRot = smallerHullparentTransform.localEulerAngles;

            float angle, invertedAngle;

            if (isHorizontal)
            {
                angle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
                invertedAngle = 180 - angle;

                if (!isPositiveAxis)
                    angle = invertedAngle;
            } else
            {
                angle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
                invertedAngle = 180 - angle;

                if (!isPositiveAxis)
                    angle = invertedAngle;
            }

            if (!isPositiveAxis)
                angle = invertedAngle;

            targetRot.z = angle;

            smallerHullparentTransform.localEulerAngles = targetRot;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (startedPinching || (handInBounds != null && handInBounds.isFolding)) return;

        if (col.CompareTag("FoldingCollider"))
        {
            handInBounds = col.transform.parent.GetComponent<AnimateHands>();
            handCol = col;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (startedPinching || (handInBounds != null && handInBounds.isFolding)) return;

        if (col == handCol)
        {
            if (handInBounds.IsPinching.Value)
            {
                startedPinching = true;
                handInBounds.isFolding = true;
                handInBounds.IsPinching.AddListener(StoppedPinching);
            }
        }
    }

    void StoppedPinching(bool isPinching)
    {
        if (!isPinching)
        {
            startedPinching = false;
            handInBounds.isFolding = false;
            handInBounds.IsPinching.RemoveListener(StoppedPinching);
        }
    }

    public void SetUpScript(bool _isHorizontal, Transform _trayParent, bool _isPositiveAxis, AshTrayStateManager stateManager)
    {
        col = GetComponent<Collider>();

        isHorizontal = _isHorizontal;
        trayParent = _trayParent;
        smallerHullparentTransform = transform.parent;
        isPositiveAxis = _isPositiveAxis;
        stateManager.CurrentState.AddListener(OnStateChange);


        DisableComponent();
    }

    void OnStateChange(AshTrayStateManager.State state)
    {
        if (state == AshTrayStateManager.State.Folding)
            EnableComponent();
        else
            DisableComponent();
    }

    void EnableComponent()
    {
        this.enabled = true;
    }

    void DisableComponent()
    {
        this.enabled = false;
    }

}
