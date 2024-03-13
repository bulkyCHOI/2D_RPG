using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAccountPakcetReq
{
    public string AccountName;
    public string Password;
}

public class CreateAccountPakcetRes
{
    public bool CreateOk;
}

public class LoginAccountPakcetReq
{
    public string AccountName;
    public string Password;
}

public class ServerInfo
{
    public string ServerName;
    public string ServerIp;
    public int CrowdedLevel;
}

public class LoginAccountPakcetRes
{
    public bool LoginOk;
    public List<ServerInfo> ServerList = new List<ServerInfo>();
}

//public class WebPacket
//{
//    public static void SendCreateAccount(string account, string password)
//    {
//        CreateAccountPakcetReq packet = new CreateAccountPakcetReq()
//        {
//            AccountName = account,
//            Password = password
//        };

//        Managers.Web.SendPostRequest<CreateAccountPakcetRes>("account/create", packet, (res) =>
//        {
//            Debug.Log(res.CreateOk);
//        });
//    }

//    public static void SendLoginAccount(string account, string password)
//    {
//        LoginAccountPakcetReq packet = new LoginAccountPakcetReq()
//        {
//            AccountName = account,
//            Password = password
//        };

//        Managers.Web.SendPostRequest<LoginAccountPakcetRes>("account/login", packet, (res) =>
//        {
//            Debug.Log(res.LoginOk);
//        });
//    }
//}