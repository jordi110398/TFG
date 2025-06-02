using UnityEngine;

public class ArrowFollowDirection : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool follow = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void EnableFollow()
    {
        follow = true;
    }

    void Update()
    {
        if (follow && rb.linearVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
