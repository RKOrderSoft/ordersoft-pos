Imports OrderSoft
Imports System.Windows.Threading
Imports System.Linq

Public Class basicOrder
    Public Property OrderId As String
    Public Property TableNumber As String
    Public Property TimeSubmitted As String
    Public Property TimeCompleted As String
End Class

Public Class posWindow

    Dim ordersInListView As New List(Of String) ' A list of OrderIDs keeping track of what orders are already in the list view
    Dim selectedOrder As String = "" ' OrderID of currently selected order in list view

    Dim posClient As OSClient ' This window's client
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

        ' Clear selectedOrder on window initialisation to prevent fetching an order that has already been paid
        selectedOrder = ""
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

            ' Checks if the order is already in the listview, if not then adds it to the listview
            If ordersInListView.Contains(valuesToAdd.OrderId) = False Then ' Not in listview yet
                lvOrders.Items.Add(valuesToAdd)
                ordersInListView.Add(newOrder.OrderId)
            End If

        Next

        ' Loop through all items in list view and check if they are still unpaid

    End Sub

    ' Item selected in list view
    Private Sub lvOrders_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lvOrders.SelectionChanged
        selectedOrder = lvOrders.SelectedItem.OrderId ' selectedOrder is set to the orderId of the order selected
    End Sub

    ' Pay button clicked
    Private Sub btnPay_Click(sender As Object, e As RoutedEventArgs) Handles btnPay.Click
        If selectedOrder = "" Then ' If there is no order selected
            ' Show error message
            lblErrorMsg.Visibility = Visibility.Visible
        Else ' If there is an order selected, open the orderInfoWindow
            Dim nextWindow As orderInfoWindow = New orderInfoWindow(posClient, selectedOrder) ' Passing in client and selected order
            nextWindow.Show()
            Me.Close()
        End If
    End Sub

    ' Logout button click
    Private Sub btnLogout_Click(sender As Object, e As RoutedEventArgs) Handles btnLogout.Click
        If MessageBox.Show("Are you sure you want to log out?", "My App", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then
            End
        End If
    End Sub

    ' Handles button to open help window
    Private Sub btnHelp_Click(sender As Object, e As RoutedEventArgs)
        Dim helpWindow As posWindowHelp = New posWindowHelp()
        helpWindow.Show()
    End Sub

    ' Handles OPEN button that opens up the order manually entered by table number on the bottom left of the window
    Private Async Sub btnOpenTableNumber_Click() Handles btnOpenTableNumber.Click
        If txtboxTableNum.Text = "" Then ' If the text box for manual table number entry is empty
            ' Show error message
            lblManualErrorMsg.Visibility = Visibility.Visible
        Else ' Text box is not empty, open the orderInfoWindow
            ' Search for order ID using the table number given
            Try ' Try requesting an order using table number given
                Dim manualOrder = Await posClient.GetOrder(tableNum:=Convert.ToInt32(txtboxTableNum.Text))
                Dim nextWindow As orderInfoWindow = New orderInfoWindow(posClient, manualOrder.OrderId) ' Passing in client and order manually entered by table number
                nextWindow.Show() ' Show next window
                Me.Close()
            Catch ex As Exception ' Invalid table number given
                ' Show error message
                lblManualErrorMsg.Visibility = Visibility.Visible
            End Try
        End If
    End Sub

    ' Handles enter button being pressed down 
    Private Sub txtboxTableNum_KeyDown(sender As Object, e As KeyEventArgs) Handles txtboxTableNum.KeyDown
        If e.Key = Key.Return Then ' If key pressed in enter then
            btnOpenTableNumber_Click() ' Try open order by table number
        End If
    End Sub

    Private Sub btnSortLv_Click(sender As Object, e As RoutedEventArgs) Handles btnSortLv.Click
        Dim lvOrdersArr As New List(Of basicOrder)
        For Each order As basicOrder In lvOrders.Items
            lvOrdersArr.Add(order)
        Next
        lvOrders.Items.Clear()
        'lvOrdersArr = lvOrdersArr.OrderBy(Function(order) order.TableNumber)
        For Each order As basicOrder In lvOrdersArr
            lvOrders.Items.Add(order)
        Next
    End Sub
End Class
