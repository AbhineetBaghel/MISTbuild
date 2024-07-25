using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using UnityEngine.UI;


public class Authentication : MonoBehaviour
{
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TMPro.TextMeshProUGUI publicKeyText;

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
    }

    private void OnLogout()
    {
        loginButton.gameObject.SetActive(true);
        logoutButton.gameObject.SetActive(false);
    }
}