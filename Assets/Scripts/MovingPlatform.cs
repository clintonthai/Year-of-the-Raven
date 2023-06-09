using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MovingPlatform : PhysicsObject
{
    [Header("Moving Platform Parameters")]
    public Vector3 destinationOffset;
    private Vector3 startPosition;
    private Vector3 endPosition;

    public float moveTime;
    public float moveDelay;
    public bool shouldReturnImmediately;

    public bool isAutomatic = true;
    private bool isTargettingDestination = false; // true while the platform is moving to or sitting at the end position
    private Coroutine moveCoroutine;

    private RideRegion[] rideRegions;
    public MeshRenderer meshRenderer;

    protected override void Awake() {
        base.Awake();

        startPosition = transform.position;
        endPosition = startPosition + destinationOffset;

        if (isAutomatic) {
            StartCoroutine(DoMovement());
        }

        rideRegions = GetComponentsInChildren<RideRegion>();
    }

    protected override void FixedUpdate()
    {
        // don't do what physicsobject normally does here
    }

    public override void EnterTornado(Tornado tornado)
    {
        // do nothing
    }
    public override void ExitTornado()
    {
        // do nothing
    }


    public void Move() {
        Move(!isTargettingDestination);
    }

    public void Move(bool shouldTargetDestination) {
        if (isAutomatic) {
            Debug.LogError("Cannot manually move an automatic moving platform");
            return;
        }

        if (shouldTargetDestination == isTargettingDestination) {
            // we're already moving there
            return;
        }

        // cancel the previous movement, if any
        if (moveCoroutine != null) {
            StopCoroutine(moveCoroutine);
        }

        isTargettingDestination = shouldTargetDestination;
        float theMoveTime = moveTime;
        if (!isTargettingDestination && shouldReturnImmediately) {
            theMoveTime = 0.5f;
        }
        moveCoroutine = StartCoroutine(DoMove(transform.position, isTargettingDestination ? endPosition : startPosition, theMoveTime));
    }


    IEnumerator DoMovement() {
        while (true) {
            yield return new WaitForSeconds(moveDelay);
            isTargettingDestination = true;
            yield return StartCoroutine(DoMove(startPosition, endPosition, moveTime));
            
            yield return new WaitForSeconds(moveDelay);
            isTargettingDestination = false;
            if (shouldReturnImmediately) {
                yield return StartCoroutine(DoMove(endPosition, startPosition, 0.5f));   
            }
            else {
                yield return StartCoroutine(DoMove(endPosition, startPosition, moveTime));
            }
            
        }
    }

    IEnumerator DoMove(Vector3 start, Vector3 end, float time) {
        float t = 0;
        while (t < time) {
            // wait first to sync all movement to the fixed update cycle
            yield return new WaitForFixedUpdate();
             t += Time.fixedDeltaTime;

            // set velocity such that the platform will move to where it wants to be
            Vector3 desiredPos = Vector3.Lerp(start, end, t / time);
            SetRelativeVelocity((desiredPos - transform.position) / Time.fixedDeltaTime);
            transform.position = desiredPos;
        }

        // snap to the end point on the next frame
        yield return new WaitForFixedUpdate();
        SetRelativeVelocity((end - transform.position) / Time.fixedDeltaTime);
    }


    /*
    public override Vector3 GetRidePoint(PhysicsObject rider) {
        if (rideRegions.Length == 0) {
            // a moving platform should have at least one ride region, but in case it doesn't, give the position of the moving platform
            return base.GetRidePoint(rider);
        }

        // select the ride region that is closest to the object
        RideRegion theRide = rideRegions[0];
        foreach (RideRegion ride in rideRegions) {
            if ((ride.transform.position - rider.transform.position).sqrMagnitude <
                (theRide.transform.position - rider.transform.position).sqrMagnitude)
            {
                theRide = ride;
            }
        }
        return theRide.transform.position;
    }
    */


    private void OnDrawGizmos() {
        if (collider == null) {
            collider = GetComponent<Collider>();
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + destinationOffset);
        Gizmos.DrawWireCube(collider.bounds.center + destinationOffset, collider.bounds.size);
    }


    public void SetDestination(TileEditorContext context) {
        destinationOffset = context.adjacentPosition - transform.position;
    }

    public void SetModel(TileEditorContext context) {
#if UNITY_EDITOR
        // find models
        var models = new List<GameObject>();

        string[] guids = AssetDatabase.FindAssets("t:prefab", new string[] {"Assets/Prefabs/Model Prefabs/MovingPlatforms"});
        if (guids == null || guids.Length == 0) {
            Debug.LogError("Couldn't find any moving platform model prefabs");
            return;
        }
        
        foreach (string guid in guids) {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            models.Add(asset);
        }

        // figure out which one we have and assign the next one
        string meshName = meshRenderer.gameObject.name;
        int modelIndex = models.FindIndex(model => model.name == meshName);
        if (modelIndex < 0) {
            Debug.LogError("Couldn't find moving platform model with name " + meshName);
            return;
        }

        modelIndex = (modelIndex + 1) % models.Count;
        GameObject newModel = (GameObject)PrefabUtility.InstantiatePrefab(models[modelIndex], meshRenderer.transform.parent);
        Undo.RegisterCreatedObjectUndo(newModel, newModel.name);

        try {
            Undo.DestroyObjectImmediate(meshRenderer.gameObject);
        } catch {
            Debug.LogWarning("Undo.DestroyObjectImmediate failed so we're deactivating this game object instead");
            meshRenderer.gameObject.SetActive(false);
        }
        meshRenderer = newModel.GetComponent<MeshRenderer>();
#endif
    }
}
