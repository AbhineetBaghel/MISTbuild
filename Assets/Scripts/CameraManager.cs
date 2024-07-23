using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public Camera playerCamera;
    public Camera sceneViewCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SwitchToSceneViewCamera()
    {
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (sceneViewCamera != null)
            sceneViewCamera.gameObject.SetActive(true);
    }

    public void SwitchToPlayerCamera(Camera newPlayerCamera)
    {
        if (sceneViewCamera != null)
            sceneViewCamera.gameObject.SetActive(false);

        playerCamera = newPlayerCamera;
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);
    }
}
