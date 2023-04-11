using UnityEditor.SearchService;
using UnityEngine;

public class GravityBehaviour : AbstractGameRunner
{
    /// <summary>
    /// Date: 08/04/23, By: Yvar
    /// The idea is to create a zone where the player has a static gravity direction or a dynamic one.
    /// For example, if the zone is setup to have a static gravity direction, the players downward gravity
    /// is always towards this direction. If it is a dynamic zone, then it will be more of a circular zone.
    /// The pivot of the zone can be a pushing or pulling gravity. The strenght of the gravity can also be set.
    /// While I only say player here, it should be more of a interface that should be targeted.
    /// </summary>

    [SerializeField] GravityType gravityType = GravityType.STATIC_ZONE;

    [Space(10), Header("gravityDirection will be normalized"), SerializeField] Vector3 gravityDirection = Vector3.down;
    [SerializeField] float gravityStrenght = 9.81f;

    [Space(10), Header("zone radius only applied when gravity point"), SerializeField] float zoneRadius = 10;

    public override void RunnerAwake()
    {

    }

    public override void RunnerStart()
    {
        if (gravityType != GravityType.STATIC_ZONE)
        {
            GetComponent<SphereCollider>().radius = zoneRadius;
        }
    }

    public override void RunnerUpdate()
    {

    }

    public override void RunnerFixedUpdate()
    {

    }

    public override void RunnerLateUpdate()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        IGravity obj = other.GetComponent<IGravity>();
        if (obj != null)
        {
            obj.OnEnterZone();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        IGravity obj = other.GetComponent<IGravity>();
        if (obj != null)
        {
            if (gravityType == GravityType.STATIC_ZONE)
            {
                obj.SetGravity(gravityDirection.normalized * gravityStrenght);
            }
            if (gravityType == GravityType.GRAVITY_POINT_PULL)
            {
                obj.SetGravity((transform.position - obj.GetPosition()).normalized * gravityStrenght);
            }
            if (gravityType == GravityType.GRAVITY_POINT_PUSH)
            {
                obj.SetGravity((obj.GetPosition() - transform.position).normalized * gravityStrenght);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IGravity obj = other.GetComponent<IGravity>();
        if (obj != null)
        {
            obj.OnExitZone();
        }
    }
}

public enum GravityType { STATIC_ZONE = 0, GRAVITY_POINT_PULL = 1, GRAVITY_POINT_PUSH = 2 }

public interface IGravity
{
    void OnEnterZone();
    void SetGravity(Vector3 direction);
    void OnExitZone();
    Vector3 GetPosition();
}