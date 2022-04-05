using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcUaTrendClient.Model
{
    public class Log
    {
        private static Log instance;

        public static Log GetInstance()
        {
            if(instance == null)
                instance = new Log();
            return instance;
        }
        private Log()
        {
            logItems = new ObservableCollection<LogItem>();
            LogItems = new ReadOnlyObservableCollection<LogItem>(logItems);
        }
        public class LogItem
        {
            public LogItem(string message)
            {
                TimeStamp = DateTime.Now;
                Message = message;                
            }
            public DateTime TimeStamp { get; private set; }
            public string Message { get; private set; }
        }
        private ObservableCollection<LogItem> logItems;
        public ReadOnlyObservableCollection<LogItem> LogItems { get; private set; }
        public static void That(string message)
        {
            GetInstance().logItems.Add(new LogItem(message));
        }        
    }    
}
