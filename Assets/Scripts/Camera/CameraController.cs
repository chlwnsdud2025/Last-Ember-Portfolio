using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    public Vector3 cameraPos = new Vector3(0, 10, -3);

    public Vector3 cameraRot = new Vector3(80, 0, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        


    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + cameraPos;
        transform.rotation = Quaternion.Euler(cameraRot);
    }
}
