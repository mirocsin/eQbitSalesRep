'Imports System.Data.SqlClient
Imports System.IO
Imports MySql.Data.MySqlClient

Public Class SQLClass
    Implements IDisposable

    Private SqlConnection1 As MySqlConnection
    Private SqlCommand1 As MySqlCommand
    Private SqlTransaction1 As MySqlTransaction = Nothing
    Public Shared ThrowException As Boolean = True

    Public ReadOnly Property Sql_Connection() As MySqlConnection
        Get
            Return SqlConnection1
        End Get
    End Property

    Public Property Sql_Transaction() As MySqlTransaction
        Get
            Return SqlTransaction1
        End Get
        Set(ByVal value As MySqlTransaction)
            SqlTransaction1 = value
        End Set
    End Property

    Public Sub New(ByVal tcConnectionString As String)
        Try
            SqlConnection1 = New MySqlConnection(tcConnectionString)
            SqlConnection1.Open()
            SqlCommand1 = New MySqlCommand
            SqlCommand1.Connection = SqlConnection1
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Function SqlConnection_BeginTransaction() As MySqlTransaction
        SqlTransaction1 = SqlConnection1.BeginTransaction(IsolationLevel.ReadUncommitted)
        SqlConnection_BeginTransaction = SqlTransaction1
        SqlCommand1.Transaction = SqlTransaction1
    End Function

    Public Sub Close()
        SqlConnection1.Close()
        SqlConnection1.Dispose()
        SqlCommand1 = Nothing
        SqlConnection1 = Nothing
        SqlCommand1 = Nothing
    End Sub

    Public Sub AddParameter(ByVal tcParamName As String, ByVal toValue As Object)
        SqlCommand1.Parameters.AddWithValue(tcParamName, toValue)
    End Sub

    Public Sub ExecuteSQLScript(ByVal tcSQLScriptPath As String)
        Dim strScript As String = File.OpenText(tcSQLScriptPath).ReadToEnd
        Try
            With SqlCommand1
                .CommandText = strScript
                .ExecuteNonQuery()
            End With
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Public Sub ExecuteQuery(ByVal tcSQL As String)
        SqlCommand1.CommandText = tcSQL
        Try
            SqlCommand1.ExecuteNonQuery()
        Catch ex As Exception
            If ThrowException Then
                Throw New System.Exception(ex.Message)
            Else
                MsgBox(ex.Message, MsgBoxStyle.Critical)
            End If
        End Try
    End Sub

    Public Sub ClearParameter()
        SqlCommand1.Parameters.Clear()
    End Sub

    Function ExecuteQueryToDataset(ByVal tcSQL As String, Optional ByVal tlClearParameter As Boolean = True) As DataSet
        Dim da As New MySqlDataAdapter
        Dim ds As New DataSet
        Dim loHash As New Hashtable
        SqlCommand1.CommandText = tcSQL
        da.SelectCommand = SqlCommand1
        If Not SqlTransaction1 Is Nothing Then
            da.SelectCommand.Transaction = SqlTransaction1
        End If
        da.Fill(ds)
        If tlClearParameter Then
            SqlCommand1.Parameters.Clear()
        End If
        ExecuteQueryToDataset = ds
        da.Dispose()
        da = Nothing
        loHash = Nothing
    End Function

    Function ExecuteQueryUpdateableDataset(ByVal tcSQL As String, Optional ByVal tlClearParameter As Boolean = True) As Hashtable
        Dim da As New MySqlDataAdapter
        Dim ds As New DataSet
        Dim loHash As New Hashtable
        Dim sqlCommand2 As New MySqlCommand
        sqlCommand2.CommandText = tcSQL
        sqlCommand2.Connection = Me.Sql_Connection

        Dim myParamArray(SqlCommand1.Parameters.Count - 1) As MySqlParameter
        SqlCommand1.Parameters.CopyTo(myParamArray, 0)
        SqlCommand1.Parameters.Clear()
        For Each oPara In myParamArray
            sqlCommand2.Parameters.Add(oPara)
        Next

        da.SelectCommand = sqlCommand2
        If Not SqlTransaction1 Is Nothing Then
            da.SelectCommand.Transaction = SqlTransaction1
        End If
        'da.Fill(ds)
        da.FillSchema(ds, SchemaType.Mapped)

        Dim objCommandBuilder As New MySqlCommandBuilder(da)
        objCommandBuilder.ConflictOption = ConflictOption.OverwriteChanges 'This property fixes the Concurrency Violation Update error

        'da.UpdateCommand = objCommandBuilder.GetUpdateCommand
        'da.InsertCommand = objCommandBuilder.GetInsertCommand
        'da.DeleteCommand = objCommandBuilder.GetDeleteCommand

        'If tlClearParameter Then
        '    sqlCommand2.Parameters.Clear()
        'End If
        loHash.Add("ds", ds)
        loHash.Add("da", da)
        ExecuteQueryUpdateableDataset = loHash
    End Function

#Region "IDisposable Support "
    ' A class that implements IDisposable. 
    ' By implementing IDisposable, you are announcing that  
    ' instances of this type allocate scarce resources. 
    ' Pointer to an external unmanaged resource. 
    Private handle As IntPtr
    ' Track whether Dispose has been called. 
    Private disposed As Boolean = False
    ' Implement IDisposable. 
    ' Do not make this method virtual. 
    ' A derived class should not be able to override this method. 
    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        ' This object will be cleaned up by the Dispose method. 
        ' Therefore, you should call GC.SupressFinalize to 
        ' take this object off the finalization queue  
        ' and prevent finalization code for this object 
        ' from executing a second time.
        GC.SuppressFinalize(Me)
    End Sub

    ' Dispose(bool disposing) executes in two distinct scenarios. 
    ' If disposing equals true, the method has been called directly 
    ' or indirectly by a user's code. Managed and unmanaged resources 
    ' can be disposed. 
    ' If disposing equals false, the method has been called by the  
    ' runtime from inside the finalizer and you should not reference  
    ' other objects. Only unmanaged resources can be disposed. 
    Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
        ' Check to see if Dispose has already been called. 
        If Not Me.disposed Then
            ' If disposing equals true, dispose all managed  
            ' and unmanaged resources. 
            If disposing Then
                ' Dispose managed resources.
                'component.Dispose()
            End If
            ' Call the appropriate methods to clean up  
            ' unmanaged resources here. 
            ' If disposing is false,  
            ' only the following code is executed.
            CloseHandle(handle)
            handle = IntPtr.Zero
            ' Note disposing has been done.
            disposed = True
        End If
    End Sub
    ' Use interop to call the method necessary   
    ' to clean up the unmanaged resource.
    <System.Runtime.InteropServices.DllImport("Kernel32")> _
    Private Shared Function CloseHandle(ByVal handle As IntPtr) As [Boolean]
    End Function

    ' This finalizer will run only if the Dispose method  
    ' does not get called. 
    ' It gives your base class the opportunity to finalize. 
    ' Do not provide finalize methods in types derived from this class. 
    Protected Overrides Sub Finalize()
        ' Do not re-create Dispose clean-up code here. 
        ' Calling Dispose(false) is optimal in terms of 
        ' readability and maintainability.
        Dispose(False)
        MyBase.Finalize()
    End Sub

#End Region


End Class
