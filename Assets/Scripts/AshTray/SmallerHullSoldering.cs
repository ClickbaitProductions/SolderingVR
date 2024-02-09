using UnityEngine;

public class SmallerHullSoldering : MonoBehaviour
{

    AshTraySolderingState solderingState;
    Collider col;

    public void SetUpScript(AshTraySolderingState _solderingState)
    {
        solderingState = _solderingState;
        col = GetComponent<Collider>();
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("SolderingGun"))
        {
            solderingState.TryUpdateLineRenderer(col);
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("SolderingGun"))
        {
            solderingState.StopSoldering();
        }
    }

}
