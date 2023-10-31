namespace WebCmd.Lib;



public class OVPNUserInfo
{
    public OVPNUserInfo(string name, string realAddress, long bytesReceived, long bytesSent, string connectedSince)
    {
        Name = name;
        RealAddress = realAddress;
        BytesReceived = bytesReceived;
        BytesSent = bytesSent;
        ConnectedSince = connectedSince;
    }

    public OVPNUserInfo()
    {
    }

    public string Name { get; set; }
    public string RealAddress { get; set; }
    public long BytesReceived { get; set; }
    public long BytesSent { get; set; }
    public string ConnectedSince { get; set; }
}

public static class OVPNParser 
{
    public static List<OVPNUserInfo> ParseStatus(string statusResponse)
    {
        var users = new List<OVPNUserInfo>();
        if (string.IsNullOrWhiteSpace(statusResponse)) return users;
        var splinted = statusResponse.Split("\n");
        var indexOfStartUsers = splinted
            .Select((l, i) => new { Line = l, Index = i }).FirstOrDefault(l => l.Line.StartsWith("Common Name"))?.Index;
        if (indexOfStartUsers == null) return users;
        var currentLineIndex = indexOfStartUsers.Value + 1;
        if (splinted[currentLineIndex].Contains("ROUTING TABLE")) return users;
        var currentLine = splinted[currentLineIndex];
        while (!currentLine.Contains("ROUTING TABLE"))
        {
            var user = ParseStatusUserLine(currentLine);
            users.Add(user);
            currentLineIndex += 1;
            currentLine = splinted[currentLineIndex];
        }
        return users;
    }

    private static OVPNUserInfo ParseStatusUserLine(string line)
    {
        /*
         * OpenVPN CLIENT LIST
            Updated,Thu Oct 26 14:27:58 2023
            Common Name,Real Address,Bytes Received,Bytes Sent,Connected Since
            user,172.20.0.1:33454,206407,3461,Thu Oct 26 14:27:51 2023
            ROUTING TABLE
         */
        var user = new OVPNUserInfo();
        var splited = line.Split(",");
        user.Name = splited[0];
        user.RealAddress = splited[1];
        user.BytesReceived = long.Parse(splited[2].Trim());
        user.BytesSent = long.Parse(splited[3].Trim());
        user.ConnectedSince = splited[4].Trim();
        return user;
    }
}

public static class OVPNTelnet
{
    public static async Task<string> GetStatusResponse(TelnetWrapper telnetClient)
    {
        if (!telnetClient.IsConnected) return "";
        await telnetClient.ReadLastMessage();
        await telnetClient.SendText("status");
        Thread.Sleep(500);//даем время сброситься интерфейсу (судя по всему это нужно)
        return await telnetClient.ReadLastMessage();
    }
}
