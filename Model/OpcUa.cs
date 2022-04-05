using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Threading.Tasks;

namespace OpcUaTrendClient.Model
{
    public class OpcUa
    {
        ApplicationInstance application;
        ApplicationConfiguration configuration;

        public OpcUa()
        {
            application = new ApplicationInstance();

            application.ApplicationName = "UA Sample Client";
            application.ApplicationType = ApplicationType.Client;
            application.ConfigSectionName = "Opc.Ua.SampleClient";                               
        }
        public void Connect(string URI)
        {
            Log.That($"Attempt connect to {URI}");
            try
            {
                StartClient(URI).Wait();
            }
            catch (System.Exception e)
            {
                Log.That(e.ToString());
            }            
        }
        private async Task StartClient(string URI)
        {
            //throw new System.Exception("Test");
            Log.That("Load config");                        
            configuration = await application.LoadApplicationConfiguration(false);           


            Log.That("Create session");
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(URI, false, 15000);
            var endpointConfiguration = EndpointConfiguration.Create(configuration);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            Session session = await Session.Create(configuration, endpoint, false, "OPC UA Console Client", 60000, new UserIdentity(new AnonymousIdentityToken()), null);            
            Log.That(session.ReadValue(new NodeId("ns=3;s=\"DB_X\".\"X\"")).ToString());

        }               
    }
}
