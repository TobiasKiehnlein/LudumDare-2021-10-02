using UnityEngine;

public class Portal : MonoBehaviour
{
    // Start is called before the first frame update
    public Portal reciever;
    private bool _dormant;
    private float _sleeper;

    private void Update()
    {
        _sleeper -= Time.deltaTime;
        if (!(_sleeper < 0)) return;
        _sleeper = 0;
        _dormant = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_dormant) return;
        if (other.CompareTag("Player")) TeleportPlayer(other.gameObject);
    }

    private void Sleep()
    {
        _dormant = true;
        _sleeper = 1.5f;
    }

    private void TeleportPlayer(GameObject player)
    {
        var euler1 = gameObject.transform.eulerAngles;
        var euler2 = reciever.gameObject.transform.eulerAngles;

        var velocity = player.GetComponent<Rigidbody2D>().velocity;
        player.transform.position = reciever.gameObject.transform.position + new Vector3(
            rotateVector2(new Vector2(0, 1), -euler1.z - euler2.z).x,
            rotateVector2(new Vector2(0, 1), -euler1.z - euler2.z).y, 0);

        velocity = rotateVector2(velocity, -euler1.z - euler2.z);

        player.GetComponent<Rigidbody2D>().velocity = -velocity;
        reciever.Sleep();
    }

    private Vector2 rotateVector2(Vector2 vec, float angle)
    {
        var x1 = vec.x;
        var y1 = vec.y;

        var x2 = x1 * Mathf.Cos(angle) - y1 * Mathf.Sin(angle);
        var y2 = x2 * Mathf.Sin(angle) + y1 * Mathf.Cos(angle);

        return new Vector2(x2, y2);
    }
}