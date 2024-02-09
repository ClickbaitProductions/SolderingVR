using System.Collections.Generic;
using UnityEngine;

public class StatesScripts
{
    public AshTrayCreasingState creasingState;
    public AshTraySolderingState solderingState;

    public StatesScripts(AshTrayStateManager stateManager, Transform creasingPenTip, CreasingPen creasingPen,
        HapticInteractable creasingPenHapticInteractable, GameObject lineRendererPrefab, SolderingGun solderingGun,
        HapticInteractable solderingGunHaptic)
    {
        creasingState = new AshTrayCreasingState(stateManager, creasingPenTip, creasingPen, creasingPenHapticInteractable);
        solderingState = new AshTraySolderingState(stateManager, lineRendererPrefab, solderingGun, solderingGunHaptic);
    }
}

public class AshTrayStateManager : MonoBehaviour
{

    public enum State
    {
        Creasing,
        Folding,
        Soldering,
    }

    Dictionary<State, AshTrayBaseState> stateMapping;

    public StatesScripts allStates { get; private set; }
    public Observer<State> CurrentState = new Observer<State>(State.Folding);
    AshTrayBaseState currentScript;

    [Header("General")]
    [Tooltip("The GameObject to be cut")]
    public GameObject trayVisualGO;
    public Transform visualsParent;
    public BoxCollider boxCollider {  get; private set; }

    
    [Header("Creasing State")]
    public Transform cuttingPlaneTransform;
    [SerializeField] Transform creasingPenTip;
    [SerializeField] CreasingPen creasingPen;
    [SerializeField] HapticInteractable creasingPenHaptic;

    [Header("Soldering State")]
    public Transform solderingGunTip;
    [SerializeField] SolderingGun solderingGun;
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] HapticInteractable solderingGunHaptic;
    public float distanceBetweenPoints = 0.05f;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();

        allStates = new StatesScripts(this, creasingPenTip, creasingPen, creasingPenHaptic, lineRendererPrefab, solderingGun, solderingGunHaptic);

        stateMapping = new Dictionary<State, AshTrayBaseState>()
        {
            {State.Creasing, allStates.creasingState},
            {State.Soldering, allStates.solderingState}
        };

        UpdateCuttingPlaneTransform(cuttingPlaneTransform.position, Quaternion.Euler(90, 0, 0));
        ChangeState(State.Folding);
    }

    AshTrayBaseState GetStateScript(State state)
    {
        if (stateMapping.ContainsKey(state))
        {
            return stateMapping[state];
        } else
        {
            return null;
        }
    }

    void Start()
    {
        creasingPen.IsGrabbing.AddListener(CreasingPen_OnGrabToggle);
        solderingGun.IsGrabbing.AddListener(SolderingGun_OnGrabToggle);
    }

    void CreasingPen_OnGrabToggle(bool isGrabbing)
    {
        if (isGrabbing)
        {
            ChangeState(State.Creasing);
        } else
        {
            ChangeState(State.Folding);
        }
    }

    void SolderingGun_OnGrabToggle(bool isGrabbing)
    {
        if (isGrabbing)
        {
            ChangeState(State.Soldering);
        } else
        {
            ChangeState(State.Folding);
        }
    }

    void Update()
    {
        if (currentScript != null)
            currentScript.UpdateState();
    }

    public void ChangeState(State state)
    {
        if (currentScript != null)
            currentScript.ExitState();

        CurrentState.Value = state;

        currentScript = GetStateScript(CurrentState.Value);
        if (currentScript != null)
            currentScript.EnterState();
    }

    void OnCollisionEnter(Collision col)
    {
        if (currentScript != null)
            currentScript.OnCollisionEnter(col);
    }

    void OnCollisionStay(Collision col)
    {
        if (currentScript != null)
            currentScript.OnCollisionStay(col);
    }

    void OnCollisionExit(Collision col)
    {
        if (currentScript != null)
            currentScript.OnCollisionExit(col);
    }

    public void UpdateCuttingPlaneTransform(Vector3 pos, Quaternion rot)
    {
        cuttingPlaneTransform.position = pos;
        cuttingPlaneTransform.rotation = rot;
    }

    public void DestroyGameObject(GameObject gameObject)
    {
        Destroy(gameObject);
    }

    public void DestroyComponent(Component component)
    {
        Destroy(component);
    }

    public GameObject InstantiateGameObject(GameObject prefab = null, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), Transform parent = null)
    {
        if (prefab == null)
        {
            // Spawns an empty gameObject
            GameObject emptyGO = new GameObject("SmallerHull");
            emptyGO.transform.position = pos;
            emptyGO.transform.rotation = rot;
            if (parent != null)
                emptyGO.transform.parent = parent;

            return emptyGO;
        } else
        {
            if (parent == null)
                return Instantiate(prefab, pos, rot);
            else
                return Instantiate(prefab, pos, rot, parent); 
        }
    }


    public Vector3 GetHorizontalLocalAxis()
    {
        return transform.right;
    }

    public Vector3 GetVerticalLocalAxis()
    {
        return transform.forward;
    }

}
