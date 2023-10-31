namespace WebCmd.Lib;

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

public enum Verbs {
    WILL = 251,
    WONT = 252,
    DO = 253,
    DONT = 254,
    IAC = 255
}
public enum Options
{
    SGA = 3
}
public class TelnetConnection : IDisposable
{
    TcpClient tcpSocket;
    int TimeOutMs = 2000; //стандартный таймаут
    public string Host { get; protected set; }
    public int Port { get; protected set; }
    public TelnetConnection(string hostname, int port)
    {
        Host = hostname;
        Port = port;
        tcpSocket = new TcpClient(Host, Port);
    }
    public string Login(string Username,string Password,int LoginTimeOutMs)
    {
        int oldTimeOutMs = TimeOutMs;
        TimeOutMs = LoginTimeOutMs; //таймаут для логина (он длиннее)
        string s = Read();
        if (!s.TrimEnd().EndsWith(":"))
           throw new Exception("Failed to connect : no login prompt");
        WriteLine(Username);
        s += Read();
        if (!s.TrimEnd().EndsWith(":"))
            throw new Exception("Failed to connect : no password prompt");
        WriteLine(Password);
        s += Read();
        TimeOutMs = oldTimeOutMs; //сбрасываем на стандартный таймаут
        return s;
    }
    public void WriteLine(string cmd)
    {
        Write(cmd + "\n");
    }
    public void Write(string cmd)
    {
        if (!tcpSocket.Connected) return;
        byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(cmd.Replace("\0xFF","\0xFF\0xFF"));
        tcpSocket.GetStream().Write(buf, 0, buf.Length);
    }
    public string Read()
    {
        if (!tcpSocket.Connected) return null;
        StringBuilder sb=new StringBuilder();
        do
        {
            ParseTelnet(sb);
            Thread.Sleep(TimeOutMs);
        } while (tcpSocket.Available > 0);
        return sb.ToString().Replace(" [K", "");//убираем символ от (наверное) роутера
    }
    public bool IsConnected
    {
        get { return tcpSocket.Connected; }
    }
    void ParseTelnet(StringBuilder sb)
    {
        while (tcpSocket.Available > 0)
        {
            int input = tcpSocket.GetStream().ReadByte();
            switch (input)
            {
                case -1 :
                    break;
                case (int)Verbs.IAC:
                    // interpret as command
                    int inputverb = tcpSocket.GetStream().ReadByte();
                    if (inputverb == -1) break;
                    switch (inputverb)
                    {
                        case (int)Verbs.IAC: 
                            //literal IAC = 255 escaped, so append char 255 to string
                            sb.Append(inputverb);
                            break;
                        case (int)Verbs.DO: 
                        case (int)Verbs.DONT:
                        case (int)Verbs.WILL:
                        case (int)Verbs.WONT:
                            // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                            int inputoption = tcpSocket.GetStream().ReadByte();
                            if (inputoption == -1) break;
                            tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                            if (inputoption == (int)Options.SGA )
                                tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL:(byte)Verbs.DO); 
                            else
                                tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT); 
                            tcpSocket.GetStream().WriteByte((byte)inputoption);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    sb.Append( (char)input );
                    break;
            }
        }
    }

    public void Dispose()
    {
        tcpSocket.Close();
    }
}

public class TelnetWrapper : IDisposable
{
    private readonly TelnetConnection _connection;

    public bool IsConnected => _connection.IsConnected;

    private static readonly List<TelnetConnection> _existedConnections = new List<TelnetConnection>();

    protected TelnetWrapper(TelnetConnection connection)
    {
        _connection = connection;
    }

    public static Task<TelnetWrapper> CreateConnection(string host, int port)
    {
        var con = _existedConnections.FirstOrDefault(i => i.Host == host && i.Port == port);
        if (con != null && con.IsConnected != false) return Task.FromResult(new TelnetWrapper(con));
        con = new TelnetConnection(host, port);
        _existedConnections.Add(con);
        return Task.FromResult(new TelnetWrapper(con));
    }

    public Task<string> ReadLastMessage()
    {
        return Task.FromResult(_connection.Read());
    }

    public Task SendText(string text)
    {
        _connection.WriteLine(text);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _existedConnections.Remove(_connection);
        _connection.Dispose();
    }
}

