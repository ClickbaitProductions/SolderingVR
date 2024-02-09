using UnityEngine;

public abstract class AshTrayBaseState
{

    protected AshTrayStateManager stateManager;

    public AshTrayBaseState(AshTrayStateManager ashtrayStateManager)
    {
        this.stateManager = ashtrayStateManager;
    }

    public virtual void EnterState() { }

    public virtual void UpdateState() { }

    public virtual void ExitState() { }

    public virtual void OnCollisionEnter(Collision col) { }

    public virtual void OnCollisionStay(Collision col) { }

    public virtual void OnCollisionExit(Collision col) { }

}
