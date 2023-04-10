using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    [HideInInspector]
    public GameObject camera;
    [SerializeField]
    private GameObject cameraPosition;
    [SerializeField]
    private float cameraLerpSpeed = 0.3f;
    [SerializeField]
    private float cameraRotationSpeed = 30;
    [SerializeField]
    private GameObject cameraPrefab;

    private void Start()
    {
    }

    //This takes in a PlayerController input because we access the HUD from it.
    public GameObject InstantiateCamera(PlayerController playerController)
    {
        camera = Instantiate(cameraPrefab, cameraPosition.transform.position, cameraPosition.transform.rotation); //Innitial spawn of the camera.

        Canvas hudCanvas = playerController.hud.GetComponent<Canvas>();
        hudCanvas.worldCamera = camera.GetComponent<Camera>();

        //Makes sure that the UI is drawn in front of everything.
        hudCanvas.planeDistance = 1;

        return (camera);
    }

    //This is used by the respawn() function. It will connect the camera later.
    public GameObject InstantiateCamera()
    {
        camera = Instantiate(cameraPrefab, cameraPosition.transform.position, cameraPosition.transform.rotation); //Innitial spawn of the camera.

        return (camera);
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
            //camera.transform.eulerAngles = new Vector3(45, cameraPosition.transform.eulerAngles.y, cameraPosition.transform.eulerAngles.z); //Sets the camera so it matches the global rotation of the player, with it looking down a little.
            camera.transform.rotation = Quaternion.RotateTowards(camera.transform.rotation, cameraPosition.transform.rotation, cameraRotationSpeed * Time.deltaTime);
        }
    }

    public void OnDestroy()
    {
        Destroy(camera);
    }
}
