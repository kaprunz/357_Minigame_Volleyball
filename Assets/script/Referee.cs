using UnityEngine;

public class Referee : MonoBehaviour
{

    public GameObject Ball;
    public GameObject Player;
    public GameObject Enemy;
    public static Referee instance;

    [SerializeField]
    Rigidbody ballRb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        // Set up Singleton instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        BallSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BallSpawn()
    {
        Ball.transform.position = new Vector3(0, 2.5f, 0);
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.useGravity = false;
    }
}
