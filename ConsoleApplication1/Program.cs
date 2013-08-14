﻿using System;
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
            var cli = new BluetoothClient();
            cli.Encrypt = true;
            cli.Connect(endpoint);
            Console.WriteLine("Connected: " + cli.Connected + " Port: " + cli.RemoteEndPoint.Port);
            var session = new ObexClientSession(cli.GetStream(), 65535);
            try
            {
                //session.Connect(ObexConstant.Target.FolderBrowsing);
                //session.Connect();
                byte[] masUUID = { 0xBB, 0x58, 0x2B, 0x40, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08,
                                 0x00, 0x20, 0x0C, 0x9A, 0x66 };
                byte[] mnsUUID = { 0xBB, 0x58, 0x2B, 0x41, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08,
                                 0x00, 0x20, 0x0C, 0x9A, 0x66 };
                session.Connect(masUUID);
                Console.WriteLine("Session Connected: " + session.ConnectionId);
                Console.WriteLine(session.GetFolderListing().AllItems.Aggregate("Current Folder Listing:\n\t", (q, a) => q + a.ToString() + "\n\t"));
                session.SetPath("telecom");
                Console.WriteLine(session.GetFolderListing().AllItems.Aggregate("Current Folder Listing:\n\t", (q, a) => q + a.ToString() + "\n\t"));
                session.SetPath("msg");
                Console.WriteLine(session.GetFolderListing().AllItems.Aggregate("Current Folder Listing:\n\t", (q, a) => q + a.ToString() + "\n\t"));
                session.SetPath("inbox");
                var get2 = session.Get("9905", "x-bt/message");
                get2.ResponseHeaders.Dump(Console.Out);
                System.IO.FileStream fs2 = new System.IO.FileStream("D:\\single_test.txt", System.IO.FileMode.Create, System.IO.FileAccess.Write);
                byte[] ba = new byte[1024 * 4];
                get2.Read(ba, 0, ba.Length);
                fs2.Write(ba, 0, ba.Length);
                fs2.Close();
                get2.Close();
                var get = session.Get("", "x-bt/MAP-msg-listing");
                get.ResponseHeaders.Dump(Console.Out); //diagnostic header print
                System.IO.FileStream fs = new System.IO.FileStream("D:\\inbox_test.txt", System.IO.FileMode.Create, System.IO.FileAccess.Write);
                get.Read(ba, 0, ba.Length);
                fs.Write(ba, 0, ba.Length);
                fs.Close();
                var app = get.ResponseHeaders.GetByteSeq(ObexHeaderId.AppParameters);

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

            Console.ReadLine();
        }
    }
}
