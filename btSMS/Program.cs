using System;
using System.IO;
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

namespace btSMS
{
    class Program
    {
        static void Main(string[] args)
        {
            var address_m = "04FE3167133D";
            var address_r = "B0C4E7F92CCA";
            BluetoothAddress addr = BluetoothAddress.Parse(address_m);
            var bdi = new BluetoothDeviceInfo(addr);
            var records = bdi.GetServiceRecords(BluetoothService.RFCommProtocol);
            int mapPort = 0;
            foreach (ServiceRecord record in records)
            {
                var port = ServiceRecordHelper.GetRfcommChannelNumber(record);
                var name = record.GetPrimaryMultiLanguageStringAttributeById(UniversalAttributeId.ServiceName);
                Console.WriteLine("Name: " + name + "; Port: " + port);
                if (name.Contains("SMS"))
                {
                    Console.WriteLine(record.GetAttributeById((ServiceAttributeId)0x0315).Value.ElementTypeDescriptor.ToString());
                    mapPort = port;                    
                }
            }
            using (var connection = initConnection(addr, mapPort))
            {
                using (var session = initSession(connection))
                {
                    try
                    {
                        Console.WriteLine("Session Connected: " + session.ConnectionId);
                        session.SetPath("telecom");
                        session.SetPath("msg");
                        Console.WriteLine(session.GetFolderListing().AllItems.Aggregate("Current Folder Listing:\n\t", (q, a) => q + a.ToString() + "\n\t"));
                        //GetMessage(session, ba);
                        //GetNumMessages(session, ba);
                        //PushMessage(session, Constants.sendTemplate.Replace("%message", "test test").Replace("%toNumber", "3124361855"));
                        SubscribeNotifications(session, true);
                        SubscribeNotifications(session, false);
                    }
                    catch (ObexResponseException obexRspEx)
                    {
                        Console.WriteLine("The OBEX Server error: " + obexRspEx.ResponseCode);
                        if (obexRspEx.Description != null)
                        {
                            Console.WriteLine("    Reason: " + obexRspEx.Description);
                        }
                    }
                }
            }

            Console.WriteLine("\nDONE.");
            Console.ReadLine();
        }

        private static BluetoothClient initConnection(BluetoothAddress addr, int port)
        {
            var endpoint = new BluetoothEndPoint(addr, BluetoothService.MessageAccessProfile, port);
            var cli = new BluetoothClient();
            cli.Encrypt = true;
            cli.Connect(endpoint);
            Console.WriteLine("Connected: " + cli.Connected + " Port: " + cli.RemoteEndPoint.Port);
            return cli;
        }

        private static ObexClientSession initSession(BluetoothClient cli)
        {
            var session = new ObexClientSession(cli.GetStream(), 65535);
            session.Connect(Constants.masUUID);
            return session;
        }

        private static void PushMessage(ObexClientSession session, string body)
        {
            byte[] ba = new byte[1024 * 8];
            var headers = new ObexHeaderCollection();
            headers.AddType("x-bt/message");
            headers.Add(ObexHeaderId.Name, "outbox");
            byte[] appParams = { 0x14, 0x01, 0x01};
            headers.Add(ObexHeaderId.AppParameters, appParams);
            headers.Dump(Console.Out);
            UTF8Encoding utf = new UTF8Encoding();
            using (var put = session.Put(headers))
            {
                ba = utf.GetBytes(body);
                put.Write(ba, 0, ba.Length);
            }
        }

        private static void GetNumMessages(ObexClientSession session, int numMessages)
        {
            byte[] ba = new byte[1024 * 8];
            int bytesRead;
            var headers = new ObexHeaderCollection();
            headers.AddType("x-bt/MAP-msg-listing");
            headers.Add(ObexHeaderId.Name, "inbox");
            byte[] appParams = { 0x01, 0x02, 0x00, 0x02, 0x10, 0x04, 0x00, 0x00, 0x96, 0x8f };
            headers.Add(ObexHeaderId.AppParameters, appParams);
            headers.Dump(Console.Out);
            using (var get = session.Get(headers))
            {
                get.ResponseHeaders.Dump(Console.Out);
                using (FileStream fs = new FileStream("D:\\inbox_test.txt", FileMode.Create, FileAccess.Write))
                {
                    bytesRead = get.Read(ba, 0, ba.Length);
                    fs.Write(ba, 0, bytesRead);
                }
            }
        }

        private static void GetFilteredMessages(ObexClientSession session)
        {
        }

        private static void GetMessage(ObexClientSession session)
        {
            byte[] ba = new byte[1024 * 8];
            using (var get2 = session.Get("9905", "x-bt/message"))
            {
                get2.ResponseHeaders.Dump(Console.Out);
                using (FileStream fs2 = new FileStream("D:\\single_test.txt", FileMode.Create, FileAccess.Write))
                {
                    var bytesRead = get2.Read(ba, 0, ba.Length);
                    fs2.Write(ba, 0, bytesRead);
                }
            }
        }

        private static void SubscribeNotifications(ObexClientSession session, bool connect)
        {
            var headers = new ObexHeaderCollection();
            headers.AddType("x-bt/MAP-NotificationRegistration");
            byte[] appParams = { 0x0E, 0x01, (connect ? (byte)0x01 : (byte)0x00) };
            headers.Add(ObexHeaderId.AppParameters, appParams);
            headers.Dump(Console.Out);
            using (var put = session.Put(headers))
            {
                byte[] zero = {0x30};
                put.Write(zero, 0, zero.Length);
            }
        }
    }
}
