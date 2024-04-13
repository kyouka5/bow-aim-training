using UnityEngine;

public class Arrow : MonoBehaviour
{
    public BowAgent agent;
    private new Rigidbody rigidbody;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rigidbody.velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rigidbody.velocity);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Center"))
        {
            agent.AddReward(1.0f);
            agent.EndEpisode();
        }
        else if (other.gameObject.CompareTag("Board"))
        {
            float reward = CalculateRewardByDistanceToCenter(other);
            agent.AddReward(reward);
        }
        else
        {
            agent.AddReward(-0.1f);
        }

        agent.ResetArrow();
    }

    private float CalculateRewardByDistanceToCenter(Collision other)
    {
        Vector3 collisionPoint = other.GetContact(0).point;
        float distanceToCenter = Vector3.Distance(collisionPoint, agent.targetCenter.transform.position);
        float reward = Mathf.Max(0, 1 - distanceToCenter);

        return reward;
    }
}
