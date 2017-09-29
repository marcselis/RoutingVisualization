using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoutingVisualization
{
    public class EndpointDetails
    {
        public string Name { get; set; }
        public string Host { get; set; }
    }

    public class Message
    {
        public string Id { get; set; }
        [JsonConverter(typeof(DictionaryConverter))]
        public IDictionary<string, string> Headers { get; set; }
        // public string MessageId { get; set; }
        public EndpointDetails Sending_Endpoint { get; set; }
        public EndpointDetails Receiving_Endpoint { get; set; }
        public string Message_Type { get; set; }
    }
}