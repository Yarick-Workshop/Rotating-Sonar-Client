namespace Rotating.Sonar.ClientApp.Console;

using System;

public interface IComPortListener
{
    string PortName { get; }
    void Listen(Func<bool> isCancelled, Action<string>? newLineCallBack = null);
}
