using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Bluetooth.AttributeIds;
using Brecham.Obex;
using Brecham.Obex.Objects;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var address = "04FE3167133D";
            BluetoothAddress addr = BluetoothAddress.Parse(address);
            var endpoint = new BluetoothEndPoint(addr, BluetoothService.MessageAccessProfile, 16);
            var bdi = new BluetoothDeviceInfo(addr);
            var records = bdi.GetServiceRecords(BluetoothService.MessageAccessProfile);
            foreach (ServiceRecord record in records)
            {
                var port = ServiceRecordHelper.GetRfcommChannelNumber(record);
                var name = record.GetPrimaryMultiLanguageStringAttributeById(UniversalAttributeId.ServiceName);
                Console.WriteLine("Name: " + name + "; Port: " + port);
            }
            var cli = new BluetoothClient();
            cli.Encrypt = true;
            cli.Connect(endpoint);
            var session = new ObexClientSession(cli.GetStream(), 65535);
            //session.Connect(ObexConstant.Target.FolderBrowsing);


            Console.ReadLine();
        }
    }
}
