﻿Imports OrderSoft

Public Class loginWindow
    Dim loginWindowClient As OSClient

    Public Sub New(client)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call

        ' Passing through client from MainWindow
        loginWindowClient = client

        ' Debugging, opens posWindow instantly
        'Dim nextWindow As posWindow = New posWindow(loginWindowClient)
        'nextWindow.Show()
    End Sub

    Private Async Sub login_onclick() Handles btnLogin.Click
        ' Disable login UI elements while interacting with server
        lblUser.IsEnabled = False
        lblPassword.IsEnabled = False
        txtUser.IsEnabled = False
        txtPassword.IsEnabled = False
        btnLogin.IsEnabled = False

        Dim failed = False
        Try
            Await loginWindowClient.Login(txtUser.Text, txtPassword.Password)
        Catch ex As Exception
            failed = True
            MessageBox.Show(ex.ToString, "Error while logging in")
        End Try

        ' Login failed
        If failed Then
            ' Re-enable login options for retry
            lblUser.IsEnabled = True
            lblPassword.IsEnabled = True
            txtUser.IsEnabled = True
            txtPassword.IsEnabled = True

            btnLogin.IsEnabled = True
        Else ' Login succeeded, open posWindow
            Dim nextWindow As posWindow = New posWindow(loginWindowClient)
            nextWindow.Show()

            Me.Close()
        End If
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim helpWindow As loginWindowHelp = New loginWindowHelp()
        helpWindow.Show()
    End Sub

    Private Sub txtUser_KeyDown(sender As Object, e As KeyEventArgs) Handles txtUser.KeyDown
        If e.Key = Key.Return Then
            login_onclick()
        End If
    End Sub

    Private Sub txtPassword_KeyDown(sender As Object, e As KeyEventArgs) Handles txtPassword.KeyDown
        If e.Key = Key.Return Then
            login_onclick()
        End If
    End Sub
End Class
