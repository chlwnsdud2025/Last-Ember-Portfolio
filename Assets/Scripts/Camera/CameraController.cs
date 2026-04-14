using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;


    // Update is called once per frame
    void Update()
    {
        transform.Translate(player.position);
    }
}
