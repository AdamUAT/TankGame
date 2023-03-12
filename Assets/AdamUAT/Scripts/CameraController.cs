using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject cameraPrefab;
    private GameObject camera;
    [SerializeField]
    private GameObject cameraPosition;
    [SerializeField]
    private float cameraLerpSpeed = 0.3f;

    private void Start()
    {
        camera = Instantiate(cameraPrefab, cameraPosition.transform.position, cameraPosition.transform.rotation); //Innitial spawn of the camera.
    }

    //This update is only for cameras, as it helps stop making things jittery.
    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    /// <summary>
    /// Moves the camera smoothly to the cameraPosition GameObject.
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (camera != null)
        {
            camera.transform.position = Vector3.Lerp(camera.transform.position, cameraPosition.transform.position, cameraLerpSpeed); //Smooth sets the camera to be behind the player by moving it 30% closer to the player each tick, so 1st tick its 30%, 2nd is 48, 3rd is 63.6%, 4th is 74.25%, etc.
            camera.transform.eulerAngles = new Vector3(45, cameraPosition.transform.eulerAngles.y, cameraPosition.transform.eulerAngles.z); //Sets the camera so it matches the global rotation of the player, with it looking down a little.
        }
    }
}
