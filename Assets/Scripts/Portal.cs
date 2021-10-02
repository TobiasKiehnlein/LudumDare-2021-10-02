using System;
using UnityEngine;

public class Portal : MonoBehaviour
{
    // Start is called before the first frame update
    public Portal reciever;
    private bool _dormant;
    private float _sleeper;

    private void Start()
    {
        
        var str = " forward ";
        str += this.gameObject.transform.forward.ToString();
        str += " up ";   str += this.gameObject.transform.up.ToString();
        str += " right ";   str += this.gameObject.transform.right.ToString();
      
        
        Debug.Log(str);
    }

    private void Update()
    {
        
     //   Debug.Log(this.gameObject.transform.forward);
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
        var euler1 = reciever.gameObject.transform.rotation.z;
        var euler2 = this.gameObject.transform.rotation.z;


        var theta = euler1 - euler2;

        var velocity = player.GetComponent<Rigidbody2D>().velocity;

        var quaternion = Quaternion.Euler(0, 0, theta);

        velocity = quaternion * velocity;
        player.GetComponent<Rigidbody2D>().velocity = -velocity;
        var transform1 = reciever.transform;
        player.transform.position = transform1.position + .5f * transform1.up;






    }
/*
    private Vector2 rotateVector2(Vector2 vec, float angle)
    {
       
        var x1 = vec.x;
        var y1 = vec.y;

        var x2 = x1 * Mathf.Cos(angle) - y1 * Mathf.Sin(angle);
        var y2 = x1 * Mathf.Sin(angle) + y1 * Mathf.Cos(angle);

        return new Vector2(x2, y2);
    }
    */
}