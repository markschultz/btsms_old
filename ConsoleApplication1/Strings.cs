using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Strings
    {
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
