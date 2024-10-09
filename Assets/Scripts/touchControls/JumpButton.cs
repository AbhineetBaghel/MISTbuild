using UnityEngine;
using UnityEngine.UI;

public class JumpButton : MonoBehaviour
{
    public static bool jumpPressed = false;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnJumpButtonPressed);
    }

    private void OnJumpButtonPressed()
    {
        jumpPressed = true;
    }

    private void LateUpdate()
    {
        jumpPressed = false;
    }
}