Imports OrderSoft
Imports System.Windows.Threading

' list box stuff https://www.tutorialspoint.com/vb.net/vb.net_listbox.htm

Public Class posWindow
    Dim posClient As New OSClient()
    Private posTimer As DispatcherTimer
    Public Sub New(client)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' Passing through client from loginWindow
        posClient = client

        ' Timer setup
        posTimer = New DispatcherTimer
        posTimer.Interval = TimeSpan.FromMilliseconds(3000)
        AddHandler posTimer.Tick, AddressOf posTick
        posTimer.Start()
    End Sub

    ' Executes every 3 seconds
    Private Sub posTick()
        ' Ask server for unpaid orders and updates the listbox
        System.Console.WriteLine("test")
    End Sub

End Class
