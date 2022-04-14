using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using OpcUaTrendClient.Model;
using System.Collections.ObjectModel;
using Opc.Ua;
using System.Windows;
using System.Collections.Specialized;

namespace OpcUaTrendClient.ModelView
{
    internal class MainModelView : BindableBase
    {        
        public MainModelView()
        {
            Connect = new DelegateCommand(() => connect().Wait());
            ReadSelectedNode = new DelegateCommand(() => ReadNode(selectedNodeId));

            SubscribeSelectedNode = new DelegateCommand(() => OpcUa.GetInstance().Subscribe(selectedNodeId, NodeValueUpdated)   );

            LogItems = new ObservableCollection<Log.LogItem>(Log.GetInstance().LogItems);
            ((INotifyCollectionChanged)Log.GetInstance().LogItems).CollectionChanged += (s, a) =>
            {
                if (a.NewItems?.Count >= 1)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (Log.LogItem item in a.NewItems)
                            LogItems.Add(item);
                    }));
                if (a.OldItems?.Count >= 1)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (Log.LogItem item in a.OldItems)
                            LogItems.Remove(item);
                    }));
            };

        }

        private async Task connect()
        {
            await OpcUa.GetInstance().Connect(Endpoint);
            RootNode = new OpcUaNode(await OpcUa.GetInstance().Browse(ObjectIds.ObjectsFolder), "Root");
            RootNode.ReadNode += ReadNode;
            RootNode.BrowseNodes += BrowseNodes;

            Nodes = RootNode.Nodes;
            RaisePropertyChanged(nameof(Nodes));
        }

        public ObservableCollection<Log.LogItem> LogItems { get; set; }
        public string Endpoint { get; set; } = "opc.tcp://10.10.10.98:4840/";
        public DelegateCommand Connect { get; private set; }
        public OpcUaNode RootNode { get; private set; }
        public ObservableCollection<OpcUaNode> Nodes { get; private set; }
        public OpcUaNode SelectedNode { get; set; }
        public async void ReadNode(NodeId Id)
        {
            Log.That($"Reading {Id.ToString()}...");
            selectedNodeId = Id;
            RaisePropertyChanged(nameof(SelectedNodeId));
            selectedNodeValue = await OpcUa.GetInstance().ReadNode(selectedNodeId);            
            RaisePropertyChanged(nameof(SelectedNodeValue));
        }
        public async void BrowseNodes(OpcUaNode node)
        {
            Log.That($"{node.Name} browsing {node.Id.ToString()}");
            node.Nodes = new ObservableCollection<OpcUaNode>(await OpcUa.GetInstance().Browse(node.Id));
        }

        public DelegateCommand ReadSelectedNode { get; private set; }
        public DelegateCommand SubscribeSelectedNode { get; private set; }
        public string SelectedNodeValue { get { return selectedNodeValue.ToString(); } private set { } }
        DataValue selectedNodeValue = new DataValue();
        public string SelectedNodeId { get { return selectedNodeId.ToString(); } private set { } }
        NodeId selectedNodeId = new NodeId("ns=3;s=\"DB_X\".\"X\"");

        public void NodeValueUpdated(DataValue value)
        {
            
        }

    }
}

