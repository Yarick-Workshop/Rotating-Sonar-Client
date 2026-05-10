namespace Rotating.Sonar.Client.Common.SerialPorts;

using System.IO.Ports;

public static class SerialPortProvider
{
    public static string[] GetPortNames()
    {
        return SerialPort.GetPortNames();
    }
}
