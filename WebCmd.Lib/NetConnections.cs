namespace WebCmd.Lib;

public static class ParserNetConnections
{
    public static List<NetConnection> ParseConnections(string commandOutput)
    {
        var connections = new List<NetConnection>();

        var lines = commandOutput.Split("\n").Skip(1);
        
        foreach (var line in lines)
        {
            try
            {
                var connection = new NetConnection();
                var splited = line.Split(" ").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                connection.Protocol = splited[0];
                connection.LocalAddress = splited[4];
                connection.PeerAddress = splited[5];
                var processLine = splited[6];
                var processParams = processLine.Split(",");
                connection.PidProcess = processParams[1].Replace("pid=", "");
                connections.Add(connection);
            } catch (Exception){}
             
        }
        
        return connections;
    }
}

public class NetConnection
{
    public NetConnection(string protocol, string localAddress, string peerAddress, string pidProcess)
    {
        Protocol = protocol;
        LocalAddress = localAddress;
        PeerAddress = peerAddress;
        PidProcess = pidProcess;
    }

    public NetConnection()
    {
    }

    public string Protocol { get; set; }
    public string LocalAddress { get; set; }
    public string PeerAddress { get; set; }
    public string PidProcess { get; set; }
}