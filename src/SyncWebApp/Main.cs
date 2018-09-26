using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using SyncWebApp.Model;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SyncWebApp
{
    public static class Main
    {
        private static HttpClient httpClient = new HttpClient();
        private static GraphServiceClient graphClient;

        static Main()
        {
            Uri bambooBaseUri = null;
            Uri.TryCreate(Environment.GetEnvironmentVariable("BAMBOO_API_URL"), UriKind.Absolute, out bambooBaseUri);
            httpClient.BaseAddress = bambooBaseUri;

            var byteArray = Encoding.ASCII.GetBytes($"{Environment.GetEnvironmentVariable("BAMBOO_API_KEY")}:s");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            graphClient = new GraphServiceClient(null, null);

        }

        [FunctionName("OnNewO365Update")]
        public static async Task Test(
            [QueueTrigger("%QUEUE_UPDATEO365%")] UpdateO365Message message,
            ILogger log,
            [Token(
                Identity = TokenIdentityMode.ClientCredentials,
                IdentityProvider = "AAD",
                Resource = "https://graph.microsoft.com")] string token
        )
        {
            graphClient.AuthenticationProvider = new DelegateAuthenticationProvider(
                        (requestMessage) =>
                        {
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

                            return Task.CompletedTask;
                        });

            var userLookup = await graphClient.Users.Request().Filter($"mail eq '{message.WorkEmail}'").GetAsync();

            var userId = userLookup.FirstOrDefault()?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var user = graphClient.Users[userId];

            await user.Request().UpdateAsync(new User { JobTitle = message.JobTitle, OfficeLocation = message.Location, Department = message.Department });

            var managerLookup = await graphClient.Users.Request().Filter($"mail eq '{message.SupervisorEmail}'").GetAsync();
            var managerId = managerLookup.FirstOrDefault()?.Id;
            if (string.IsNullOrEmpty(managerId))
            {
                return;
            }

            var manager = managerLookup.FirstOrDefault();
            await user.Manager.Reference.Request().PutAsync(manager.Id);




            return;

        }


        [FunctionName("OnNewEmployee")]
        public static async Task OnNewMessage(
            [QueueTrigger("%QUEUE_EMPLOYEE%")]EmployeeMessage message,
            [Queue("%QUEUE_UPDATEO365%")] IAsyncCollector<UpdateO365Message> collector
        )
        {
            var employeeResponse = await httpClient.GetAsync($"employees/{message.Id}?fields=supervisorEid");
            var content = await employeeResponse.Content.ReadAsAsync<JObject>();
            var supervisorId = content.GetValue("supervisorEid").Value<string>();
            var supervisorResponse = await httpClient.GetAsync($"employees/{supervisorId}?fields=workemail");
            var content2 = await supervisorResponse.Content.ReadAsAsync<JObject>();
            var supervisorEmail = content2.GetValue("workemail").Value<string>();
            await collector.AddAsync(message.To(supervisorEmail, supervisorId));

        }

        [FunctionName("ProcessDirectory")]
        public static async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "process/start")] HttpRequest req,
            [Queue("%QUEUE_EMPLOYEE%")] IAsyncCollector<EmployeeMessage> collector
        )
        {
            var response = await httpClient.GetAsync("employees/directory");
            var content = await response.Content.ReadAsStringAsync();
            var directory = await response.Content.ReadAsAsync<Directory>();

            foreach (var item in directory.Employees.ToEmployeeMessage())
            {
                await collector.AddAsync(item);
            }

            return new OkObjectResult(directory);


        }

    }
}
