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
            Browse = new DelegateCommand(   () => BrowseNodes().Wait() );
            Read = new DelegateCommand( () => ReadNode().Wait() );
        }
        public string Name { get; set; }
        public NodeId Id { get; set; }
        public string Value { get; set; }
        public ObservableCollection<OpcUaNode> Nodes { get { return _nodes; } set { _nodes = value; OnPropChng(nameof(Nodes)); } }
        private ObservableCollection<OpcUaNode> _nodes;

        public bool IsNodeExpanded { get; set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand Browse { get; private set; }
        public DelegateCommand Read { get; private set; }
        private async Task ReadNode()
        {
            Value = await OpcUa.GetInstance().ReadNode(Id);
        }
        private async Task BrowseNodes()
        {
            Log.That($"{Name} browsing {Id.ToString()}");
            Nodes = new ObservableCollection<OpcUaNode>( await OpcUa.GetInstance().Browse(Id)   );

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
  