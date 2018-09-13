Imports OrderSoft
Imports System.Windows.Threading
Imports System.Linq

' A class made for adding it to the list view
' Simplifies objects in list view to improve performance
Public Class basicOrder
    Public Property OrderId As String
    Public Property TableNumber As String
    Public Property TimeSubmitted As String
    Public Property TimeCompleted As String
End Class

Public Class posWindow
    Dim charactersAllowed As New List(Of Char) ' Characters allowed to be in the table number text box
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

        ' Clear ordersInListView that may still be left from previous session
        ordersInListView.Clear()

        ' Characters allowed to be added to the text box
        charactersAllowed.Add("0")
        charactersAllowed.Add("1")
        charactersAllowed.Add("2")
        charactersAllowed.Add("3")
        charactersAllowed.Add("4")
        charactersAllowed.Add("5")
        charactersAllowed.Add("6")
        charactersAllowed.Add("7")
        charactersAllowed.Add("8")
        charactersAllowed.Add("9")

        ' Timer setup
        posTimer = New DispatcherTimer
        posTimer.Interval = TimeSpan.FromMilliseconds(3000) ' Tick every 3 seconds
        AddHandler posTimer.Tick, AddressOf PosTick
        posTimer.Start() ' Start timer

        ' Initial update of the list view
        PosTick() ' Initial tick

        ' Make error message labels hidden
        lblErrorMsg.Visibility = Visibility.Hidden
        lblManualErrorMsg.Visibility = Visibility.Hidden

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

            ' Adding order details to the basicOrder that's going to be added to the listview
            valuesToAdd.OrderId = newOrder.OrderId
            valuesToAdd.TableNumber = Convert.ToString(newOrder.TableNumber)
            valuesToAdd.TimeSubmitted = newOrder.TimeSubmittedString
            valuesToAdd.TimeCompleted = newOrder.TimeCompletedString

            ' Checks if the order is already in the listview, if not then adds it to the listview
            If ordersInListView.Contains(valuesToAdd.OrderId) = False Then ' Not in listview yet
                lvOrders.Items.Add(valuesToAdd) ' Add to listview
                ordersInListView.Add(newOrder.OrderId) ' Add to list keeping track of which orders are already in the listview
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

    ' Close and logout button click
    Private Sub btnClose_Click(sender As Object, e As RoutedEventArgs) Handles btnClose.Click
        If MessageBox.Show("Are you sure you want to logout and close the program?", "My App", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then
            End
        End If
    End Sub

    ' Data validation for table number text box
    ' Makes sure only integers are able to be typed in the text box for table number
    Private Sub txtboxTableNum_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtboxTableNum.TextChanged
        Dim text As String = txtboxTableNum.Text
        Dim Letter As String
        Dim SelectionIndex As Integer = txtboxTableNum.SelectionStart
        Dim Change As Integer

        For x As Integer = 0 To txtboxTableNum.Text.Length - 1
            Letter = txtboxTableNum.Text.Substring(x, 1)
            If charactersAllowed.Contains(Letter) = False Then ' If the character entered is in the allowed characters array
                text = text.Replace(Letter, String.Empty)
                Change = 1
            End If
        Next

        txtboxTableNum.Text = text
    End Sub

    ' Handle help window opening button
    Private Sub btnHelp1_Click(sender As Object, e As RoutedEventArgs) Handles btnHelp1.Click
        Dim helpWindow As posWindowHelp = New posWindowHelp()
        helpWindow.Show()
    End Sub

    ' A list view sorting sub that theoretically should work but doesn't
    ' Handles sort button click
    'Private Sub btnSortLv_Click(sender As Object, e As RoutedEventArgs) Handles btnSortLv.Click
    '   Dim lvOrdersArr As New List(Of basicOrder)
    '   For Each order As basicOrder In lvOrders.Items
    '        lvOrdersArr.Add(order)
    '   Next
    '    lvOrders.Items.Clear() ' Clear listview before adding orders back into it
    'lvOrdersArr = lvOrdersArr.OrderBy(Function(order) order.TableNumber) ' Uses OrderBy to sort list before adding back to listview
    'For Each order As basicOrder In lvOrdersArr ' Loop through list
    '       lvOrders.Items.Add(order) ' Add to listview
    'Next
    'End Sub
End Class
