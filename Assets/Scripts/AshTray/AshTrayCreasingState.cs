using EzySlice;
using Unity.VisualScripting;
using UnityEngine;

public class AshTrayCreasingState : AshTrayBaseState
{

    /// <summary>
    /// Let's the player draw on the ash tray using the pen. Depending on the angle of the line drawn, the ash tray will be cut vertically
    /// or horizontally into seperate meshes, letting the FoldingEdge state fold the seperate mesh
    /// </summary>

    public Observer<bool> IsCutting = new Observer<bool>(false);

    Transform creasingPenTip;
    CreasingPen creasingPen;

    Vector3 startDrawingPos;
    Vector3 endDrawingPos;

    float threshold = 0.1f;   // CHANGE THIS
    float creaseAngle = 5; // Angle to rotate the smaller hull be when a crease is made to show the impression

    // Wait for this much time after OnCollisionExit to register OnCollisionEnter to prevent multiple accidental cuts
    const float MAX_WAIT_TIME = 0.5f;
    float waitTimer = 0;
    bool canCut = true;
    bool penIsActive = false;
    bool penJustTurnedActive = false;

    Material trayMaterial;

    // Call the parent constructor
    public AshTrayCreasingState(AshTrayStateManager stateManager, Transform _creasingPenTip, CreasingPen _creasingPen, 
        HapticInteractable hapticInteractable) : base(stateManager)
    { 
        creasingPenTip = _creasingPenTip;
        creasingPen = _creasingPen;

        creasingPen.IsActive.AddListener(CreasingPen_OnActiveChanged);
        IsCutting.AddListener(hapticInteractable.OnUseToggle);
        trayMaterial = stateManager.trayVisualGO.GetComponent<MeshRenderer>().material;
    }

    public override void ExitState()
    {
        IsCutting.Value = false;
    }

    void CreasingPen_OnActiveChanged(bool isActive)
    {
        penIsActive = isActive;

        if (penIsActive)
            penJustTurnedActive = true;
        else if (IsCutting.Value)
            TryCutTray();
    }

