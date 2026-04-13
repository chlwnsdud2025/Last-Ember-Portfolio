using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        


    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(player.position);
    }
}
