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



    Public Property subtotal As Single
    Public Property tax As Single
    Public Property orderTotal As Single
End Class

Public Class orderInfoWindow

    Dim orderClient As OSClient

    Public Sub New(client, order)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        orderClient = client
        initialiseWindow()

    End Sub

    ' Calculates and adds all required info in the window
    Public Async Sub initialiseWindow()
        Dim originalOrder = Await orderClient.GetOrder()
    End Sub
End Class
