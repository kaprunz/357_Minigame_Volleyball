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
        PositionBall("");
        PositionPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PositionBall(string whoServe)
    {
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.useGravity = false;
        switch (whoServe)
        {
            case "Player":
                Ball.transform.position = new Vector3(2.25f, 2.5f, -4f);
                break;
            default:
                Ball.transform.position = new Vector3(0, 2.5f, 0);
                break;
        }
    }

    public void PositionPlayer()
    {
        Player.transform.position = new Vector3(2.25f, 1, -5f);
        Player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        Player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}
