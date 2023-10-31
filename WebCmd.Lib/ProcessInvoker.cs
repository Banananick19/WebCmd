using System.Diagnostics;

namespace WebCmd.Lib;

public interface IProcessInvoker
{
    string GetCommandName();
    void Process(string arguments);
    string ResultProcess();
    Task ProcessAsync(string arguments);
    bool IsSuccess();
}

public abstract class ProcessInvoker : IProcessInvoker
{
    protected Process? CurrentProcess = new Process();

    public abstract string GetCommandName();

    public void Process(string arguments)
    {
        CurrentProcess = System.Diagnostics.Process.Start(new ProcessStartInfo()
        {
            FileName = GetCommandName(),
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        });
        CurrentProcess?.WaitForExit();
    }

    public string ResultProcess()
    {
        var result = CurrentProcess?.StandardOutput.ReadToEnd() ?? "";
        return result;
    }

    public async Task ProcessAsync(string arguments)
    {
        CurrentProcess = System.Diagnostics.Process.Start(new ProcessStartInfo()
        {
            FileName = GetCommandName(),
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        });
        if (CurrentProcess != null) await CurrentProcess.WaitForExitAsync();
    }

    public abstract bool IsSuccess();
}

public class PingProcessInvoker : ProcessInvoker
{
    public override string GetCommandName()
    {
        return "ping";
    }

    public override bool IsSuccess()
    {
        return CurrentProcess is { ExitCode: 0 };
    }
}

public class SshProcessInvoker : ProcessInvoker
{
    public override string GetCommandName()
    {
        return "nmap";
    }

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
        if (OperatingSystem.IsWindows())
        {
            await pI.ProcessAsync(host);
        } else await pI.ProcessAsync( $"-c 4 {host}");
        return pI;
    }
}

public static class SshCmdService
{
    public static async Task<IProcessInvoker> StartProcessAsync(string host)
    {
        var pI = new SshProcessInvoker();
        await pI.ProcessAsync($"-p22 {host}");
        return pI;
    }
}