    public override void UpdateState()
    {
        if (!canCut)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                canCut = true;
            }
        }
    }

    public override void OnCollisionStay(Collision col)
    {
        if (!col.gameObject.CompareTag("CreasingPen")) return;

        if (penIsActive)
        {
            if (penJustTurnedActive)
            {
                startDrawingPos = creasingPenTip.position;
                penJustTurnedActive = false;
                IsCutting.Value = true;
            }

            endDrawingPos = creasingPenTip.position;
        }
    }

    public override void OnCollisionExit(Collision col)
    {
        if (!col.gameObject.CompareTag("CreasingPen")) return;

        TryCutTray();
    }

    void TryCutTray()
    {
        if (canCut)
        {
            //Vector3 projectedVector = GetProjectionToLocalAxis(out float magnitude, out bool isHorizontalCut);
            Vector3 projectedVectorNormalized = GetProjectionNormalizedToLocalAxis(out float magnitude, out bool isHorizontalCut);
            if (magnitude > threshold)
            {
                CutTray(projectedVectorNormalized, isHorizontalCut);

                if (penIsActive)
                {
                    // Only if the player is still pressing the trigger, take this precaution. If he presses the trigger twice in a 0.01s
                    // timespne thats fine
                    canCut = false;
                    waitTimer = MAX_WAIT_TIME;
                }
            }
        }

        IsCutting.Value = false;
    }

    void CutTray(Vector3 projectedVectorNormalized, bool isHorizontalCut)
    {
        Vector3 planePos = startDrawingPos;

        planePos.y = stateManager.trayVisualGO.transform.position.y;

        Vector3 targetPos = planePos + projectedVectorNormalized;
        Vector3 lookDir = targetPos - planePos;
        Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
        targetRot *= Quaternion.Euler(0, 90, 0);
        targetRot.eulerAngles = new Vector3(90, targetRot.eulerAngles.y, 0);

        stateManager.UpdateCuttingPlaneTransform(planePos, targetRot);

        SlicedHull hull = stateManager.trayVisualGO.Slice(stateManager.cuttingPlaneTransform.position, stateManager.cuttingPlaneTransform.up);
        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(stateManager.trayVisualGO, trayMaterial);
            GameObject lowerHull = hull.CreateLowerHull(stateManager.trayVisualGO, trayMaterial);
            upperHull.transform.eulerAngles = stateManager.trayVisualGO.transform.eulerAngles;
            lowerHull.transform.eulerAngles = stateManager.trayVisualGO.transform.eulerAngles;

            upperHull.AddComponent<BoxCollider>();
            lowerHull.AddComponent<BoxCollider>();

            // Cause the upper/lower hulls spawn at the same world position as the tray visual GO's local position
            Vector3 upperHullPos = upperHull.transform.position;
            Vector3 lowerHullPos = lowerHull.transform.position;

            upperHull.transform.parent = stateManager.visualsParent;
            upperHull.transform.localPosition = upperHullPos;

            lowerHull.transform.parent = stateManager.visualsParent;
            lowerHull.transform.localPosition = lowerHullPos;
            //-----------------------------------------------------------------------------------------------------

            stateManager.DestroyGameObject(stateManager.trayVisualGO);

            // Set the parent of the smaller hull at the junction where they split so that it can be rotated around it using its parent
            // And set the new statemanager.trayVisualGO as the bigger hull so that it can then be split again
            GameObject largerHull, smallerHull;
            GetLargerHull(upperHull, lowerHull, out largerHull, out smallerHull);

            largerHull.name = "LargerHull";
            smallerHull.name = "SmallerHull";
            smallerHull.GetComponent<BoxCollider>().providesContacts = true;
            smallerHull.transform.parent = stateManager.visualsParent;

            GameObject smallerHullParentGO = stateManager.InstantiateGameObject(pos: planePos, rot: stateManager.trayVisualGO.transform.rotation,
                parent: stateManager.visualsParent);
            smallerHullParentGO.name = "SmallerHullParent";
            SetSmallerHullParentTransform(smallerHullParentGO, smallerHull, isHorizontalCut, out bool isPositiveAxis);

            smallerHull.transform.parent = smallerHullParentGO.transform;
            smallerHull.transform.rotation = stateManager.visualsParent.rotation;
            SetUpSmallerHull(smallerHull, isHorizontalCut, isPositiveAxis);

            stateManager.trayVisualGO = largerHull;

            UpdateTrayCollider();

            RotateSmallerHullToShowCrease(smallerHullParentGO.transform, isHorizontalCut);
        }
    }

    void SetUpSmallerHull(GameObject smallerHullGO, bool isHorizontalCut, bool isPositiveAxis)
    {
        SmallerHull smallerHull = smallerHullGO.AddComponent<SmallerHull>();
        smallerHull.SetUpScript(isHorizontalCut, stateManager.transform, isPositiveAxis, stateManager);

        SmallerHullSoldering smallerHullSoldering = smallerHullGO.AddComponent<SmallerHullSoldering>();
        smallerHullSoldering.SetUpScript(stateManager.allStates.solderingState);

        Rigidbody rb = smallerHull.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void SetSmallerHullParentTransform(GameObject smallerHullParentGO, GameObject smallerHull, bool isHorizontal, out bool isPositiveAxis)
    {
        smallerHullParentGO.transform.localEulerAngles = Vector3.zero;
        smallerHullParentGO.transform.localScale = smallerHull.transform.localScale;

        float rotY;
        Vector3 planeLocalPos = stateManager.transform.InverseTransformPoint(stateManager.cuttingPlaneTransform.position);
        isPositiveAxis = false;
        if (isHorizontal)
        {
            if (planeLocalPos.z > 0)
            {
                rotY = 270;
                isPositiveAxis = true;
            } else
                rotY = 90;
        } else
        {
            if (planeLocalPos.x > 0)
            {
                rotY = 0;
                isPositiveAxis = true;
            } else
                rotY = 180;
        }

        smallerHullParentGO.transform.localEulerAngles = Vector3.up * rotY;
    }

    void RotateSmallerHullToShowCrease(Transform smallerHullParentTransform, bool isHorizontalCut)
    {
        Vector3 localRot = smallerHullParentTransform.localEulerAngles;
        localRot.z = creaseAngle;

        smallerHullParentTransform.localEulerAngles = localRot;
    }

    void UpdateTrayCollider()
    {
        // Move the collider from the tray visual GO back onto the tray
        BoxCollider trayCol = stateManager.boxCollider;
        BoxCollider trayVisualCol = stateManager.trayVisualGO.GetComponent<BoxCollider>();
        Vector3 trayVisualLocalScale = stateManager.trayVisualGO.transform.localScale;

        Vector3 newColSize = new Vector3();
        Vector3 newColCenter = new Vector3();

        newColSize.x = trayVisualCol.size.x * trayVisualLocalScale.x;
        newColSize.y = trayVisualCol.size.y * trayVisualLocalScale.y;
        newColSize.z = trayVisualCol.size.z * trayVisualLocalScale.z;

        newColCenter.x = trayVisualCol.center.x * trayVisualLocalScale.x;
        newColCenter.y = trayVisualCol.center.y * trayVisualLocalScale.y;
        newColCenter.z = trayVisualCol.center.z * trayVisualLocalScale.z;

        trayCol.size = newColSize;
        trayCol.center = newColCenter;

        stateManager.DestroyComponent(stateManager.trayVisualGO.GetComponent<BoxCollider>());
    }

    void GetLargerHull(GameObject upperHull, GameObject lowerHull, out GameObject largerHull, out GameObject smallerHull)
    {
        BoxCollider upperHullCol = upperHull.GetComponent<BoxCollider>();
        BoxCollider lowerHullCol = lowerHull.GetComponent<BoxCollider>();

        Collider collider = upperHullCol.GetComponent<Collider>();

        float upperHullVolume = upperHullCol.size.x * upperHullCol.size.y * upperHullCol.size.z;
        float lowerHullVolume = lowerHullCol.size.x * lowerHullCol.size.y * lowerHullCol.size.z;

        if (upperHullVolume > lowerHullVolume)
        {
            largerHull = upperHull;
            smallerHull = lowerHull;
        } else
        {
            largerHull = lowerHull;
            smallerHull = upperHull;
        }
    }

    Vector3 GetProjectionNormalizedToLocalAxis(out float magnitude, out bool isHorizontalCut)
    {
        Vector3 displacementVector = endDrawingPos - startDrawingPos;

        Vector3 horizontalVector = stateManager.GetHorizontalLocalAxis();
        Vector3 verticalVector = stateManager.GetVerticalLocalAxis();

        Vector3 horizontalProjection = Vector3.Project(displacementVector, horizontalVector);
        Vector3 verticalProjection = Vector3.Project(displacementVector, verticalVector);

        float horizontalMagnitude = Vector3.Magnitude(horizontalProjection);
        float verticalMagnitude = Vector3.Magnitude(verticalProjection);

        isHorizontalCut = Mathf.Abs(horizontalMagnitude) > Mathf.Abs(verticalMagnitude);

        Vector3 projection;
        if (isHorizontalCut)
        {
            projection = horizontalProjection;
        } else
        {
            projection = verticalProjection;
        }

        Vector3 projectionNormalized = projection.normalized;

        magnitude = Vector3.Dot(projection, projectionNormalized);

        return projectionNormalized;
    }

}
