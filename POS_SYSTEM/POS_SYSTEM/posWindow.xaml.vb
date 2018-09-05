Imports OrderSoft
Imports System.Windows.Threading

Public Class basicOrder
    Public Property OrderId As String
    Public Property TableNumber As String
    Public Property TimeSubmitted As String
    Public Property TimeCompleted As String
End Class

Public Class posWindow

    Dim ordersInListView As New List(Of String) ' A list of OrderIDs keeping track of what orders are already in the list view
    Dim selectedOrder As String ' OrderID of currently selected order in list view

    Dim posClient As OSClient
    Private posTimer As DispatcherTimer ' Timer to refresh unpaid orders every tick


    Public Sub New(client)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ' Passing through client from loginWindow
        posClient = client

        ' Timer setup
        posTimer = New DispatcherTimer
        posTimer.Interval = TimeSpan.FromMilliseconds(3000) ' Tick every 3 seconds
        AddHandler posTimer.Tick, AddressOf PosTick
        posTimer.Start()

        ' Initial update of the list view
        PosTick() ' Initial tick
    End Sub

    ' A timer that requests from the server what orders are currently unpaid
    ' Populates the list view with the unpaid orders
    Private Async Sub PosTick()
        ' Ask server for unpaid orders
        Dim unpaidOrderIds = Await posClient.GetUnpaidOrders()

        ' Loop through all unpaid orders
        For Each id As String In unpaidOrderIds
            Dim newOrder = Await posClient.GetOrder(orderId:=id)
            Dim valuesToAdd As New basicOrder

            valuesToAdd.OrderId = newOrder.OrderId
            valuesToAdd.TableNumber = Convert.ToString(newOrder.TableNumber)
            valuesToAdd.TimeSubmitted = newOrder.TimeSubmittedString
            valuesToAdd.TimeCompleted = newOrder.TimeCompletedString

            If ordersInListView.Contains(valuesToAdd.OrderId) = False Then
                lvOrders.Items.Add(valuesToAdd)
                ordersInListView.Add(newOrder.OrderId)
            End If

        Next

        ' Loop through all items in list view and check if they are still unpaid

    End Sub

    ' Item selected in list view
    Private Sub lvOrders_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lvOrders.SelectionChanged
        selectedOrder = lvOrders.SelectedItem.OrderId ' selectedOrder is set to the orderId selected
    End Sub

    ' Pay button clicked
    Private Sub btnPay_Click(sender As Object, e As RoutedEventArgs) Handles btnPay.Click
        Dim nextWindow As orderInfoWindow = New orderInfoWindow(posClient, selectedOrder)
        nextWindow.Show()
        Me.Close()
    End Sub

    ' Logout button click
    Private Sub btnLogout_Click(sender As Object, e As RoutedEventArgs) Handles btnLogout.Click
        If MessageBox.Show("Are you sure you want to log out?", "My App", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then
            End
        End If
    End Sub
End Class
