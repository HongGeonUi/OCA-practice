using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace week2_1
{
    public class NewTrigger
    {
        private readonly ILogger _logger;

        public NewTrigger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<NewTrigger>();
        }

        [Function("NewTrigger")]

        [OpenApiOperation(operationId: nameof(NewTrigger.Run), tags: new[] { "name" })]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Required = true, Description = "The request body")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]


        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "completions")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var prompt = req.ReadAsString();

            var endpoint = Environment.GetEnvironmentVariable("AI_Endpoint");
            var apiKey = Environment.GetEnvironmentVariable("AI_ApiKey");
            var Model = Environment.GetEnvironmentVariable("AI_Model");
            int MaxTokens = 800;
            float Temperature = 0.7f;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    messages = new[]
                    {
                        new {role = "system", content =  "You are a helpful assistant. You are very good at summarizing the given text into 2-3 bullet points."},
                        new {role = "user", content = prompt}
                    },
                    max_tokens = MaxTokens,
                    model = Model,
                    temperature = Temperature
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var postRequest = await httpClient.PostAsync(endpoint, content);
                var result = await postRequest.Content.ReadAsStringAsync();
                dynamic resultContent = JsonConvert.DeserializeObject<dynamic>(result);
                string message = resultContent.choices[0].message.content;
            

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString(message);

                return response;
            }
        }
    }
}
