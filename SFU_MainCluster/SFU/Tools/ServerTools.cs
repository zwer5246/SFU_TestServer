using MessagesModels.Enums;
using MessagesModels.Models;
using Newtonsoft.Json;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;

namespace SFU_MainCluster.SFU.Tools
{
    public class ServerTools
    {
        private static Microsoft.Extensions.Logging.ILogger _logger;
        public static Microsoft.Extensions.Logging.ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = AddConsoleLogger();
                }

                return _logger;
            }
            set
            {
                _logger = value;
            }
        }

        public ServerTools() 
        {

        }

        public static string CreateAndSerializeMessage(object Data, MessageType type)
        {
            BaseMessage message = new BaseMessage();
            message.Data = Data;
            message.Type = type;
            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static string SerializeMessage(BaseMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public static BaseMessage DeSerializeMessage(string JSONMessage)
        {
            BaseMessage? message = JsonConvert.DeserializeObject<BaseMessage>(JSONMessage);

            return message;
        }

        private static Microsoft.Extensions.Logging.ILogger AddConsoleLogger()
        {
            var seriLogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
            var factory = new SerilogLoggerFactory(seriLogger);
            SIPSorcery.LogFactory.Set(factory);
            return factory.CreateLogger<Program>();
        }
    }
}
