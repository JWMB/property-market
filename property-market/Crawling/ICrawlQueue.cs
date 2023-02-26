using Parsers.Models;
using System.Reflection;

namespace Crawling
{
    public interface ICrawlQueue : ICrawlQueueSender
    {
        Task Send(PropertyListing listing);
        Task<PropertyListing?> Receive();
        Task Delete(PropertyListing listing);
        Task<int> ApproximateMessageCount { get; }
    }

    public interface ICrawlQueueSender
    {
        Task Send(PropertyListing listing);
    }


    public class InMemoryCrawlQueue : ICrawlQueue
    {
        public class QueueItem<T>
        {
            public required T Item { get; set; }
            public DateTimeOffset? Received { get; set; }
        }

        //public Queue<PropertyListing> Queue { get; }
        private List<QueueItem<PropertyListing>> queue = new();

        public Task<int> ApproximateMessageCount => Task.FromResult(queue.Count);

        public Task Send(PropertyListing listing)
        {
            queue.Add(new QueueItem<PropertyListing> { Item = listing });
            return Task.CompletedTask;
        }

        public Task<PropertyListing?> Receive()
        {
            var item = queue.FirstOrDefault(o => o.Received == null);
            if (item != null)
                item.Received = DateTimeOffset.UtcNow;
            return Task.FromResult(item?.Item);
        }

        public Task Delete(PropertyListing listing)
        {
            queue.RemoveAll(o => o.Item == listing);
            return Task.CompletedTask;
        }
    }
}
