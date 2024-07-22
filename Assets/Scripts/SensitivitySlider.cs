using Org.BouncyCastle.Asn1.Crmf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySlider : MonoBehaviour
{
    public Slider sensitivitySlider;
    public float minSensitivity = 0.1f;
    public float maxSensitivity = 10f;
    public GameObject sliderScreen;

    private void Start()
    {
        if (sensitivitySlider == null || sliderScreen == null)
        {
            Debug.LogError("Sensitivity Slider or SliderScreen is not assigned in the Inspector!");
            return;
        }

        // Set up the slider
        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;

        if (MouseLook.instance != null)
        {
            sensitivitySlider.value = MouseLook.instance.sensitivity.x;
        }
        else
        {
            Debug.LogWarning("MouseLook instance not found. Setting default sensitivity.");
            sensitivitySlider.value = (minSensitivity + maxSensitivity) / 2f;
        }

        // Add listener for when the slider value changes
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        // Ensure the slider is hidden at start
        sliderScreen.SetActive(false);
        Debug.Log("c to enable and disable");

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleSliderVisibility();
            Debug.Log("no");
        }
    }

    private void ToggleSliderVisibility()
    {
        sliderScreen.SetActive(!sliderScreen.activeSelf);

        // Toggle cursor visibility
        Cursor.visible = sliderScreen.activeSelf;
        Cursor.lockState = sliderScreen.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void OnSensitivityChanged(float newValue)
    {
        if (MouseLook.instance != null)
        {
            MouseLook.instance.SetSensitivity(newValue);
        }
        else
        {
            Debug.LogWarning("MouseLook instance not found. Sensitivity change not applied.");
        }
    }
}
