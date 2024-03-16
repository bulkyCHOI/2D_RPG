public class CreateAccountPakcetReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class CreateAccountPakcetRes
{
    public bool CreateOk { get; set; }
}

public class LoginAccountPakcetReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class ServerInfo
{
    public string ServerName { get; set; }
    public string ServerIp { get; set; }
    public int ServerPort { get; set; }
    public int BusyScore { get; set; }
}

public class LoginAccountPakcetRes
{
    public bool LoginOk { get; set; }
    public int AccountId { get; set; }
    public int Token { get; set; }
    public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
}