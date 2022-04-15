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
            #region LOG
            LogItems = new ObservableCollection<Log.LogItem>(Log.GetInstance().LogItems);
            ((INotifyCollectionChanged)Log.GetInstance().LogItems).CollectionChanged += (s, a) =>
            {
                if (a.NewItems?.Count >= 1)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (Log.LogItem item in a.NewItems)
                            LogItems.Insert(0, item);
                    }));
                if (a.OldItems?.Count >= 1)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (Log.LogItem item in a.OldItems)
                            LogItems.Remove(item);
                    }));
            };
            #endregion

            Connect = new DelegateCommand(() => connect().Wait());
            ReadSelectedNode = new DelegateCommand(() => ReadNode(SelectedNode));

            SubscribeSelectedNode = new DelegateCommand(() => SubscribeNode(SelectedNode)  );
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
        public async void ReadNode(OpcUaNode node)
        {
            Log.That($"Reading {node.Id}...");
            SelectedNode = node;
            node.UpdateValue( await OpcUa.GetInstance().ReadNode(node.Id));
            Log.That(node.Value.ToString());
            RaisePropertyChanged(nameof(SelectedNode));
        }
        public async void BrowseNodes(OpcUaNode node)
        {
            Log.That($"{node.Name} browsing {node.Id}");
            node.Nodes = new ObservableCollection<OpcUaNode>(await OpcUa.GetInstance().Browse(node.Id));
        }
        public void SubscribeNode(OpcUaNode node)
        {
            OpcUa.GetInstance().Subscribe(node);
            node.PropertyChanged += ( (s, e) => RaisePropertyChanged(nameof(SelectedNode)));
        }
        public DelegateCommand ReadSelectedNode { get; private set; }
        public DelegateCommand SubscribeSelectedNode { get; private set; }
    }
}
