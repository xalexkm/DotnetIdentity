using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;

public class MyModel
{
    public string Name { get; set; }
    public string Message { get; set; }
}

namespace DotnetFunctions
{
    public class MyHttpTrigger
    {
        private readonly ILogger<MyHttpTrigger> _logger;
        
        // Define the log message as a static field
        private static readonly Action<ILogger, string?, string?, Exception?> _logRequestProcessed =
            LoggerMessage.Define<string?, string?>(
                LogLevel.Information,
                new EventId(1, "RequestProcessed"),
                "Request processed successfully - Name:{Name} Message:{Message}");
        
        private record ApiResponse<T>
        {
            public string Message { get; init; } = string.Empty;
            public T? Data { get; init; }
        }

        public MyHttpTrigger(ILogger<MyHttpTrigger> logger)
        {
            _logger = logger;
        }

        [Function("MyHttpTrigger")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var data = await JsonSerializer.DeserializeAsync<MyModel>(req.Body);
                
                _logRequestProcessed(_logger, data?.Name, data?.Message, null);

                return new OkObjectResult(new ApiResponse<MyModel>
                {
                    Message = "Request processed successfully",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing request: {Message}", ex.Message);
                return new BadRequestObjectResult("Error processing request body");
            }
        }
    }
}