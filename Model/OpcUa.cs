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
        SessionReconnectHandler reconnectHandler;
        const int ReconnectPeriod = 10;
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
            session.KeepAlive += Session_KeepAlive;
            Log.That(session.ReadValue(new NodeId("ns=3;s=\"DB_X\".\"X\"")).ToString());

        }

        private void Session_KeepAlive(Session session, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                Log.That("{e.Status}, {session.OutstandingRequestCount}, {session.DefunctRequestCount)}");

                if (reconnectHandler == null)
                {
                    Console.WriteLine("--- RECONNECTING ---");
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(session, ReconnectPeriod * 1000, Client_ReconnectComplete);

                }
            }
        }
        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, reconnectHandler))
            {
                return;
            }
            session = reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;

            session.AddSubscription(subscription);

            Log.That("--- RECONNECTED ---");
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
            browser.NodeClassMask = 0;// (int)NodeClass.Variable | (int)NodeClass.Object | (int)NodeClass.Method | (int)NodeClass.DataType | (int)NodeClass.VariableType | (int)NodeClass.ReferenceType;
            references = browser.Browse(nodeId);

            ObservableCollection<OpcUaNode> nodes = new ObservableCollection<OpcUaNode>();

            foreach (var rd in references)
            {
                nodes.Add(new OpcUaNode { Name = $"{rd.DisplayName} + {rd.NodeClass}", Id = ExpandedNodeId.ToNodeId(rd.NodeId, session.NamespaceUris) });
            }
            return nodes;
        }

        Subscription subscription;
        public void Subscribe(OpcUaNode node)
        {
            Log.That($"Subscribed to {node.Id}");
            ValueUpdated += node.UpdateValue;
            subscription = new Subscription(session.DefaultSubscription);

            var list = new List<MonitoredItem> {
                new MonitoredItem(subscription.DefaultItem) { StartNodeId = node.Id},
            };

            list.ForEach(i => i.Notification += OnNotification);
            subscription.AddItems(list);
            //list.ForEach(i => Log.That(i.Subscription.Id.ToString()));
            subscription.PublishingEnabled = true;

            session.AddSubscription(subscription);
            subscription.Create();
        }

        public Action<DataValue> ValueUpdated;
        private void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in item.DequeueValues())
            {
                Log.That($"NewValue: {value.Value}");
                ValueUpdated(value);
            }

        }
        public void CallMethod()
        {
            
        }
    }
}
