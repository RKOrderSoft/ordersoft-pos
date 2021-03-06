﻿Imports OrderSoft
Imports System.Windows.Documents

' A dish object made for this window to simplify things
Public Class POSDish
    Public Property dishId As Integer
    Public Property name As String
    Public Property size As String
    Public Property basePrice As Single
    Public Property upgradePrice As Single
    Public Property totalPrice As Single
End Class

' An order object made for this window to simplify things
Public Class POSOrder
    ' Basic order information
    Public Property origOrder As OrderObject
    Public Property orderId As String
    Public Property tableNumber As Integer
    Public Property serverID As String
    Public Property serverInfo As UserObject
    Public Property serverName As String
    Public Property notes As String

    ' All dishes in the order in a simple list of POSDishes
    Public Property allDishes As List(Of POSDish)

    ' Money amounts
    Public Property subtotal As Single
    Public Property tax As Single
    Public Property orderTotal As Single
    Public Property amtPaid As Single
    Public Property change As Single
End Class

Public Class Printer


End Class

Public Class orderInfoWindow
    Dim charactersAllowed As New List(Of Char) ' Characters allowed to be in the amount paid text box
    Dim orderClient As OSClient ' Client for this window
    Dim orderInfo As New POSOrder ' Object representing the order opened in this window from last window

    Public Sub New(client, order)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' Passing in client from last window
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

        ' Hide error message labels
        lblErrorNoAmtEntered.Visibility = Visibility.Hidden
        lblErrorPaidTooLow.Visibility = Visibility.Hidden

        initialiseWindow(order) ' Initialises the window with information from the order selected on last window
    End Sub

    ' Calculates and adds all required info into the window
    Public Async Sub initialiseWindow(order)
        Dim originalOrder = Await orderClient.GetOrder(order)

        ' Populating orderInfo
        orderInfo.origOrder = originalOrder
        orderInfo.orderId = order
        orderInfo.tableNumber = originalOrder.TableNumber
        orderInfo.serverID = originalOrder.ServerId
        orderInfo.notes = originalOrder.Notes

        ' Get the server name from the server ID
        orderInfo.serverInfo = Await orderClient.GetUserDetails(orderInfo.serverID)
        orderInfo.serverName = orderInfo.serverInfo.Username

        ' Take string of dishes and convert to a list of POSDishes for easier access to data
        orderInfo.allDishes = Await parseDishes(originalOrder.Dishes)

        ' Calculate total and tax
        For Each dish As POSDish In orderInfo.allDishes
            orderInfo.orderTotal = orderInfo.orderTotal + dish.totalPrice
        Next
        orderInfo.tax = orderInfo.orderTotal / 11

        ' Displaying information on labels in window
        lblOrderId.Content = "Order ID: " + orderInfo.orderId
        lblServerID.Content = "Server Name: " + orderInfo.serverName
        lblTableNumber.Content = "Table Number: " + orderInfo.tableNumber.ToString()
        lblNotes.Content = "Notes: " + orderInfo.notes
        lblTax.Content = "Tax: " + orderInfo.tax.ToString("C2")
        lblSubtotal.Content = "Subtotal: " + orderInfo.orderTotal.ToString("C2")
        lblTotal.Content = "Total: " + orderInfo.orderTotal.ToString("C2")

        ' Populate list box with dishes
        For Each dish As POSDish In orderInfo.allDishes
            lvDishes.Items.Add(dish)
        Next
    End Sub

    ' Takes array of dishes and converts it into a list of POSDish
    Public Async Function parseDishes(Dishes) As Task(Of List(Of POSDish))
        Dim posDishes As New List(Of POSDish)

        ' Loop through the array of dishes and populates posDishes
        For Each d As String In Dishes
            Dim posDish As New POSDish
            posDish.dishId = Split(d, "/")(0) ' extracts ID
            Dim dishInfo = Await orderClient.GetDishes(dishId:=posDish.dishId)

            ' Set basic info
            posDish.name = dishInfo(0).Name
            posDish.basePrice = dishInfo(0).BasePrice

            If Split(d, "/").Length = 2 Then ' if the dish has a size, it's separated by a slash
                posDish.size = Split(d, "/")(1) ' extracts size by splitting by the slash
                posDish.upgradePrice = dishInfo(0).UpgradePrice
                ' Calculate size upgrade cost
                Dim indexOfSize = Array.IndexOf(dishInfo(0).Sizes, posDish.size)
                posDish.totalPrice = (dishInfo(0).BasePrice + (indexOfSize * dishInfo(0).UpgradePrice)).ToString("C2")
            Else ' No slash, therefore no size specified, use base price and size (if available)
                posDish.totalPrice = dishInfo(0).BasePrice.ToString("C2")
                If dishInfo(0).Sizes Is Nothing Then ' No sizes available at all for dish in database
                    posDish.size = "N/A" ' Set size to N/A on receipt and info window
                Else ' There are sizes available for the dish in the database
                    posDish.size = dishInfo(0).Sizes(0) ' Set size to default size (first size in database)
                End If
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
            If charactersAllowed.Contains(Letter) = False Then ' If the character entered is in the allowed characters array
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

                ' Calculate change
                orderInfo.change = orderInfo.amtPaid - orderInfo.orderTotal

                ' Set up original order before sending back to server
                orderInfo.origOrder.AmtPaid = orderInfo.amtPaid - orderInfo.change
                orderInfo.origOrder.TimePaid = DateTime.Now

                ' Send original order back to server with amount paid and time paid
                Dim changedOrder = Await orderClient.SetOrder(orderInfo.origOrder)



                ' Message box to ask if a receipt should be printed
                Dim msgboxResult = MessageBox.Show("Change: " + orderInfo.change.ToString("C2") + vbNewLine + "Would you like to print a receipt?", "My App", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)
                If msgboxResult = MessageBoxResult.Yes Then
                    ' Print receipt
                    ' Setup printing
                    Dim prnt As New PrintDialog()
                    Dim receipt As FlowDocument = printReceipt()
                    receipt.Name = "FlowDoc"

                    ' Print flow document
                    Dim idpSource As IDocumentPaginatorSource = receipt
                    If prnt.ShowDialog() = True Then
                        prnt.PrintDocument(idpSource.DocumentPaginator, "Receipt printing")
                    End If

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

    ' Cancel button, goes back to posWindow
    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Dim lastWindow As posWindow = New posWindow(orderClient)
        lastWindow.Show()
        Me.Close()
    End Sub

    ' Handles setting up the receipt for printing
    Private Function printReceipt() As FlowDocument
        ' Set up flow document
        Dim receipt As New FlowDocument()

        Dim sec As New Section()

        Dim parHeading As New Paragraph()
        Dim parHeader As New Paragraph()
        Dim parInfo As New Paragraph()
        Dim parDishes As New Paragraph()
        Dim parTotals As New Paragraph()

        ' Set up fonts for paragraphs
        parHeading.FontFamily = New FontFamily("Segoe UI")
        parHeader.FontFamily = New FontFamily("Segoe UI")
        parInfo.FontFamily = New FontFamily("Courier New")
        parDishes.FontFamily = New FontFamily("Courier New")
        parTotals.FontFamily = New FontFamily("Courier New")

        ' Setting up heading (Ordersoft System)
        parHeading.FontSize = 30
        parHeading.Inlines.Add("Ordersoft")

        ' Setting up header
        parHeader.FontSize = 20
        parHeader.Inlines.Add("Manmaruya Beverly Hills")

        ' Adding info to flow document
        ' Basic info
        parInfo.FontSize = 8
        parInfo.Inlines.Add("Time paid: " + orderInfo.origOrder.TimePaidString + vbNewLine)
        parInfo.Inlines.Add("Table number: " + orderInfo.tableNumber.ToString() + vbNewLine)
        parInfo.Inlines.Add("Server: " + orderInfo.serverName + vbNewLine)

        ' Adding dishes to the flow document
        parDishes.FontSize = 8

        ' Algorithms to calculate spacing and formatting for the dishes in the flow document
        ' Determine length of longest string in dish names to format receipt
        Dim lenLongestDish As Integer = 0
        For Each dish As POSDish In orderInfo.allDishes
            If lenLongestDish < Len(dish.name) Then ' If the length of dish name is longer than lenLongestDish
                lenLongestDish = Len(dish.name) ' Set the length of the longest dish name to the length of the dish name
            End If
        Next

        ' Determine length of longest string in dish sizes to format receipt
        Dim lenLongestSize As Integer = 0
        For Each dish As POSDish In orderInfo.allDishes
            If lenLongestSize < Len(dish.size) Then ' If the length of dish size is longer than lenLongestSize
                lenLongestSize = Len(dish.size) ' Set the length of the longest dish size to the length of the dish size
            End If
        Next

        ' Adding lines to dishes paragraph
        For Each dish As POSDish In orderInfo.allDishes
            parDishes.Inlines.Add(dish.name + New String(" ", lenLongestDish - Len(dish.name) + 1) + "- " + dish.size + New String(" ", lenLongestSize - Len(dish.size) + 1) + "- " + dish.totalPrice.ToString("C2") + vbNewLine)
        Next

        ' Add final totals to receipt
        parTotals.FontSize = 8
        parTotals.Inlines.Add("Notes: " + orderInfo.notes + vbNewLine)
        parTotals.Inlines.Add("Tax: " + orderInfo.tax.ToString("C2") + vbNewLine)
        parTotals.Inlines.Add("Total due: " + orderInfo.orderTotal.ToString("C2") + vbNewLine)
        parTotals.Inlines.Add("Amount paid: " + orderInfo.amtPaid.ToString("C2") + vbNewLine)
        parTotals.Inlines.Add("Change: " + orderInfo.change.ToString("C2") + vbNewLine)

        ' Add paragraphs to the section
        sec.Blocks.Add(parHeading)
        sec.Blocks.Add(parHeader)
        sec.Blocks.Add(parInfo)
        sec.Blocks.Add(parDishes)
        sec.Blocks.Add(parTotals)

        ' Add section to the flow document
        receipt.Blocks.Add(sec)

        ' Return flow document for printing
        Return receipt
    End Function

    ' Handles opening help window
    Private Sub btnHelp_Click(sender As Object, e As RoutedEventArgs) Handles btnHelp.Click
        Dim helpWindow = New orderInfoWindowHelp()
        helpWindow.Show()
    End Sub

    ' Handling keypad clicks, adding numbers to the text box
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
        If txtboxAmtPaid.Text.Contains(".") Then ' If there's already a decimal point in the text box
        Else
            txtboxAmtPaid.Text += "."
        End If
    End Sub

    Private Sub btnBackspace_Click(sender As Object, e As RoutedEventArgs) Handles btnBackspace.Click
        If txtboxAmtPaid.Text = "" Then ' If the text box is already empty then do nothing
        Else ' If text box is not empty then backspace
            txtboxAmtPaid.Text = txtboxAmtPaid.Text.Substring(0, Len(txtboxAmtPaid.Text) - 1)
        End If
    End Sub

    ' Handle program close button
    Private Sub btnClose_Click(sender As Object, e As RoutedEventArgs) Handles btnClose.Click
        If MessageBox.Show("Are you sure you want to logout and close the program?", "My App", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then
            End
        End If
    End Sub
End Class


