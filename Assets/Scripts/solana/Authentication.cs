using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class Authentication : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button loginButton;
    [SerializeField] private UnityEngine.UI.Button logoutButton;
    [SerializeField] private TMPro.TextMeshProUGUI publicKeyText;
    [SerializeField] public GameObject CreateRoomBtn;
    [SerializeField] public GameObject JoinRoomBtn;
    [SerializeField] public GameObject JoinRoomList;



    private void Awake() 
    {
        loginButton.onClick.AddListener(() => Login());
        logoutButton.onClick.AddListener(() => Logout());
    }

    private void OnEnable()
    {
        Web3.OnLogin += OnLogin;
        Web3.OnLogout += OnLogout;
    }

    private void OnDisable() 
    {
        Web3.OnLogin -= OnLogin;
        Web3.OnLogout -= OnLogout;
    }

    public async void Login()
    {
        Debug.Log("Login");
        await Web3.Instance.LoginWalletAdapter();
        publicKeyText.text = Web3.Account.PublicKey.ToString();
        CreateRoomBtn.SetActive(true);
        JoinRoomBtn.SetActive(true);
        JoinRoomList.SetActive(true);


    }

    public void Logout()
    {
        Debug.Log("Logout");
        Web3.Instance.Logout();
    }

    private void OnLogin(Account account)
    {
        loginButton.gameObject.SetActive(false);
        logoutButton.gameObject.SetActive(true);
       // SceneManager.LoadScene("SampleScene");
    }

    private void OnLogout()
    {
        loginButton.gameObject.SetActive(true);
        logoutButton.gameObject.SetActive(false);
    }
}