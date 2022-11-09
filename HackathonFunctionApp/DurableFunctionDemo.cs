using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace HackathonFunctionApp
{
    public static class DurableFunctionDemo
    {
        [FunctionName("DurableFunctionDemo")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

			var orchestrationInput = context.GetInput<OrchestrationInit>();

			outputs.Add(await context.CallActivityAsync<string>("DurableFunctionDemo_Hello", new ActivityParams { Name = "Tokyo", Timeout = orchestrationInput.Timeout }));
			
			return outputs;
        }


		/// <summary>
		/// This represents the long running operation on the App Server side. 
        /// Currently mocked by sleeping.
		/// </summary>
		[FunctionName("DurableFunctionDemo_Hello")]
        public static string SayHello([ActivityTrigger] ActivityParams activityParams, ILogger log)
        {
            log.LogInformation($"Saying hello to {activityParams.Name}.");
			log.LogInformation($"Sleeping inside activity for {activityParams.Timeout}...");
			Thread.Sleep(activityParams.Timeout);
			return $"Hello {activityParams.Name}!";
        }

        [FunctionName("DurableFunctionDemo_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var content = await req.Content.ReadAsStringAsync();
            string instanceId = await starter.StartNewAsync("DurableFunctionDemo", null, JsonSerializer.Deserialize<OrchestrationInit>(content));

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}