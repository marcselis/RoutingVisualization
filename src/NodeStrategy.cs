using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RoutingVisualization
{
    public class NodeStrategy : INodeStrategy<EndpointDetails>, INodeStrategy<Message>
    {
        private readonly INodeStrategy<EndpointDetails> _endpointNodeStrategy;
        private readonly INodeStrategy<Message> _messageNodeStrategy;

        public NodeStrategy(INodeStrategy<EndpointDetails> endpointNodeStrategy, INodeStrategy<Message> messageNodeStrategy)
        {
            _endpointNodeStrategy = endpointNodeStrategy;
            _messageNodeStrategy = messageNodeStrategy;
        }

        public string GetNodeId(EndpointDetails details)
        {
            return _endpointNodeStrategy.GetNodeId(details);
        }

        public string GetNodeId(Message details)
        {
            return _messageNodeStrategy.GetNodeId(details);
        }
    }

    public interface INodeStrategy<in T> 
    {
        string GetNodeId(T details);
    }

    public abstract class NodeStrategy<T> : INodeStrategy<T>
    {
        public abstract string GetNodeId(T details);

        protected static string ToNodeName(params string[] parts)
        {
            return Regex.Replace(string.Join("_", parts), "[-.]", "_").ToLower();
        }
    }
    

    class PhysicalRoutingNodeStrategy : NodeStrategy<EndpointDetails>
    {
        public override string GetNodeId(EndpointDetails details)
        {
            return ToNodeName(details.Host, details.Name);
        }
    }

    class LogicalRoutingNodeStrategy : NodeStrategy<EndpointDetails>
    {
        public override string GetNodeId(EndpointDetails details)
        {
            if (details == null)
                return null;
            return ToNodeName(details.Name);
        }
    }

    class CollaseMessagesFromSameSenderMessageNodeStrategy : NodeStrategy<Message>
    {
        private readonly NodeStrategy<EndpointDetails> _endpointNodeStrategy;

        public CollaseMessagesFromSameSenderMessageNodeStrategy(NodeStrategy<EndpointDetails> endpointNodeStrategy)
        {
            _endpointNodeStrategy = endpointNodeStrategy;
        }

        public override string GetNodeId(Message details)
        {
            var intent = details.Headers["NServiceBus.MessageIntent"];
            var sendingEndpointNodeId = _endpointNodeStrategy.GetNodeId(details.Sending_Endpoint);
            return ToNodeName(sendingEndpointNodeId, intent, details.Message_Type);
        }
    }

    class CollapseMessagesToSameReceiverMessageNodeStrategy : NodeStrategy<Message>
    {
        private readonly NodeStrategy<EndpointDetails> _endpointNodeStrategy;

        public CollapseMessagesToSameReceiverMessageNodeStrategy(NodeStrategy<EndpointDetails> endpointNodeStrategy)
        {
            _endpointNodeStrategy = endpointNodeStrategy;
        }

        public override string GetNodeId(Message details)
        {
            var intent = details.Headers["NServiceBus.MessageIntent"];
            var receivingEndpointNodeId = _endpointNodeStrategy.GetNodeId(details.Receiving_Endpoint);
            return ToNodeName(receivingEndpointNodeId, intent, details.Message_Type);
        }
    }

    class IntentBasedMessageNodeStrategy : INodeStrategy<Message>
    {
        private readonly IDictionary<string, INodeStrategy<Message>> _strategyMap = new Dictionary<string, INodeStrategy<Message>>(StringComparer.InvariantCultureIgnoreCase);
        private readonly INodeStrategy<Message> _defaultStrategy;

        public IntentBasedMessageNodeStrategy(INodeStrategy<Message> defaultStrategy)
        {
            _defaultStrategy = defaultStrategy;
        }

        public string GetNodeId(Message details)
        {
            var intent = details.Headers["NServiceBus.MessageIntent"];

            INodeStrategy<Message> strategy;
            if (!_strategyMap.TryGetValue(intent, out strategy))
                strategy = _defaultStrategy;

            return strategy.GetNodeId(details);
        }

        public void Add(string intent, INodeStrategy<Message> strategy)
        {
            _strategyMap.Add(intent, strategy);
        }
    }
}