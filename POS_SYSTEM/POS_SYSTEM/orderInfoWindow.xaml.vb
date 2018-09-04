Imports OrderSoft

Public Class POSDish
    Public Property dishId As Integer
    Public Property name As String
    Public Property size As String
    Public Property upgradePrice As Single
    Public Property totalPrice As Single
End Class

Public Class POSOrder
    Public Property origOrder As OrderObject
    Public Property orderId As String
    Public Property tableNumber As Integer
    Public Property serverID As String
    Public Property notes As String

    Public Property dishIds As List(Of String)

    Public Property subtotal As Single
    Public Property tax As Single
    Public Property orderTotal As Single
    Public Property amtPaid As Single
End Class

Public Class orderInfoWindow

    Dim orderClient As OSClient
    Dim orderInfo As POSOrder


    Public Sub New(client, order)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        orderClient = client
        initialiseWindow(order)

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
        orderInfo.dishIds = parseDishes(originalOrder.Dishes)




    End Sub

    Public Sub parseDishes(strDishes)
        Dim dishIds As List(Of String)


        Return dishIds
    End Sub
End Class
