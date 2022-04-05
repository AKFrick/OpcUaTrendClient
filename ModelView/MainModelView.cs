using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using OpcUaTrendClient.Model;
using System.Collections.ObjectModel;

namespace OpcUaTrendClient.ModelView
{
    internal class MainModelView : BindableBase
    {        
        public MainModelView()
        {
            opc = new OpcUa();
            Connect = new DelegateCommand(() => opc.Connect(Endpoint));
        }
        public Log MyLog => Log.GetInstance();
        public string Endpoint { get; set; }
        public DelegateCommand Connect { get; private set; }        
        private OpcUa opc;
    }
}

