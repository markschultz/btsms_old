open System
open System.Windows
open System.Linq
open System.Text
open System.Windows.Controls
open InTheHand.Windows.Forms
open InTheHand.Net.Sockets
open InTheHand.Net
open InTheHand.Net.Bluetooth
open InTheHand.Net.Bluetooth.AttributeIds
open Brecham.Obex
open Brecham.Obex.Objects
open Strings

type MainWindow = FSharpx.XAML<"Window.xaml">

let fromHex (s:string) = 
    s
    |> Seq.windowed 2
    |> Seq.mapi (fun i j -> (i,j))
    |> Seq.filter (fun (i,j) -> i % 2=0)
    |> Seq.map (fun (_,j) ->
        Byte.Parse(new String(j), Globalization.NumberStyles.AllowHexSpecifier))
    |> Array.ofSeq

let sendMessage (session:ObexClientSession) message toNum = 
    let headers = new ObexHeaderCollection()
    headers.AddType "x-bt/message"
    headers.Add(ObexHeaderId.Name, "outbox")
    headers.Add(ObexHeaderId.AppParameters, (fromHex "140101"))
    headers.Dump Console.Out
    let utf = new UTF8Encoding()
    let put = session.Put headers
    let ba = utf.GetBytes (sendTemplate.Replace("%message", message).Replace("%toNumber", toNum))
    put.Write(ba,0,ba.Length)
    put.Close

let loadWindow() =
    let window = MainWindow()
    window.Button1.Click.Add(fun _ -> 
        let address_m = "04FE3167133D";
        let addr = BluetoothAddress.Parse(address_m)
        let endpoint = new BluetoothEndPoint(addr, BluetoothService.MessageAccessProfile, 16)
        let bdi = new BluetoothDeviceInfo(addr)
        let records = bdi.GetServiceRecords(BluetoothService.MessageAccessProfile)
        let cli = new BluetoothClient()
        cli.Encrypt <- true
        cli.Connect(endpoint)
        printfn "Connected: %b Port: %d" cli.Connected cli.RemoteEndPoint.Port
        let session = new ObexClientSession(cli.GetStream(), 65535)
        let masUUID = fromHex "BB582B40420C11DBB0DE0800200C9A66"
        let mnsUUID = fromHex "BB582B41420C11DBB0DE0800200C9A66"
        session.Connect(masUUID)
        printfn "Session Connected: %d" session.ConnectionId
        session.SetPath("telecom/msg")
        //Array.fold (fun (i,j) -> ServiceRecordHelper.GetRfcommChannelNumber(j) |> (+) i) () (device.GetServiceRecords(BluetoothService.RFCommProtocol)) |> ignore

        
        window.Textbox1.Text = device.InstalledServices.ToString() |> ignore )
    window.Root
 
[<STAThread>]
(new Application()).Run(loadWindow()) 
|> ignore