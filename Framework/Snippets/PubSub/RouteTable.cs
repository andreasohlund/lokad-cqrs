using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace Snippets.PubSub
{
    [DataContract]
    public sealed class RouteTable
    {
        [DataContract]
        public sealed class RouteItem
        {
            [DataMember]
            public string ContractRegex { get; set; }
            [DataMember]
            public HashSet<string> QueueNames { get; set; }

            public RouteItem()
            {
                QueueNames = new HashSet<string>();
            }
        }

        [DataMember]
        public List<RouteItem> Items { get; private set; }

        public void Subscribe(string regex, string queueName)
        {
            var item = Items.FirstOrDefault(r => string.Equals(regex,r.ContractRegex));
            if (null == item)
            {
                item = new RouteItem()
                    {
                       ContractRegex = regex
                    };
                Items.Add(item);
            }
            
            item.QueueNames.Add(queueName);
        }
        public void Unsubscribe(string regex, string queueName)
        {
            var item = Items.FirstOrDefault(r => string.Equals(regex, r.ContractRegex));
            if (null == item)
            {
                return;
            }
            item.QueueNames.Remove(queueName);
            if (item.QueueNames.Count == 0)
            {
                Items.Remove(item);
            }
        }

        public RouteTable()
        {
            Items = new List<RouteItem>();
        }
    }
}