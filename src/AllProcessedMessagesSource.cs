using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RoutingVisualization
{
    internal static class AllProcessedMessagesSource
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new LowercaseContractResolver()
        };

        public static async Task RegisterListener(Action<Message> onNext)
        {
            var client = new HttpClient(new HttpClientHandler {UseDefaultCredentials = true});
            var loop = true;
            Console.CancelKeyPress+=(s,e)=>
            {
                Console.WriteLine("Stopping gracefully...");
                e.Cancel = true;
                loop = false;
            };
            var page = 1;
            var msgNr = 1;
            try
            {
                do
                {
                    var response = await client.GetAsync(
                            $"{ConfigurationManager.AppSettings["ServiceControlAddress"]}/messages/?include_system_messages=False&page={page++}")
                        .ConfigureAwait(false);
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = JsonConvert.DeserializeObject<Message[]>(responseContent, JsonSettings);
                    var count = 0;
                    foreach (var message in result)
                    {
                        onNext(message);
                        Console.Write($"\rProcessing message {msgNr++}");
                        count++;
                    }
                    if (count == 0)
                    {
                        break;
                    }

                } while (loop);
            }
            catch (TaskCanceledException)
            { }
        }


        private class LowercaseContractResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                return propertyName.ToLower();
            }
        }



    }
}