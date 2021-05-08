using Microsoft.Extensions.Logging;

namespace SP.StudioCore.Log
{
    public class MqLoggerProvider:ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            throw new System.NotImplementedException();
        }
    }
}