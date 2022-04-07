﻿using System;
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

namespace OpcUaTrendClient.ModelView
{
    internal class MainModelView : BindableBase
    {        
        public MainModelView()
        {
            Connect = new DelegateCommand(() => connect().Wait());
        }

        private async Task connect()
        {
            await OpcUa.GetInstance().Connect(Endpoint);
            Nodes = new ObservableCollection<OpcUaNode>(await OpcUa.GetInstance().Browse(ObjectIds.ObjectsFolder));            
            RaisePropertyChanged(nameof(Nodes));
        }

        public Log MyLog => Log.GetInstance();
        public string Endpoint { get; set; } = "opc.tcp://10.10.10.98:4840/";
        public DelegateCommand Connect { get; private set; }
        public ObservableCollection<OpcUaNode> Nodes { get; private set; }
        public OpcUaNode SelectedNode { get; set; }

    }
}

