using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btSMS
{
    class Constants
    {
        public static byte[] masUUID = { 0xBB, 0x58, 0x2B, 0x40, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08,
                             0x00, 0x20, 0x0C, 0x9A, 0x66 };
        public static byte[] mnsUUID = { 0xBB, 0x58, 0x2B, 0x41, 0x42, 0x0C, 0x11, 0xDB, 0xB0, 0xDE, 0x08,
                             0x00, 0x20, 0x0C, 0x9A, 0x66 };
        public static string sendTemplate =
@"BEGIN:BMSG
VERSION:1.0
STATUS:READ
TYPE:SMS_CDMA
FOLDER:telecom/msg/outbox
BEGIN:VCARD
VERSION:2.1
N:%toName
TEL:%fromNumber
END:VCARD
BEGIN:BENV
BEGIN:VCARD
VERSION:2.1
N:%fromName
TEL:%toNumber
END:VCARD
BEGIN:BBODY
CHARSET:UTF-8
LENGTH:0
BEGIN:MSG
%message
END:MSG
END:BBODY
END:BENV
END:BMSG";
    }
}
