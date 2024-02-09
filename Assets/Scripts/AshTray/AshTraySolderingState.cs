using UnityEngine;

public class AshTraySolderingState : AshTrayBaseState
{

    public Observer<bool> IsSoldering = new Observer<bool>(false);

    GameObject lineRendererPrefab;
    SolderingGun solderingGun;

    bool isActivated = false;

    LineRenderer currentLineRenderer;
    Vector3 lastPos;

    public AshTraySolderingState(AshTrayStateManager stateManager, GameObject _lineRendererPrefab, SolderingGun _solderingGun, 
        HapticInteractable hapticInteractable) : base(stateManager)
    {
        lineRendererPrefab = _lineRendererPrefab;
        solderingGun = _solderingGun;
        solderingGun.IsActivated.AddListener(OnActivateToggle);
        IsSoldering.AddListener(hapticInteractable.OnUseToggle);
    }

    void OnActivateToggle(bool _isActivated)
    {
        isActivated = _isActivated;

        if (!isActivated)
        {
            StopSoldering();
        }
    }

    public override void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag("SolderingGun"))
        {
            TryUpdateLineRenderer(stateManager.boxCollider);
        }
    }

    public void TryUpdateLineRenderer(Collider trayCol)
    {
        if (isActivated && (solderingGun.trayColBeingDrawnOn == null || trayCol.Equals(solderingGun.trayColBeingDrawnOn)))
        {
            if (solderingGun.trayColBeingDrawnOn == null)
                solderingGun.trayColBeingDrawnOn = trayCol;

            Vector3 currentPos = trayCol.ClosestPoint(stateManager.solderingGunTip.position);

            if (!IsSoldering.Value)
            {
                GameObject currentLineGO = stateManager.InstantiateGameObject(lineRendererPrefab, currentPos, Quaternion.identity, stateManager.transform);
                IsSoldering.Value = true;
                lastPos = currentPos;

                currentLineRenderer = currentLineGO.GetComponent<LineRenderer>();

                currentLineRenderer.useWorldSpace = true;
                currentLineRenderer.SetPosition(0, currentPos);
            }

            if (Vector3.Distance(currentPos, lastPos) > stateManager.distanceBetweenPoints)
            {
                UpdateLineRenderer(currentPos);
            }
        }
    }

    void UpdateLineRenderer(Vector3 currentPos)
    {
        //Vector3 localPos = stateManager.transform.InverseTransformPoint(currentPos);

        currentLineRenderer.positionCount = currentLineRenderer.positionCount + 1;
        currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, currentPos);

        lastPos = currentPos;
    }

    public override void OnCollisionExit(Collision col)
    {
        StopSoldering();
    }

    public void StopSoldering()
    {
        IsSoldering.Value = false;

        lastPos = Vector3.zero;
        currentLineRenderer = null;
        IsSoldering.Value = false;

        solderingGun.trayColBeingDrawnOn = null;
    }

}
