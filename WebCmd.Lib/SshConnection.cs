using Renci.SshNet;

namespace WebCmd.Lib;

public class SshConnection : IDisposable
{
    private readonly SshClient _client;

    public bool IsConnected => _client.IsConnected;

    public ConnectionInfo ConnectionInfo => _client.ConnectionInfo;
    
    public SshConnection(string host, string user, string password)
    {
        _client = new SshClient(host, user, password);
    }

    public async Task ConnectAsync()
    {
        await _client.ConnectAsync(CancellationToken.None);
    }

    public string RunCommandWithResponse(string command)
    {
        if (_client.IsConnected == false) return "";
        var sshCommand = _client.RunCommand(command);
        return string.IsNullOrEmpty(sshCommand.Result) ? sshCommand.Error : sshCommand.Result;
    }

    public void Dispose()
    {
        _client.Disconnect();
    }
}

public static class SshClientsPool
{
    private static readonly List<SshConnection?> Clients = new List<SshConnection?>();

    public static async Task<SshConnection> GetIfNullCreateConnect(string host, string username, string password)
    {
        var client =
            Clients.FirstOrDefault(c => c != null && c.ConnectionInfo.Host == host && c.ConnectionInfo.Username == username);
        if (client != null)
        {
            if (client.IsConnected) return client;
            Clients.Remove(client);
            client.Dispose();
        }
        var newClient = new SshConnection(host, username, password);
        await newClient.ConnectAsync();
        Clients.Add(newClient);
        return newClient;
    }
}