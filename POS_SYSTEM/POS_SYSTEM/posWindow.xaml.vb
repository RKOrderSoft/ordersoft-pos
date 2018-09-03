Imports OrderSoft
Imports System.Windows.Threading

' list box stuff https://www.tutorialspoint.com/vb.net/vb.net_listbox.htm

Public Class basicOrder
    Public Property TableNumber As String
    Public Property TimeSubmitted As String
    Public Property TimeCompleted As String
End Class

Public Class posWindow
    Dim posClient As OSClient
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

        ' Orders listview setup

    End Sub

    ' Executes every 3 seconds
    Private Async Sub posTick()
        lvOrders.Items.Clear()
        ' Ask server for unpaid orders and updates the list view
        Dim openOrderIds = Await posClient.GetUnpaidOrders()
        For Each id As String In openOrderIds
            Dim newOrder = Await posClient.GetOrder(orderId:=id)
            Dim valuesToAdd As New basicOrder

            valuesToAdd.TableNumber = Convert.ToString(newOrder.TableNumber)
            valuesToAdd.TimeSubmitted = newOrder.TimeSubmittedString
            valuesToAdd.TimeCompleted = newOrder.TimeCompletedString
            lvOrders.Items.Add(valuesToAdd)
        Next
    End Sub

End Class
