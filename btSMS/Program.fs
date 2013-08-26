open System
open System.Windows
open System.Linq
open System.Windows.Controls
open InTheHand.Windows.Forms
open InTheHand.Net.Sockets
open InTheHand.Net
open InTheHand.Net.Bluetooth
open InTheHand.Net.Bluetooth.AttributeIds
open Brecham.Obex
open Brecham.Obex.Objects

type MainWindow = FSharpx.XAML<"Window.xaml">

let loadWindow() =
    let window = MainWindow()
    window.Button1.Click.Add(fun _ -> 
        let dlg = new SelectBluetoothDeviceDialog()
        let result = dlg.ShowDialog()
        let device = dlg.SelectedDevice
        let addr = device.DeviceAddress
        let endpoint = new BluetoothEndPoint(addr, BluetoothService.RFCommProtocol)
        //Array.fold (fun (i,j) -> ServiceRecordHelper.GetRfcommChannelNumber(j) |> (+) i) () (device.GetServiceRecords(BluetoothService.RFCommProtocol)) |> ignore
        
        window.Textbox1.Text = device.InstalledServices.ToString() |> ignore )
        //window.Textbox1.Text = bluetoothtest endpoint |> ignore )
    window.Root
 
[<STAThread>]
(new Application()).Run(loadWindow()) 
|> ignore