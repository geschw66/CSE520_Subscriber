using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Measurement

{
   class DriverMain
   {

      /// <summary>
      /// AWS IoT endpoint - replace with your own
      /// </summary>
      private const string IotEndpoint = "altjckff383r2.iot.us-west-2.amazonaws.com";
      /// <summary>
      /// TLS1.2 port used by AWS IoT
      /// </summary>
      private const int BrokerPort = 8883;
      private const string TOPIC = "timeTest01";
      static void Main(string[] args)
      {
         DriverMain sub = new DriverMain();
         sub.Subscribe();

         //DriverMain pub = new DriverMain();
         //pub.Publish();

      }

      public void Subscribe()
      {
         Console.Write("SUBSCRIBING:");
         //convert to pfx using openssl.
         var clientCert = new X509Certificate2(@"cert stuff goes here", "pwd goes here.");

         //this is the AWS caroot.pem file.
         var caCert = X509Certificate.CreateFromSignedFile(@"C:\OutOfOffice\rootCA.pem");
         var client = new MqttClient(IotEndpoint, BrokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2 /*this is what AWS IoT uses*/);

         //event handler for inbound messages
         client.MqttMsgPublishReceived += ClientMqttMsgPublishReceived;

         //client id here is totally arbitary, but I'm pretty sure you can't have more than one client named the same.
         client.Connect("listener");

         // '#' is the wildcard to subscribe to anything under the 'root' topic
         // the QOS level here - I only partially understand why it has to be this level - it didn't seem to work at anything else.
         client.Subscribe(new[] { TOPIC }, new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

         while (true)
         {
            //Keeps it going....while listener listens....
         }

      }

      public void Publish()
      {
         //convert to pfx using openssl - see confluence
         //you'll need to add these two files to the project and copy them to the output (not included in source control deliberately!)
         var clientCert = new X509Certificate2(@"C:\OutOfOffice\OUTOFOFFICEPFXFILE.pfx", "9geoI1966");
         var caCert = X509Certificate.CreateFromSignedFile(@"C:\OutOfOffice\rootCA.pem");
         // create the client
         var client = new MqttClient(IotEndpoint, BrokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);
         //message to publish - could be anything
         var message = DateTime.UtcNow.Millisecond.ToString();

         //client naming has to be unique if there was more than one publisher
         client.Connect("windowsTest");
         //publish to the topic

         client.Publish(TOPIC, Encoding.UTF8.GetBytes(message));
         //this was in for debug purposes but it's useful to see something in the console
         if (client.IsConnected)
         {
            Console.WriteLine("SUCCESS!");
         }
         //wait so that we can see the outcome
         Console.ReadLine();
      }

      /// <summary>
      /// Listener. listens the published message.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public static void ClientMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
      {
        //For device to client webserver time: Use this for end to end test with Raspberry Pi.
        double recieve = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;


         ////Want absolutely get the recieved time first. use this for Subscriber to MQTT Round trip time.
         //double recieve = (DateTime.Now - DateTime.MinValue).TotalMilliseconds;
         
         ////Convert it to a double.
         double senttime = Double.Parse(Encoding.UTF8.GetString(e.Message));

         //Do the math:
         double diff = recieve - senttime;

         //Output:
         //Console.WriteLine("We received a message:");
         Console.WriteLine(recieve.ToString()+","+ senttime.ToString() +"," + diff.ToString());
      }
   }
}
