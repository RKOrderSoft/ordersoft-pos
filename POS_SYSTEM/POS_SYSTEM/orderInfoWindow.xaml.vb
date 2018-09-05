Imports OrderSoft
Imports System.Windows.Documents

' A dish object made for this window to simplify things
Public Class POSDish
    Public Property dishId As Integer
    Public Property name As String
    Public Property size As String
    Public Property upgradePrice As Single
    Public Property totalPrice As Single
End Class

' An order object made for this window to simplify things
Public Class POSOrder
    Public Property origOrder As OrderObject
    Public Property orderId As String
    Public Property tableNumber As Integer
    Public Property serverID As String
    Public Property notes As String

    Public Property dishIds As List(Of POSDish)

    Public Property subtotal As Single
    Public Property tax As Single
    Public Property orderTotal As Single
    Public Property amtPaid As Single
End Class

Public Class Printer


End Class

Public Class orderInfoWindow
    Dim charactersAllowed As New List(Of Char) ' Characters allow to be in the amount paid text box

    Dim orderClient As OSClient ' Client for this window
    Dim orderInfo As New POSOrder ' Object representing the order opened in this window from last window

    Public Sub New(client, order)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' Passing in client
        orderClient = client

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
        charactersAllowed.Add(".")

        initialiseWindow(order) ' Initialises the window with information from the order selected on last window
    End Sub

    ' Calculates and adds all required info in the window
    Public Async Sub initialiseWindow(order)
        Dim originalOrder = Await orderClient.GetOrder(order)

        ' 
        orderInfo.origOrder = originalOrder
        orderInfo.orderId = order
        orderInfo.tableNumber = originalOrder.TableNumber
        orderInfo.serverID = originalOrder.ServerId
        orderInfo.notes = originalOrder.Notes

        ' Take string of dishes and convert to a list
        orderInfo.dishIds = Await parseDishes(originalOrder.Dishes)

        ' Calculate total and tax
        For Each dish As POSDish In orderInfo.dishIds
            orderInfo.orderTotal = orderInfo.orderTotal + dish.totalPrice
        Next
        orderInfo.tax = orderInfo.orderTotal / 11

        ' Displaying information
        lblOrderId.Content = "Order ID: " + orderInfo.orderId
        lblServerID.Content = "Server ID: " + orderInfo.serverID
        lblTableNumber.Content = "Table Number: " + orderInfo.tableNumber.ToString()
        lblNotes.Content = "Notes: " + orderInfo.notes
        lblTax.Content = "Tax: " + orderInfo.tax.ToString("C2")
        lblSubtotal.Content = "Subtotal: " + orderInfo.orderTotal.ToString("C2")
        lblTotal.Content = "Total: " + orderInfo.orderTotal.ToString("C2")

        ' Populate list box with dishes
        For Each dish As POSDish In orderInfo.dishIds
            lvDishes.Items.Add(dish)
        Next
    End Sub

    ' takes array of dishes and converts it into a list of POSDish
    Public Async Function parseDishes(Dishes) As Task(Of List(Of POSDish))
        Dim posDishes As New List(Of POSDish)

        For Each d As String In Dishes
            Dim posDish As New POSDish
            posDish.dishId = Split(d, "/")(0) ' extracts ID
            Dim dishInfo = Await orderClient.GetDishes(dishId:=posDish.dishId)

            If Split(d, "/").Length = 2 Then ' if the dish has a size
                posDish.size = Split(d, "/")(1) ' extracts size
                ' calculate size upgrade cost
                '
                '
                '
            Else ' no size specified
                posDish.totalPrice = dishInfo(0).BasePrice
                posDish.name = dishInfo(0).Name
                posDish.size = "N/A"
            End If

            posDishes.Add(posDish)
        Next

        Return posDishes
    End Function

    ' Makes sure only money values can be entered into the text box
    Private Sub txtboxAmtPaid_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtboxAmtPaid.TextChanged
        Dim text As String = txtboxAmtPaid.Text
        Dim Letter As String
        Dim SelectionIndex As Integer = txtboxAmtPaid.SelectionStart
        Dim Change As Integer

        For x As Integer = 0 To txtboxAmtPaid.Text.Length - 1
            Letter = txtboxAmtPaid.Text.Substring(x, 1)
            If charactersAllowed.Contains(Letter) = False Then
                text = text.Replace(Letter, String.Empty)
                Change = 1
            End If
        Next

        txtboxAmtPaid.Text = text
    End Sub

    Private Async Sub btnPay_Click(sender As Object, e As RoutedEventArgs) Handles btnPay.Click
        ' Disable button and text box while processing payment to server


        ' Check if amount paid is lower than amount owed
        If txtboxAmtPaid.Text < orderInfo.orderTotal Then
            ' Display error message
            lblErrorMsg.Visibility = Visibility.Visible
        Else
            btnPay.IsEnabled = False
            txtboxAmtPaid.IsEnabled = False

            orderInfo.amtPaid = txtboxAmtPaid.Text

            ' Set up original order before sending back to server
            orderInfo.origOrder.AmtPaid = orderInfo.amtPaid
            orderInfo.origOrder.TimePaid = DateTime.Now

            ' Send original order back to server with amount paid and time paid
            Dim changedOrder = Await orderClient.SetOrder(orderInfo.origOrder)

            ' Message box to ask if a receipt should be printed
            Dim msgboxResult = MessageBox.Show("Change: " + (orderInfo.amtPaid - orderInfo.orderTotal).ToString("C2") + vbNewLine + "Would you like to print a receipt?", "My App", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)
            If msgboxResult = MessageBoxResult.Yes Then
                printReceipt()

                Dim lastWindow As posWindow = New posWindow(orderClient)
                lastWindow.Show()
                Me.Close()
            ElseIf msgboxResult = MessageBoxResult.No Then
                Dim lastWindow As posWindow = New posWindow(orderClient)
                lastWindow.Show()
                Me.Close()
            End If
        End If

    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Dim lastWindow As posWindow = New posWindow(orderClient)
        lastWindow.Show()
        Me.Close()
    End Sub

    Private Sub printReceipt()
        Dim receipt As New FlowDocument()

        Dim sec As New Section()

        Dim parInfo As New Paragraph()
        Dim parDishes As New Paragraph()
        Dim parTotals As New Paragraph()

        'parInfo.
    End Sub

End Class


