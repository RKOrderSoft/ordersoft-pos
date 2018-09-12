Imports OrderSoft

Class MainWindow
    Dim client As New OSClient()

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    ' Handle connect button
    Private Async Sub connect_onclick() Handles btnConnect.Click
        ' Set IP UI elements as disabled while loading
        lblIP.IsEnabled = False
        txtIP.IsEnabled = False
        btnConnect.IsEnabled = False

        Dim failed = False ' Variable tracking whether post was failed or not
        Try
            ' Initiate client with given IP
            Await client.Init("http://" + (txtIP.Text).ToString() + "/")
        Catch ex As Exception
            ' Error was thrown during initialisation
            failed = True
            MessageBox.Show(ex.ToString, "An error occurred while connecting")
        End Try

        ' IP failed to connect
        If failed Then
            ' Re-enable connection UI elements to allow retry
            lblIP.IsEnabled = True
            txtIP.IsEnabled = True
            btnConnect.IsEnabled = True
        Else ' Connection succeeded, open login window
            Dim nextWindow As loginWindow = New loginWindow(client)
            Me.Hide()
            nextWindow.Show()
        End If
    End Sub

    ' Handles posting to server when focused on IP text box
    Private Sub txtIP_KeyDown(sender As Object, e As KeyEventArgs) Handles txtIP.KeyDown
        If e.Key = Key.Return Then ' If key pressed in enter then
            connect_onclick() ' Initiate connection to server
        End If
    End Sub

    ' Handles opening of help window
    Private Sub btnHelp_Click(sender As Object, e As RoutedEventArgs)
        Dim helpWindow = New MainWindowHelp()
        helpWindow.Show()
    End Sub
End Class
