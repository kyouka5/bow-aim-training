using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BowAgent : Agent
{
    public Transform target;
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public GameObject targetCenter;
    public float acceptableAimingAngle = 5.0f;

    private Rigidbody arrowRigidbody;
    private GameObject arrowInstance;
    private Quaternion initialRotation;
    private Quaternion arrowRotation = Quaternion.identity;
    private bool isButtonDown = false;
    private bool isArrowActive = false;

    private const int FORCE = 15;
    private enum ACTIONS
    {
        SHOOT = 0,
        REST = 1,
    }

    void Start()
    {
        initialRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        ResetBow();
        ResetTarget();
        ResetArrow();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float horizontalAim = actionBuffers.ContinuousActions[0];
        float verticalAim = actionBuffers.ContinuousActions[1];
        bool shoot = actionBuffers.DiscreteActions[0] == (int)ACTIONS.SHOOT;

        AdjustAim(horizontalAim, verticalAim);

        if (IsAimingAtTarget())
        {
            AddReward(0.01f);
        } else
        {
            AddReward(-0.02f);
        }

        if (shoot && !isArrowActive && IsAimingAtTarget())
        {
            ShootArrow();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;

        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");

        if (Input.GetKeyDown((int) MouseButton.Left))
        {
            if (!isButtonDown)
            {
                isButtonDown = true;
            }
        }
        else if (isButtonDown && Input.GetMouseButtonUp((int) MouseButton.Left))
        {
            isButtonDown = false;
            discreteActionsOut[0] = (int) ACTIONS.SHOOT;
        }
        else
        {
            discreteActionsOut[0] = (int) ACTIONS.REST;
        }
    }

    public void ResetArrow()
    {
        if (arrowRigidbody != null)
        {
            Destroy(arrowRigidbody.gameObject);
        }

        arrowInstance = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowRotation, gameObject.transform);
        arrowRigidbody = arrowInstance.GetComponent<Rigidbody>();
        arrowInstance.GetComponent<Arrow>().agent = this;
    }

    public void ResetBow()
    {
        transform.rotation = Quaternion.identity;
    }

    private void ResetTarget()
    {
        int xRange = Random.Range(3, 10);
        int zRange = Random.Range(-10, -2);
        target.localPosition = new Vector3(xRange, target.localPosition.y, zRange);
        targetCenter = target.transform.Find("Center").gameObject;
    }

    private bool IsAimingAtTarget()
    {
        Vector3 targetDirection = (targetCenter.transform.position - transform.position).normalized;
        Vector3 agentForward = transform.forward;

        float angleToTarget = Vector3.Angle(agentForward, targetDirection);

        return angleToTarget <= acceptableAimingAngle;
    }

    private void AdjustAim(float horizontal, float vertical)
    {
        if (isArrowActive)
        {
            return;
        }

        float normalizedVertical = (vertical + 1f) / 2f;
        float normalizedHorizontal = (horizontal + 1f) / 2f;

        float lerpedVertical = Mathf.Lerp(-45f, 45f, normalizedVertical);
        float lerpedHorizontal = Mathf.Lerp(-20f, 10f, normalizedHorizontal);

        Quaternion targetRotation = initialRotation * Quaternion.Euler(lerpedHorizontal, lerpedVertical, 0);

        transform.rotation = targetRotation;
    }

    private void ShootArrow()
    {
        arrowRotation = arrowSpawnPoint.rotation;
        arrowRigidbody.velocity = arrowSpawnPoint.transform.forward * FORCE;
        isArrowActive = true;
        StartCoroutine(WaitUntilArrowIsDestroyed(arrowInstance));
    }

    private IEnumerator WaitUntilArrowIsDestroyed(GameObject arrow)
    {
        yield return new WaitUntil(() => arrow == null);
        isArrowActive = false;
    }
}
