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
                        byte[] ba = new byte[1024 * 4];
                        //GetSingle(session, ba);
                        //GetListing(session, ba);
                        SendMessage(session, ba, "test test", "3124361855");
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

        private static void SendMessage(ObexClientSession session, byte[] ba, string message, string to)
        {
            var headers = new ObexHeaderCollection();
            headers.AddType("x-bt/message");
            headers.Add(ObexHeaderId.Name, "outbox");
            byte[] appParams = { 0x14, 0x01, 0x01};
            headers.Add(ObexHeaderId.AppParameters, appParams);
            headers.Dump(Console.Out);
            UTF8Encoding utf = new UTF8Encoding();
            var put = session.Put(headers);
            ba = utf.GetBytes(Constants.sendTemplate.Replace("%message", message).Replace("%toNumber", to));
            put.Write(ba, 0, ba.Length);
            put.Close();
        }

        private static void GetListing(ObexClientSession session, byte[] ba)
        {
            int bytesRead;
            var headers = new ObexHeaderCollection();
            headers.AddType("x-bt/MAP-msg-listing");
            headers.Add(ObexHeaderId.Name, "inbox");
            byte[] appParams = { 0x01, 0x02, 0x00, 0x02, 0x10, 0x04, 0x00, 0x00, 0x96, 0x8f };
            headers.Add(ObexHeaderId.AppParameters, appParams);
            headers.Dump(Console.Out);
            var get = session.Get(headers);
            get.ResponseHeaders.Dump(Console.Out);
            System.IO.FileStream fs = new System.IO.FileStream("D:\\inbox_test.txt", System.IO.FileMode.Create, System.IO.FileAccess.Write);
            bytesRead = get.Read(ba, 0, ba.Length);
            fs.Write(ba, 0, bytesRead);
            fs.Close();
            var app = get.ResponseHeaders.GetByteSeq(ObexHeaderId.AppParameters);
            get.Close();
        }

        private static void GetSingle(ObexClientSession session, byte[] ba)
        {
            var get2 = session.Get("9905", "x-bt/message");
            get2.ResponseHeaders.Dump(Console.Out);
            System.IO.FileStream fs2 = new System.IO.FileStream("D:\\single_test.txt", System.IO.FileMode.Create, System.IO.FileAccess.Write);
            var bytesRead = get2.Read(ba, 0, ba.Length);
            fs2.Write(ba, 0, bytesRead);
            fs2.Close();
            get2.Close();
        }
    }
}
