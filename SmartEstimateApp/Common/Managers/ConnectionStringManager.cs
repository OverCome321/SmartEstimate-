using Microsoft.Extensions.Configuration;

namespace Common.Managers;

public static class ConnectionStringManager
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        string machineName = Environment.MachineName;
        switch (machineName)
        {
            case "DESKTOP-K81FSPL":
                return configuration.GetConnectionString("Connection1");
            case "DESKTOP-RE0M47N":
                return configuration.GetConnectionString("Connection2");
            default:
                return configuration.GetConnectionString("Connection1");
        }
    }
}
