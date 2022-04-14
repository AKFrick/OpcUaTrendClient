using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Prism.Commands;
using Prism.Mvvm;
using Opc.Ua;

namespace OpcUaTrendClient.Model
{
    public class OpcUaNode : INotifyPropertyChanged
    {
        public OpcUaNode()
        {
            Browse = new DelegateCommand(() => browseNodesCommand());
            Read = new DelegateCommand(() => readNodeCommand());
        }
        public OpcUaNode(ObservableCollection<OpcUaNode> nodes, string name) : this()
        {
            Nodes = nodes;            
            Name = name;
        }
        public string Name { get; set; }
        public NodeId Id { get; set; }
        public string Value { get; set; }
        public ObservableCollection<OpcUaNode> Nodes { get { return _nodes; }
            set {
                _nodes = value; OnPropChng(nameof(Nodes));
                foreach (var node in _nodes)
                {
                    node.ReadNode += (a) => ReadNode(a);
                    node.BrowseNodes += (a) =>BrowseNodes(a);
                }
            } }
        private ObservableCollection<OpcUaNode> _nodes;

        public Action<NodeId> ReadNode;
        public Action<OpcUaNode> BrowseNodes;

        public bool IsNodeExpanded { get; set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;
        public OpcUaNode ParentNode { get; set; }
        public DelegateCommand Browse { get; private set; }
        public DelegateCommand Read { get; private set; }
        private void readNodeCommand()
        {
            ReadNode?.Invoke(Id);
        }
        private void browseNodesCommand()
        {
            BrowseNodes?.Invoke(this);
        }
        private void OnPropChng(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }        
    }
}
  