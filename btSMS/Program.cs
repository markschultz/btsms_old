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
            var endpoint = new BluetoothEndPoint(addr, BluetoothService.MessageAccessProfile, 16);
            var bdi = new BluetoothDeviceInfo(addr);
            var records = bdi.GetServiceRecords(BluetoothService.MessageAccessProfile);
            foreach (ServiceRecord record in records)
            {
                var port = ServiceRecordHelper.GetRfcommChannelNumber(record);
                var name = record.GetPrimaryMultiLanguageStringAttributeById(UniversalAttributeId.ServiceName);
                Console.WriteLine("Name: " + name + "; Port: " + port);
            }
            using (var cli = new BluetoothClient())
            {
                cli.Encrypt = true;
                cli.Connect(endpoint);
                Console.WriteLine("Connected: " + cli.Connected + " Port: " + cli.RemoteEndPoint.Port);
                using (var session = new ObexClientSession(cli.GetStream(), 65535))
                {
                    try
                    {
                        session.Connect(Constants.masUUID);
                        Console.WriteLine("Session Connected: " + session.ConnectionId);
                        Console.WriteLine(session.GetFolderListing().AllItems.Aggregate("Current Folder Listing:\n\t", (q, a) => q + a.ToString() + "\n\t"));
                        session.SetPath("telecom");
                        Console.WriteLine(session.GetFolderListing().AllItems.Aggregate("Current Folder Listing:\n\t", (q, a) => q + a.ToString() + "\n\t"));
                        session.SetPath("msg");
                        Console.WriteLine(session.GetFolderListing().AllItems.Aggregate("Current Folder Listing:\n\t", (q, a) => q + a.ToString() + "\n\t"));
                        //GetMessage(session, ba);
                        //GetNumMessages(session, ba);
                        PutMessage(session, Constants.sendTemplate.Replace("%message", "test test").Replace("%toNumber", "3124361855"));
                    }
                    catch (ObexResponseException obexRspEx)
                    {
                        if (obexRspEx.ResponseCode == Brecham.Obex.Pdus.ObexResponseCode.PeerUnsupportedService)
                        {
                            Console.WriteLine("The OBEX Server does not support the requested Target service/application.");
                        }
                        else
                        {
                            Console.WriteLine("The OBEX Server rejected our connection: " + obexRspEx.ResponseCode);
                            if (obexRspEx.Description != null)
                            {
                                Console.WriteLine("    Reason: " + obexRspEx.Description);
                            }
                        }
                    }
                }
            }
            Console.WriteLine("\nDONE.");
            Console.ReadLine();
        }

        private static ObexClientSession initSession(BluetoothAddress addr)
        {
        }

        private static void PutMessage(ObexClientSession session, string body)
        {
            byte[] ba = new byte[1024 * 4];
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
            byte[] ba = new byte[1024 * 4];
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
            byte[] ba = new byte[1024 * 4];
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

        private static void SubscribeNotifications(ObexClientSession session)
        {
        }
    }
}
