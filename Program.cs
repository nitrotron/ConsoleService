using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IO.Ports;
using System.Threading;

namespace ConsoleService
{
    [ServiceContract]
    public interface IHelloWorldService
    {
        [OperationContract]
        string SayHello(string name);

        [OperationContract]
        int getCount();

        [OperationContract]
        SerialPort getPort();
    }
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class HelloWorldService : IHelloWorldService
    {
        int count = 0;
        static SerialPort _Port;
        public string SayHello(string name)
        {
            count++;
            return string.Format("Hello, {0}, Count {1}", name,count);
        }

        public int getCount()
        {
            return count;
        }

        public SerialPort getPort()
        {
            return _Port;
        }

        public void setPort(SerialPort p)
        {
            _Port = p;
        }
    }

    class Program
    {
        SerialPort port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
        HelloWorldService hws = new HelloWorldService();
        public void readSerial()
        {
            StringBuilder response = new StringBuilder();
            while (true)
            {
                try
                {
                    response.Append(port.ReadLine());
                }
                catch
                {
                    //Close();
                    //return string.Empty;
                }
                if (response.ToString().Contains(";"))
                    Console.WriteLine(response.ToString());

            }
        }
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8080/hello");
            Program prog = new Program();
            prog.port.Open();

            //SerialPort port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            //HelloWorldService hws = new HelloWorldService();
            prog.hws.setPort(prog.port);

            Thread threadRec = new Thread(new ThreadStart(prog.readSerial));
            //threadRec.Start();

            // Create the ServiceHost.
            using (ServiceHost host = new ServiceHost(typeof(HelloWorldService), baseAddress))
            {
                // Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);
                

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();

                
                Console.WriteLine("The service is ready at {0}", baseAddress);
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }

            
        }
    }
}
