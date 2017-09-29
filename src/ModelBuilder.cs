using System.Linq;

namespace RoutingVisualization
{
    public class ModelBuilder
    {
        private readonly Model _model;
        private readonly NodeStrategy _nodeStrategy;

        public ModelBuilder(NodeStrategy nodeStrategy)
        {
            _model = new Model();
            _nodeStrategy = nodeStrategy;
        }

        public void Accept(Message message)
        {
            if (message?.Headers == null)
            {
                return;
            }

            string intent;
            if (!message.Headers.TryGetValue("NServiceBus.MessageIntent", out intent))
            {
                return;
            }
            if (intent == "Subscribe" || intent == "Unsubscribe")
                return;

            var senderId = _nodeStrategy.GetNodeId(message.Sending_Endpoint);
            if (senderId != null)
            {
                var senderNode = _model.GetEndpoint(senderId);
                senderNode["Label"] = message.Sending_Endpoint.Name;
            }

            var receiverId = _nodeStrategy.GetNodeId(message.Receiving_Endpoint);
            if(receiverId != null)
            { 
                var receiverNode = _model.GetEndpoint(receiverId);
                receiverNode["Label"] = message.Receiving_Endpoint.Name;
            }

            var messageId = _nodeStrategy.GetNodeId(message);
            if(messageId != null)
            { 
                var messageNode = _model.GetMessage(messageId);
                messageNode["Intent"] = intent;
                if (!string.IsNullOrWhiteSpace(message.Message_Type))
                {
                    messageNode["Label"] = message.Message_Type.Split('.').Last();
                }
            }

            if (senderId != null && messageId != null)
            {
                var senderLink = _model.GetLink(senderId, messageId);
                senderLink["Intent"] = intent;
            }

            if (messageId != null && receiverId != null)
            {
                var receiverLink = _model.GetLink(messageId, receiverId);
                receiverLink["Intent"] = intent;
            }
        }

        public Model GetModel()
        {
            return _model;
        }
    }
}