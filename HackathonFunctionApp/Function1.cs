using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System;
using System.Text.Json;

namespace HackathonFunctionApp
{
    public static class Function1
    {
        const int MAX_WAIT_TIME_MS = 5000;
        const int DEFAULT_TIMEOUT_MS = 15000;

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
			var timeoutInQueryParam = req.Query["timeout"];
            var timeout = DEFAULT_TIMEOUT_MS;
            
            if (!string.IsNullOrEmpty(timeoutInQueryParam))
            {
                timeout = int.Parse(timeoutInQueryParam);
            }


			var httpClient = new HttpClient();

            var sw = new Stopwatch();
            sw.Start();

            var orchestrationResponse = await SendMessageToDurableOrchestration(httpClient, timeout);
            var statusContent = await GetDurableRunStatus(orchestrationResponse, httpClient);

			while (statusContent.RuntimeStatus != "Completed" && sw.ElapsedMilliseconds < MAX_WAIT_TIME_MS)
            {
				Thread.Sleep(500);
				log.LogInformation($"Heartbeat.. {sw.ElapsedMilliseconds}");
				statusContent = await GetDurableRunStatus(orchestrationResponse, httpClient);
			}


			sw.Stop();

            if (statusContent.RuntimeStatus == "Completed")
            {
				return new OkObjectResult(statusContent.Output);
			}

            return new AcceptedResult(new Uri(orchestrationResponse.StatusQueryGetUri), new { StatusUri = orchestrationResponse.StatusQueryGetUri });
            
        }
        private static async Task<DurableResponse> SendMessageToDurableOrchestration(HttpClient httpClient, int timeout)
        {
            var response = await httpClient.PostAsync(
                "http://localhost:7012/api/DurableFunctionDemo_HttpStart",
                new StringContent(JsonSerializer.Serialize(new OrchestrationInit{Timeout = timeout})));
			var content = await response.Content.ReadAsAsync<DurableResponse>();
            return content;
		}

        private static async Task<DurableStatusResponse> GetDurableRunStatus(DurableResponse orchestrationResponse, HttpClient httpClient)
        {
			var checkStatusMessage = new HttpRequestMessage(HttpMethod.Get, orchestrationResponse.StatusQueryGetUri);
			var statusResponse = await httpClient.SendAsync(checkStatusMessage);
			var statusContent = await statusResponse.Content.ReadAsAsync<DurableStatusResponse>();
            return statusContent;
		}
    }
}
