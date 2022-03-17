using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class LoginScreen : MonoBehaviour
{
    public delegate void OnValidLoginEvent(LoginData loginData);
    public event OnValidLoginEvent OnValidLogin;

    public bool IsVisible { get; private set; } = true;

    public Image background;
    public Transform loginDataParent;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public UIIconButton showHidePasswordButton;

    public UIOnOffButton keepLoginButton;

    public UIButton loginButton;
    public UIButton resetButton;
    public UIButton guestLoginButton;
    public RectTransform errorDisplay;
    public TextMeshProUGUI errorText;

    public UIIconButton shutdownButton;

    private Dictionary<string, LoginData> dict;

    private TMP_InputField currentField;

    private void Start()
    {
        showHidePasswordButton.SetAction(ToggleShowHidePassword);
        loginButton.SetAction(Login);
        resetButton.SetAction(ResetFields);
        guestLoginButton.SetAction(GuestLogin);

        SetVisible(false, true);

        if (null == dict)
        {
            InitDict();
        }
    }

    private void InitDict()
    {
        dict = new Dictionary<string, LoginData>();
        LoginData[] data = loginDataParent.GetComponentsInChildren<LoginData>();

        if (null == data || data.Length == 0)
            return;

        foreach (LoginData loginData in data)
        {
            if (!dict.ContainsKey(loginData.username))
                dict.Add(loginData.username, loginData);
        }
    }

    private void ToggleShowHidePassword()
    {
        passwordField.enabled = false;

        if (passwordField.contentType == TMP_InputField.ContentType.Password)
        {
            showHidePasswordButton.SetIcon(1);
            passwordField.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            showHidePasswordButton.SetIcon(0);
            passwordField.contentType = TMP_InputField.ContentType.Password;
        }

        passwordField.enabled = true;
        Focus(passwordField);
    }

    public void SetVisible(bool isVisible, bool instant = false)
    {
        if (IsVisible == isVisible)
            return;

        IsVisible = isVisible;
        Vector3 scale = IsVisible ? Vector3.one : new Vector3(0f, 1f, 1f);

        if (instant)
        {
            transform.localScale = scale;
            return;
        }

        shutdownButton.gameObject.SetActive(!IsVisible);
        Color color1 = IsVisible ? Color.black : Color.gray;
        Color color2 = IsVisible ? Color.gray : Color.black;
        background.color = color1;

        ShowError();

        Sequence seq = DOTween.Sequence().
            SetAutoKill(true);

        if (IsVisible)
        {
            seq.Append(transform.DOScale(scale, 0.25f)).
                Append(background.DOColor(color2, 0.25f)).
                OnComplete(() => {
                    ResetFields();
                    shutdownButton.gameObject.SetActive(true);
                });
        }
        else
        {
            seq.OnPlay(() => {
                    ResetFields();
                    shutdownButton.gameObject.SetActive(false);
                }).
                Append(background.DOColor(color2, 0.25f)).
                Append(transform.DOScale(scale, 0.25f));
        }

        seq.Play();
    }

    private void Error()
    {
        Action.ActionController.GetInstance().ActionDenied();
    }

    private void GuestLogin()
    {
        ResetFields();
        LoginData guestLogin = dict["guest"];
        usernameField.text = guestLogin.username;
        passwordField.text = guestLogin.password;
        Login();
    }

    private void Login()
    {
        string username = usernameField.text.Trim();
        string password = passwordField.text.Trim();

        usernameField.text = username;
        passwordField.text = password;

        usernameField.enabled = false;
        passwordField.enabled = false;

        if (!dict.ContainsKey(username))
        {
            ShowError("Invalid username!");
            Error();
            Focus(usernameField);
            return;
        }

        LoginData loginData = dict[username];

        if (!loginData.Check(password))
        {
            ShowError("Invalid username or password!");
            Focus(passwordField);
            Error();
            return;
        }

        loginData.keepLogin = keepLoginButton.IsOn;
        loginData.keepLogin &= loginData.userGroup > LoginData.UserGroup.SuperUser;
        loginData.keepLogin &= loginData.userGroup < LoginData.UserGroup.Guest;
        OnValidLogin?.Invoke(loginData);
        SetVisible(false);
    }

    private void ShowError(string text = null)
    {
        errorText.SetText(null != text ? text : "");
        errorDisplay.gameObject.SetActive(null != text);
    }

    private void ResetFields()
    {
        usernameField.enabled = false;
        passwordField.enabled = false;

        usernameField.text = "";
        passwordField.text = "";

        ShowError();

        passwordField.contentType = TMP_InputField.ContentType.Password;

        usernameField.enabled = true;
        passwordField.enabled = true;
        keepLoginButton.Reset();

        Focus(usernameField);
    }

    private void FocusNext()
    {
        TMP_InputField nextField = null;

        if (null == currentField || currentField == passwordField)
            nextField = usernameField;
        else if (currentField == usernameField)
            nextField = passwordField;

        if (null == nextField)
            return;

        Focus(nextField);
    }

    private void Focus(TMP_InputField field)
    {
        usernameField.enabled = true;
        passwordField.enabled = true;
        currentField = field;
        EventSystem.current.SetSelectedGameObject(field.gameObject, null);
    }

    private void Update()
    {
        if (!IsVisible)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            FocusNext();
        }
        else if (Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Return))
        {
            Login();
        }
    }
}
