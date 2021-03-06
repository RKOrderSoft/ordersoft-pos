﻿Imports OrderSoft

Public Class loginWindow
    Dim loginWindowClient As OSClient ' This window's instance of client

    Public Sub New(client)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call

        ' Passing through client from MainWindow
        loginWindowClient = client
    End Sub

    Private Async Sub login_onclick() Handles btnLogin.Click
        ' Disable login UI elements while interacting with server
        lblUser.IsEnabled = False
        lblPassword.IsEnabled = False
        txtUser.IsEnabled = False
        txtPassword.IsEnabled = False
        btnLogin.IsEnabled = False

        Dim failed = False ' Variable to check if login succeeded or not
        ' Try login
        Try
            Await loginWindowClient.Login(txtUser.Text, txtPassword.Password)
        Catch ex As Exception ' If login fails due to connection issues, catch the error
            failed = True
            MessageBox.Show(ex.Message + ". Make sure login details are correct and server is still up.", "Error while logging in.")
        End Try

        ' Login failed
        If failed Then
            ' Re-enable login options for retry
            lblUser.IsEnabled = True
            lblPassword.IsEnabled = True
            txtUser.IsEnabled = True
            txtPassword.IsEnabled = True

            btnLogin.IsEnabled = True
        Else ' Login succeeded
            Dim nextWindow As posWindow = New posWindow(loginWindowClient)
            nextWindow.Show()
            Me.Close()
        End If
    End Sub

    ' Handles opening of help window
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim helpWindow As loginWindowHelp = New loginWindowHelp()
        helpWindow.Show()
    End Sub

    ' Handles pressing enter key to login while focused on username box
    Private Sub txtUser_KeyDown(sender As Object, e As KeyEventArgs) Handles txtUser.KeyDown
        If e.Key = Key.Return Then
            login_onclick()
        End If
    End Sub

    ' Handles pressing enter key to login while focused on username box
    Private Sub txtPassword_KeyDown(sender As Object, e As KeyEventArgs) Handles txtPassword.KeyDown
        If e.Key = Key.Return Then
            login_onclick()
        End If
    End Sub

    ' Handles close program button
    Private Sub btnClose_Click(sender As Object, e As RoutedEventArgs) Handles btnClose.Click
        If MessageBox.Show("Are you sure you want to close the program?", "My App", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then
            End
        End If
    End Sub
End Class
