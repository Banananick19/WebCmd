namespace WebCmd.App;

public class Config
{
    public Config(IEnumerable<ConfigHost> hosts)
    {
        Hosts = hosts;
    }

    public Config()
    {
    }

    public  IEnumerable<ConfigHost> Hosts { get; set; }
} 

public class ConfigHost
{
    public ConfigHost(HostType type, string host, string args)
    {
        Type = type;
        Host = host;
        Args = args;
    }

    public ConfigHost()
    {
    }

    public HostType Type { get; set; }
    public string Host { get; set; }
    public string Args { get; set; }

    public enum HostType
    {
        Ping = 0,
        Ssh = 1,
        PMetrics = 2,
        OvpnUsers = 3,
        SshCommand = 4,
        NetConnectionOnSsh = 5
    }
}

public class SshCommandArgs
{
    public SshCommandArgs(string password, string username, string command)
    {
        Password = password;
        Username = username;
        Command = command;
    }

    public SshCommandArgs()
    {
    }

    public string Password { get; set; }
    public string Username { get; set; }
    public string Command { get; set; }
}