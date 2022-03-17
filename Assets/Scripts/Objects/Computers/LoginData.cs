using UnityEngine;

public class LoginData : MonoBehaviour
{
    public enum UserGroup
    {
        SuperUser = 0,
        Admin,
        User,
        Guest
    };

    public Sprite image;
    public string username = "";
    public string password = "";
    public UserGroup userGroup = UserGroup.User;
    public bool keepLogin = false;

    public bool Check(string password)
    {
        return this.password.Equals(password);
    }
}
