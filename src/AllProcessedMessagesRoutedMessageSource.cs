using System;
using Raven.Client;

namespace RoutingVisualization
{
    class AllProcessedMessagesRoutedMessageSource
    {
        private readonly IDocumentStore _store;

        public AllProcessedMessagesRoutedMessageSource(IDocumentStore store)
        {
            _store = store;
        }

        public void RegisterListener(Action<ProcessedMessage> onNext)
        {
            using (var session = _store.OpenSession())
            using (var stream = session.Advanced.Stream<ProcessedMessage>("ProcessedMessage"))
            {
                var count = 0;
                bool loop = true;
                Console.CancelKeyPress += (k, e) =>
                {
                    e.Cancel = true;
                    loop = false;
                };
                while (stream.MoveNext() && loop)
                {
                    Console.Write($"\rMessage #{count++}");
                    onNext(stream.Current.Document);
                }
                Console.WriteLine();
            }
        }
    }
}