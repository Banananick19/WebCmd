using System.Diagnostics;

namespace WebCmd.Lib;

public interface IProcessInvoker
{
    void Process(string command, string arguments);
    string ResultProcess();
    Task ProcessAsync(string command, string arguments);
    bool IsSuccess();
}

public abstract class ProcessInvoker : IProcessInvoker
{
    protected Process? _currentProcess = new Process();

    public void Process(string command, string arguments)
    {
        _currentProcess = System.Diagnostics.Process.Start(new ProcessStartInfo()
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        });
        _currentProcess?.WaitForExit();
    }

    public string ResultProcess()
    {
        if (_currentProcess == null) return default;
        var result = _currentProcess.StandardOutput.ReadToEnd();
        return result;
    }

    public async Task ProcessAsync(string command, string arguments)
    {
        _currentProcess = System.Diagnostics.Process.Start(new ProcessStartInfo()
        {
            FileName = command,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        });
        if (_currentProcess != null) await _currentProcess.WaitForExitAsync();
    }

    public abstract bool IsSuccess();
}

public class PingProcessInvoker : ProcessInvoker
{
    public override bool IsSuccess()
    {
        return _currentProcess is { ExitCode: 0 };
    }
}

public class SshProcessInvoker : ProcessInvoker
{
    public override bool IsSuccess()
    {
        var result = ResultProcess();
        return !result.Contains("22/tcp closed") && result.Contains("22/tcp");
    }
}



public static class PingCmdService
{
    public static async Task<IProcessInvoker> StartProcessAsync(string host)
    {
        var pI = new PingProcessInvoker();
        await pI.ProcessAsync("ping", $"-c 4 {host}");
        return pI;
    }
}

public static class SshCmdService
{
    public static async Task<IProcessInvoker> StartProcessAsync(string host)
    {
        var pI = new SshProcessInvoker();
        await pI.ProcessAsync("nmap", $"-p22 {host}");
        return pI;
    }
}

