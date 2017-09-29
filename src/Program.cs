using System;
using System.Linq;

namespace RoutingVisualization
{
    static class Program
    {
        static void Main(string[] args)
        {
            var modelBuilder = new ModelBuilder(GetNodeStrategy());
            Console.WriteLine("Querying ServiceControl API.  Press CTRL-C to abort...");
            AllProcessedMessagesSource.RegisterListener(modelBuilder.Accept).Wait();
            var model = modelBuilder.GetModel();
            Console.WriteLine("Model generated");
            var dgml = DgmlRouteDocFactory.CreateDgml(model);
            var outputFileName = args.FirstOrDefault() ?? "route-graph";
            if (!outputFileName.EndsWith(".dgml", StringComparison.CurrentCultureIgnoreCase))
            {
                outputFileName += ".dgml";
            }
            dgml.Save(outputFileName);
            Console.WriteLine($"Created {outputFileName}");
        }

        private static NodeStrategy GetNodeStrategy()
        {
            // Logical Routing Strategy collapses endpoints with the same name into a single endpoint. Even if running on different hosts
            var logicalEndpoints = new LogicalRoutingNodeStrategy();
            // Physical Routing Strategy will split up endpoints that have the same name but run on different hosts
            //var physicalEndpoints = new PhysicalRoutingNodeStrategy();

            var endpoints = logicalEndpoints;
            
            // All messages of the same type that come from the same sender will be collapsed into a single message node
            var collapseFromSender = new CollaseMessagesFromSameSenderMessageNodeStrategy(endpoints);

            // All messages of the same type that go to the same receiver will be collapsed into a single message node
            //var collapseToReceiver = new CollapseMessagesToSameReceiverMessageNodeStrategy(endpoints);

            // All published events of the same type and sender will collapse into one, 
            // all non-events of the same type and sent to the same reciever collapses into one
            //var intentBasedMessageNodeStrategy = new IntentBasedMessageNodeStrategy(collapseToReceiver);
            //intentBasedMessageNodeStrategy.Add("Publish", collapseFromSender);

            var messages = collapseFromSender;

            return new NodeStrategy(endpoints, messages);
        }
    }
}
