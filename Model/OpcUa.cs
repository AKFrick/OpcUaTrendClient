using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace OpcUaTrendClient.Model
{
    public class OpcUa
    {
        ApplicationInstance application;
        ApplicationConfiguration configuration;
        Browser browser;
        Session session;
        public ObservableCollection<OpcUaNode> Nodes { get; private set; }

        private static OpcUa instance;
        public static OpcUa GetInstance()
        {
            if (instance == null)
                instance = new OpcUa();
            return instance;
        }
        private OpcUa()
        {
            application = new ApplicationInstance();

            application.ApplicationName = "UA Sample Client";
            application.ApplicationType = ApplicationType.Client;
            application.ConfigSectionName = "Opc.Ua.SampleClient";
            Nodes = new ObservableCollection<OpcUaNode>();
        }
        public async Task Connect(string URI)
        {
            Log.That($"Attempt connect to {URI}");
            try
            {
                await StartClient(URI);
            }
            catch (System.Exception e)
            {
                Log.That(e.ToString());
            }
        }
        private async Task StartClient(string URI)
        {
            Log.That("Load config");
            configuration = await application.LoadApplicationConfiguration(false);


            Log.That("Create session");
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(URI, false, 15000);
            var endpointConfiguration = EndpointConfiguration.Create(configuration);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            session = await Session.Create(configuration, endpoint, false, "OPC UA Console Client", 60000, new UserIdentity(new AnonymousIdentityToken()), null);
            Log.That(session.ReadValue(new NodeId("ns=3;s=\"DB_X\".\"X\"")).ToString());

        }
        public async Task<DataValue> ReadNode(NodeId nodeId)
        {
            return session.ReadValue(nodeId);
        }
        public async Task<ObservableCollection<OpcUaNode>> Browse(NodeId nodeId)
        {


            ReferenceDescriptionCollection references;
            browser = new Browser(session);
            browser.BrowseDirection = BrowseDirection.Forward;
            browser.ContinueUntilDone = true;
            browser.NodeClassMask = (int)NodeClass.Variable | (int)NodeClass.Object | (int)NodeClass.Method;
            references = browser.Browse(nodeId);

            ObservableCollection<OpcUaNode> nodes = new ObservableCollection<OpcUaNode>();

            foreach (var rd in references)
            {
                nodes.Add(new OpcUaNode { Name = $"{rd.DisplayName} + {rd.NodeClass}", Id = ExpandedNodeId.ToNodeId(rd.NodeId, session.NamespaceUris) });
            }
            return nodes;
        }
        Subscription subscription;
        public void Subscribe(NodeId node, Action<DataValue> action)
        {
            //Log.That("Create sub");
            ValueUpdated += action;
            subscription = new Subscription(session.DefaultSubscription);

            var list = new List<MonitoredItem> {
                new MonitoredItem(subscription.DefaultItem) { DisplayName = "Speed", StartNodeId = node},
            };

            list.ForEach(i => i.Notification += OnNotification);
            subscription.AddItems(list);
            //list.ForEach(i => Log.That(i.Subscription.Id.ToString()));
            subscription.PublishingEnabled = true;

            session.AddSubscription(subscription);
            subscription.Create();
        }

        public Action<DataValue> ValueUpdated;
        private static void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in item.DequeueValues())
            {
                Log.That($"NewValue: {value.Value}");
            }
        }

    }
}
