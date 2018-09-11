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
    Public Property change As Single
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

        ' Populating orderInfo
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
                posDish.name = dishInfo(0).Name
                posDish.size = Split(d, "/")(1) ' extracts size
                posDish.upgradePrice = dishInfo(0).UpgradePrice
                ' calculate size upgrade cost
                Dim indexOfSize = Array.IndexOf(dishInfo(0).Sizes, posDish.size)
                posDish.totalPrice = dishInfo(0).BasePrice + (indexOfSize * dishInfo(0).UpgradePrice)
            Else ' no size specified
                posDish.totalPrice = dishInfo(0).BasePrice
                posDish.name = dishInfo(0).Name
                posDish.size = "N/A"
            End If
            posDishes.Add(posDish)
        Next

        Return posDishes
    End Function

    ' Data validation
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

        ' Check if amount paid is empty
        If txtboxAmtPaid.Text = "" Then
            lblErrorNoAmtEntered.Visibility = Visibility.Visible
            lblErrorPaidTooLow.Visibility = Visibility.Hidden
        Else
            ' Check if amount paid is lower than amount owed
            If txtboxAmtPaid.Text < orderInfo.orderTotal Then ' Amount paid is lower than amount owed
                ' Display error message
                lblErrorPaidTooLow.Visibility = Visibility.Visible
                lblErrorNoAmtEntered.Visibility = Visibility.Hidden
            Else ' Amount paid is higher than amount owed
                btnPay.IsEnabled = False
                txtboxAmtPaid.IsEnabled = False

                orderInfo.amtPaid = txtboxAmtPaid.Text

                ' Set up original order before sending back to server
                orderInfo.origOrder.AmtPaid = orderInfo.amtPaid
                orderInfo.origOrder.TimePaid = DateTime.Now

                ' Send original order back to server with amount paid and time paid
                Dim changedOrder = Await orderClient.SetOrder(orderInfo.origOrder)

                ' Calculate change
                orderInfo.change = orderInfo.amtPaid - orderInfo.orderTotal

                ' Message box to ask if a receipt should be printed
                Dim msgboxResult = MessageBox.Show("Change: " + orderInfo.change.ToString("C2") + vbNewLine + "Would you like to print a receipt?", "My App", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)
                If msgboxResult = MessageBoxResult.Yes Then
                    ' Print receipt
                    Dim prnt As New PrintDialog()
                    Dim receipt As FlowDocument = printReceipt()
                    receipt.Name = "FlowDoc"

                    Dim idpSource As IDocumentPaginatorSource = receipt
                    prnt.PrintDocument(idpSource.DocumentPaginator, "Receipt printing")

                    ' Reopen posWindow
                    Dim lastWindow As posWindow = New posWindow(orderClient)
                    lastWindow.Show()
                    Me.Close()
                ElseIf msgboxResult = MessageBoxResult.No Then
                    ' Reopen posWindow
                    Dim lastWindow As posWindow = New posWindow(orderClient)
                    lastWindow.Show()
                    Me.Close()
                End If
            End If
        End If


    End Sub

    ' Cancel button
    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Dim lastWindow As posWindow = New posWindow(orderClient)
        lastWindow.Show()
        Me.Close()
    End Sub

    Private Function printReceipt() As FlowDocument
        ' Set up flow document
        Dim receipt As New FlowDocument()

        Dim sec As New Section()

        Dim parInfo As New Paragraph()
        Dim parDishes As New Paragraph()
        Dim parTotals As New Paragraph()

        ' Set up fonts for paragraphs
        parInfo.FontFamily = New FontFamily("Courier New")
        parDishes.FontFamily = New FontFamily("Courier New")
        parTotals.FontFamily = New FontFamily("Courier New")

        ' Adding info to flow document
        parInfo.Inlines.Add("Time paid: " + orderInfo.origOrder.TimePaidString + vbNewLine)
        parInfo.Inlines.Add("Table number: " + orderInfo.tableNumber.ToString() + vbNewLine)


        For Each dish As POSDish In orderInfo.dishIds
            parDishes.Inlines.Add(dish.name + "   " + dish.size + "   " + dish.totalPrice.ToString("C2") + vbNewLine)
        Next

        parTotals.Inlines.Add("Notes: " + orderInfo.notes + vbNewLine)
        parTotals.Inlines.Add("Tax: " + orderInfo.tax.ToString("C2") + vbNewLine)
        parTotals.Inlines.Add("Total due: " + orderInfo.orderTotal.ToString("C2") + vbNewLine)
        parTotals.Inlines.Add("Amount paid: " + orderInfo.amtPaid.ToString("C2") + vbNewLine)
        parTotals.Inlines.Add("Change: " + orderInfo.change.ToString("C2") + vbNewLine)

        sec.Blocks.Add(parInfo)
        sec.Blocks.Add(parDishes)
        sec.Blocks.Add(parTotals)
        receipt.Blocks.Add(sec)

        ' Return flow document
        Return receipt
    End Function

    Private Sub btnHelp_Click(sender As Object, e As RoutedEventArgs) Handles btnHelp.Click
        Dim helpWindow = New orderInfoWindowHelp()
        helpWindow.Show()
    End Sub

    ' Handling keypad clicks
    Private Sub btn1_Click(sender As Object, e As RoutedEventArgs) Handles btn1.Click
        txtboxAmtPaid.Text += "1"
    End Sub

    Private Sub btn2_Click(sender As Object, e As RoutedEventArgs) Handles btn2.Click
        txtboxAmtPaid.Text += "2"
    End Sub

    Private Sub btn3_Click(sender As Object, e As RoutedEventArgs) Handles btn3.Click
        txtboxAmtPaid.Text += "3"
    End Sub

    Private Sub btn4_Click(sender As Object, e As RoutedEventArgs) Handles btn4.Click
        txtboxAmtPaid.Text += "4"
    End Sub

    Private Sub btn5_Click(sender As Object, e As RoutedEventArgs) Handles btn5.Click
        txtboxAmtPaid.Text += "5"
    End Sub

    Private Sub btn6_Click(sender As Object, e As RoutedEventArgs) Handles btn6.Click
        txtboxAmtPaid.Text += "6"
    End Sub

    Private Sub btn7_Click(sender As Object, e As RoutedEventArgs) Handles btn7.Click
        txtboxAmtPaid.Text += "7"
    End Sub

    Private Sub btn8_Click(sender As Object, e As RoutedEventArgs) Handles btn8.Click
        txtboxAmtPaid.Text += "8"
    End Sub

    Private Sub btn9_Click(sender As Object, e As RoutedEventArgs) Handles btn9.Click
        txtboxAmtPaid.Text += "9"
    End Sub

    Private Sub btn0_Click(sender As Object, e As RoutedEventArgs) Handles btn0.Click
        txtboxAmtPaid.Text += "0"
    End Sub

    Private Sub btnDot_Click(sender As Object, e As RoutedEventArgs) Handles btnDot.Click
        txtboxAmtPaid.Text += "."
    End Sub
End Class


