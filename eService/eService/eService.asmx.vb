﻿Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data
Imports Newtonsoft.Json
Imports System.IO
Imports Newtonsoft.Json.JsonConvert
Imports System.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports System.Web.Script.Serialization

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://microsoft.com/webservices/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class eService
    Inherits System.Web.Services.WebService
    Private strConnectionString As String = "customer_loreal_backendConnectionString"
    Private strdtformat As String = "dd/MM/yyyy HH:mm:ss tt"

    <WebMethod()> _
    Public Sub getEmployeeNotification(id As String, vchPO_Code As String)
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim strSQL As String
            '-- Purchase Orders
            Dim dtPO As DataTable
            Dim dsObj1 As New dsObj
            strSQL = "SELECT * FROM tblpo_headers WHERE vchStatus = 'CLOSED' and vchSalesman_Code = @id and COALESCE(vchStatus2,'')='' and vchPO_Code > @vchPO_Code ORDER BY vchPO_Code LIMIT 10"
            oSqlClass.AddParameter("id", id)
            oSqlClass.AddParameter("vchPO_Code", vchPO_Code)
            dtPO = oSqlClass.ExecuteQueryToDataset(strSQL).Tables(0)
            dtPO.TableName = "for_approval"
            dsObj1.addTable(dtPO)
            '-- OTHER table query notifications here...

            '--
            writeObjResponse(dsObj1.getDataset)
            oSqlClass.Close()
            oSqlClass = Nothing
            TryDispose(dtPO)
        Catch ex As Exception
            writeObjResponse(setMessage(ex.Message))
        End Try
    End Sub

    <WebMethod()> _
    Public Sub getMemberNotifications(vchCode As String)
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim strSQL As String
            '-- Purchase Orders
            Dim dtNoti As DataTable
            Dim dsObj1 As New dsObj
            strSQL = "SELECT * FROM tblnotifications WHERE vchCode = @vchCode and bitRead = 0 ORDER BY dtRecordAdded"
            oSqlClass.AddParameter("vchCode", vchCode)
            dtNoti = oSqlClass.ExecuteQueryToDataset(strSQL).Tables(0)
            dtNoti.TableName = "mem_notification"
            dsObj1.addTable(dtNoti)
            '-- OTHER table query notifications here...

            '--
            writeObjResponse(dsObj1.getDataset)
            oSqlClass.Close()
            oSqlClass = Nothing
            TryDispose(dtNoti)
        Catch ex As Exception
            writeObjResponse(setMessage(ex.Message))
        End Try
    End Sub

    <WebMethod()> _
    Public Sub createMemAutoReplenish(vchCust_Code As String, intRefnoti_id As Integer)
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim strSQL As String
            Dim dtRep As DataTable
            Dim dsObj1 As New dsObj
            strSQL = "select a.vchItem_Code, a.intOffset_bal, b.intQtyMax, b.intQtyMax-a.intOffset_bal as intToOrderQty from tblstk_movement a " & _
                     "inner join tblitems b on a.vchItem_Code = b.vchItem_Code where a.vchCust_Code=@vchCust_Code and a.intRefnoti_id=@intRefnoti_id"
            oSqlClass.AddParameter("vchCust_Code", vchCust_Code)
            oSqlClass.AddParameter("intRefnoti_id", intRefnoti_id)
            dtRep = oSqlClass.ExecuteQueryToDataset(strSQL).Tables(0)
            dtRep.TableName = "mem_autoreplenish"
            dsObj1.addTable(dtRep)
            writeObjResponse(dsObj1.getDataset)
            oSqlClass.Close()
            oSqlClass = Nothing
            TryDispose(dtRep)
        Catch ex As Exception
            writeObjResponse(setMessage(ex.Message))
        End Try
    End Sub

    <WebMethod()> _
    Public Sub updateNotiRead(vchCust_Code As String)
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim time As DateTime = DateTime.Now
            oSqlClass.AddParameter("vchCode", vchCust_Code)
            oSqlClass.AddParameter("datenow", time.ToString(strdtformat))
            oSqlClass.ExecuteQuery("Update tblnotifications SET bitRead = 1, dtRecordModed = @datenow WHERE vchCode = @vchCode")
            writeObjResponse(setMessage("SUCCESS"))
            oSqlClass.Close()
            oSqlClass = Nothing
        Catch ex As Exception
            writeObjResponse(setMessage(ex.Message))
        End Try
    End Sub

    <WebMethod()> _
    Public Sub getSOListClosed(id As String, vchSO_Code As String, scrolltype As String)
        Dim dt As DataTable
        Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
        Dim strSQL As String = "SELECT * FROM tblso_headers WHERE vchStatus = 'CLOSED' and vchSO_Code Like @id @addCondition ORDER BY vchSO_Code @sorting LIMIT 10"
        oSqlClass.AddParameter("id", id + "%")
        If String.IsNullOrEmpty(vchSO_Code) Then
            strSQL = Replace(strSQL, "@addCondition", "")
            strSQL = Replace(strSQL, "@sorting", "desc")
        Else
            If scrolltype = "pullup" Then
                ' Pull up
                strSQL = Replace(strSQL, "@addCondition", "and vchSO_Code < @vchSO_Code")
                strSQL = Replace(strSQL, "@sorting", "desc")
            ElseIf scrolltype = "pulldown" Then
                ' Pull down
                strSQL = Replace(strSQL, "@addCondition", "and vchSO_Code > @vchSO_Code")
                strSQL = Replace(strSQL, "@sorting", "asc")
            End If
            oSqlClass.AddParameter("vchSO_Code", vchSO_Code)
        End If
        dt = oSqlClass.ExecuteQueryToDataset(strSQL).Tables(0)
        writeObjResponse(dt)
        oSqlClass.Close()
        oSqlClass = Nothing
        TryDispose(dt)
    End Sub

    <WebMethod()> _
    Public Sub getPOListClosed(id As String, vchPO_Code As String, scrolltype As String, searchPO As String)
        Dim dt As DataTable
        Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
        Dim strSQL As String = ""
        If Not String.IsNullOrEmpty(searchPO) Then
            strSQL = "SELECT * FROM tblpo_headers WHERE vchStatus = 'CLOSED' and vchPO_Code Like @searchPO ORDER BY vchPO_Code desc LIMIT 10"
            strSQL = Replace(strSQL, "@searchPO", "'%" + searchPO + "%'")
        Else
            oSqlClass.AddParameter("id", id + "%")
            strSQL = "SELECT * FROM tblpo_headers WHERE vchStatus = 'CLOSED' and vchPO_Code Like @id @addCondition ORDER BY vchPO_Code @sorting LIMIT 10"
            If String.IsNullOrEmpty(vchPO_Code) Then
                strSQL = Replace(strSQL, "@addCondition", "")
                strSQL = Replace(strSQL, "@sorting", "desc")
            Else
                If scrolltype = "pullup" Then
                    ' Pull up
                    strSQL = Replace(strSQL, "@addCondition", "and vchPO_Code < @vchPO_Code")
                    strSQL = Replace(strSQL, "@sorting", "desc")
                ElseIf scrolltype = "pulldown" Then
                    ' Pull down
                    strSQL = Replace(strSQL, "@addCondition", "and vchPO_Code > @vchPO_Code")
                    strSQL = Replace(strSQL, "@sorting", "asc")
                End If
                oSqlClass.AddParameter("vchPO_Code", vchPO_Code)
            End If
        End If
        dt = oSqlClass.ExecuteQueryToDataset(strSQL).Tables(0)
        writeObjResponse(dt)
        oSqlClass.Close()
        oSqlClass = Nothing
        dt.Dispose()
        dt = Nothing
    End Sub

    <WebMethod()> _
    Public Sub getSOHeader(vchSO_Code As String, vchStatus As String)
        Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
        oSqlClass.AddParameter("vchSO_Code", vchSO_Code)
        oSqlClass.AddParameter("vchStatus", vchStatus)
        Dim dt As DataTable = oSqlClass.ExecuteQueryToDataset("SELECT * FROM tblso_headers WHERE vchSO_Code = @vchSO_Code and vchStatus = @vchStatus").Tables(0)
        writeObjResponse(dt)
        oSqlClass.Close()
        oSqlClass = Nothing
        dt.Dispose()
        dt = Nothing
    End Sub

    <WebMethod()> _
    Public Sub getSODetails(vchSO_Code As String, vchStatus As String)
        Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
        oSqlClass.AddParameter("vchSO_Code", vchSO_Code)
        oSqlClass.AddParameter("vchStatus", vchStatus)
        Dim dt As DataTable = oSqlClass.ExecuteQueryToDataset("SELECT * FROM tblso_details WHERE vchSO_Code = @vchSO_Code and vchStatus = @vchStatus").Tables(0)
        writeObjResponse(dt)
        oSqlClass.Close()
        oSqlClass = Nothing
        dt.Dispose()
        dt = Nothing
    End Sub

    <WebMethod()> _
    Public Sub getPOHeader(vchPO_Code As String, vchStatus As String)
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            oSqlClass.AddParameter("vchPO_Code", vchPO_Code)
            oSqlClass.AddParameter("vchStatus", vchStatus)
            Dim dt As DataTable = oSqlClass.ExecuteQueryToDataset("SELECT * FROM tblpo_headers WHERE vchPO_Code = @vchPO_Code and vchStatus = @vchStatus").Tables(0)
            writeObjResponse(dt)
            oSqlClass.Close()
            oSqlClass = Nothing
            TryDispose(dt)
        Catch ex As Exception
            writeObjResponse(setMessage("ERROR: " + ex.Message))
        End Try
    End Sub

    <WebMethod()> _
    Public Sub getPODetails(vchPO_Code As String, vchStatus As String)
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            oSqlClass.AddParameter("vchPO_Code", vchPO_Code)
            oSqlClass.AddParameter("vchStatus", vchStatus)
            Dim dt As DataTable = oSqlClass.ExecuteQueryToDataset("SELECT * FROM tblpo_details WHERE vchPO_Code = @vchPO_Code and vchStatus = @vchStatus ORDER BY dtRecordAdded").Tables(0)
            writeObjResponse(dt)
            oSqlClass.Close()
            oSqlClass = Nothing
            TryDispose(dt)
        Catch ex As Exception
            writeObjResponse(setMessage("ERROR: " + ex.Message))
        End Try
    End Sub

    <WebMethod()> _
    Public Sub approvePO(vchPO_Code As String)
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            oSqlClass.AddParameter("vchPO_Code", vchPO_Code)
            oSqlClass.ExecuteQuery("Update tblpo_headers SET vchStatus2 = 'APPROVED' WHERE vchPO_Code = @vchPO_Code AND vchStatus = 'CLOSED'")
            writeObjResponse(setMessage("SUCCESS"))
            oSqlClass.Close()
            oSqlClass = Nothing
        Catch ex As Exception
            writeObjResponse(setMessage("ERROR: " + ex.Message))
        End Try
    End Sub

    <WebMethod()> _
    Public Sub syncToServer(tableobj As String)
        Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
        Dim trTableTransaction As MySqlTransaction = oSqlClass.SqlConnection_BeginTransaction()
        Dim strMessage As String = ""

        '-- Deserialize table object
        Dim dtUpdate As New DataTable
        dtUpdate.Columns.Add("tablename", Type.GetType("System.String"))
        dtUpdate.Columns.Add("ds", Type.GetType("System.Object"))  ' Dataset from the ExecuteQueryUpdateableDataset
        dtUpdate.Columns.Add("dt", Type.GetType("System.Object")) ' Datatable from the ExecuteQueryUpdateableDataset
        dtUpdate.Columns.Add("dt2", Type.GetType("System.Object")) ' Datatable from Frontend Import
        dtUpdate.Columns.Add("da", Type.GetType("System.Object")) ' DataAdapter from the ExecuteQueryUpdateableDataset

        Try
            '-- Assign to the datatable object for structure and data
            Dim ds As New DataSetService
            Dim dtser As dtSerialize
            For ln = 2 To DeserializeObject(tableobj).count - 1
                dtser = DirectCast(DeserializeObject(DeserializeObject(tableobj)(ln).ToString, (GetType(dtSerialize))), dtSerialize)

                ' Get the table structure
                Dim oHash As Hashtable = oSqlClass.ExecuteQueryUpdateableDataset("SELECT * FROM " + dtser.name.ToLower.Trim + " WHERE dtype = 'NOT_EXIST'")
                Dim dt As DataTable = oHash("ds").tables(0) '.clone
                Dim da As MySqlDataAdapter = oHash("da")

                Dim dt2 As DataTable = DirectCast(DeserializeObject(dtser.data.ToString, (dt.GetType)), DataTable)
                Dim drNew As DataRow = dtUpdate.NewRow
                drNew("tablename") = dtser.name
                drNew("ds") = oHash("ds")
                drNew("dt") = dt
                drNew("dt2") = dt2
                drNew("da") = da
                dtUpdate.Rows.Add(drNew)
            Next

            '-- Update the rows
            For Each row In dtUpdate.Rows
                Dim dtUpdate2 As DataTable = row("dt")
                For Each oRecord As DataRow In CType(row("dt2"), DataTable).Rows
                    If oRecord("dtype") = "NEW" Then
                        ' NEW RECORD
                        dtUpdate2.ImportRow(oRecord)
                    ElseIf oRecord("dtype") = "EDIT" Then
                        ' EDIT RECORD
                        ' Note: LoadDataROw does not work for mysql
                        Dim drNew As DataRow = dtUpdate2.NewRow
                        For Each col In dtUpdate2.PrimaryKey
                            drNew(col.ColumnName) = oRecord(col.ColumnName)
                        Next
                        dtUpdate2.LoadDataRow(drNew.ItemArray, True)
                        dtUpdate2(dtUpdate2.Rows.Count - 1).AcceptChanges()
                        For Each col As DataColumn In dtUpdate2.Columns
                            With dtUpdate2(dtUpdate2.Rows.Count - 1)
                                .Item(col.ColumnName) = oRecord(col.ColumnName)
                            End With
                        Next
                    ElseIf oRecord("dtype") = "DELETE" Then
                        ' DELETE RECORD
                        dtUpdate2.LoadDataRow(oRecord.ItemArray, True)
                        dtUpdate2(dtUpdate2.Rows.Count - 1).Delete()
                    End If
                Next
            Next
            For Each row In dtUpdate.Rows
                CType(row("da"), MySqlDataAdapter).Update(CType(row("ds"), DataSet))
            Next
            trTableTransaction.Commit()
        Catch ex As Exception
            strMessage = "ERROR: " + ex.Message
            trTableTransaction.Rollback()
        End Try

        '-- Send
        Dim dsObj1 As New dsObj
        If String.IsNullOrEmpty(strMessage) Then
            '- Build to dataset class
            dsObj1.addTable(setMessage("SUCCESS"))

            '- Export
            Me.addDataTable(oSqlClass, dsObj1, "tblitems") ' Items
            Me.addDataTable(oSqlClass, dsObj1, "tblcategory") ' Category

            ' Write it!
            writeObjResponse(dsObj1.getDataset)
        Else
            '-- Send Error message
            dsObj1.addTable(setMessage(strMessage))

            ' Write it!
            writeObjResponse(dsObj1.getDataset)
        End If

    End Sub

    Private Function setMessage(msg As String) As DataTable
        Dim dtSend As New DataTable
        dtSend.Columns.Add("message", Type.GetType("System.String"))
        dtSend.TableName = "tblservermsg"
        Dim drNew As DataRow = dtSend.NewRow
        drNew("message") = msg
        dtSend.Rows.Add(drNew)
        setMessage = dtSend
    End Function

    Private Sub addDataTable(oSqlClass As SQLClass, dsObj1 As dsObj, tablename As String, Optional where As String = "")
        Dim dt As DataTable = oSqlClass.ExecuteQueryToDataset("SELECT * FROM  " + tablename + " " + where).Tables(0)
        dt.TableName = tablename
        dsObj1.addTable(dt)
    End Sub

    Private Sub TryDispose(obj As Object)
        Try
            obj.dispose()
        Catch ex As Exception
        End Try
        Try
            obj = Nothing
        Catch ex As Exception
        End Try
    End Sub


    <WebMethod()> _
    Public Sub activateSystem(activationCode As String)
        Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
        Dim trTableTransaction As MySqlTransaction = oSqlClass.SqlConnection_BeginTransaction()
        Dim dt As DataTable = Nothing
        oSqlClass.AddParameter("vchActivationCode", activationCode)
        Try
            'Dim dt As DataTable = oSqlClass.ExecuteQueryToDataset("SELECT * FROM tblacess WHERE vchActivationCode = @vchActivationCode AND COALESCE(bitActivated,0) = 0", False).Tables(0)
            dt = oSqlClass.ExecuteQueryToDataset("SELECT a.*,b.vchCust_Code,b.vchCust_Name,b.vchCust_Add1,b.vchCust_Add2,b.vchCust_Add3,b.vchCust_Cntry,b.vchCust_Zip,space(20) as vchMainpg,b.vchSalesman_Code,b.mnyMinOrderAmt FROM tblacess a LEFT JOIN tblcustomers b on a.vchUsername = b.vchLoginID WHERE vchActivationCode = @vchActivationCode", False).Tables(0)
            If dt.Rows.Count = 1 Then
                Dim dr As DataRow = dt(0)
                If dr("vchAccessType") = "member" Then
                    '-- Member
                    If IsDBNull(dr("vchCust_Code")) Then
                        writeObjResponse(setMessage("ERROR: User Login ID not found or not set"))
                        GoTo Exit_Sub
                    End If
                    dr("vchMainpg") = "pg_memmain"
                Else
                    '-- Employee
                    dr("vchMainpg") = "pg_empmain"
                End If
                oSqlClass.ExecuteQuery("Update tblacess SET bitActivated = 1 WHERE vchActivationCode = @vchActivationCode")
                trTableTransaction.Commit()
                writeObjResponse(dt)
            Else
                writeObjResponse(setMessage("ERROR: Activation code not valid"))
            End If

        Catch ex As Exception
            writeObjResponse(setMessage("ERROR: " + ex.Message))
            trTableTransaction.Rollback()
        End Try
Exit_Sub:
        oSqlClass.Close()
        oSqlClass = Nothing
        TryDispose(dt)
    End Sub

    <WebMethod()> _
    Public Sub getCustomerlist(name As String)

        'Context.Response.AddHeader("Access-Control-Allow-Origin", "*")
        'Context.Response.ContentType = "application/json"
        'Context.Response.Write("[{value: 'Zambian kwacha', data: 'ZMK'}, {value: 'Zimbabwean dollar', data: 'ZWD'}]")

        Dim dsObj1 As New dsObj
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim dt As DataTable
            If Not String.IsNullOrEmpty(name) Then
                oSqlClass.AddParameter("search", "%" + name + "%")
                dt = oSqlClass.ExecuteQueryToDataset("SELECT * FROM tblcustomers WHERE vchCust_Name LIKE @search OR vchCust_Code LIKE @search OR vchHomePhone LIKE @search OR vchMobilePhone LIKE @search LIMIT 10").Tables(0)
            Else
                dt = oSqlClass.ExecuteQueryToDataset("SELECT * FROM tblcustomers WHERE vchCust_Name = 'CUSTOMER_NOT_EXIST'").Tables(0)
            End If
            dsObj1.addTable(dt)
            oSqlClass.Close()
            oSqlClass = Nothing
        Catch ex As Exception
            dsObj1.addTable(setMessage("ERROR: " + ex.Message))
        End Try
        writeObjResponse(dsObj1.getDataset)
    End Sub

    <WebMethod()> _
    Public Sub getItemlist(name As String, type As String)
        Dim dsObj1 As New dsObj
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim dt As DataTable
            If Not String.IsNullOrEmpty(name) Then
                oSqlClass.AddParameter("search", "%" + name + "%")
                dt = oSqlClass.ExecuteQueryToDataset("SELECT * FROM tblitems WHERE vchItem_Code LIKE @search OR vchItem_Description LIKE @search OR vchBarcode LIKE @search OR vchItem_Category LIKE @search OR vchBrand LIKE @search  LIMIT 10").Tables(0)
            Else
                dt = oSqlClass.ExecuteQueryToDataset("SELECT * FROM tblitems WHERE vchItem_Code = 'ITEM_NOT_EXIST'").Tables(0)
            End If
            dsObj1.addTable(dt)
            oSqlClass.Close()
            oSqlClass = Nothing
        Catch ex As Exception
            dsObj1.addTable(setMessage("ERROR: " + ex.Message))
        End Try
        writeObjResponse(dsObj1.getDataset)
    End Sub

    <WebMethod()> _
    Public Sub insertRecord(tableobj As String)
        Dim dsObj1 As New dsObj
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim dtser As dtSerialize = DirectCast(DeserializeObject(DeserializeObject(tableobj).ToString, (GetType(dtSerialize))), dtSerialize)
            Dim oHash As Hashtable = oSqlClass.ExecuteQueryUpdateableDataset("SELECT * FROM " + dtser.name.ToLower.Trim + " WHERE dtype = 'NOT_EXIST'")
            Dim dt As DataTable = oHash("ds").tables(0)
            Dim da As MySqlDataAdapter = oHash("da")
            Dim dt2 As DataTable = DirectCast(DeserializeObject(dtser.data.ToString, (dt.GetType)), DataTable)
            dt.ImportRow(dt2.Rows(0))
            da.Update(dt)
            dsObj1.addTable(setMessage("SUCCESS"))
            oSqlClass.Close()
            oSqlClass = Nothing
        Catch ex As Exception
            dsObj1.addTable(setMessage("ERROR: " + ex.Message))
        End Try
        writeObjResponse(dsObj1.getDataset)
    End Sub

    <WebMethod()> _
    Public Sub updateRecord(tableobj As String)
        Dim dsObj1 As New dsObj
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim dtser As dtSerialize = DirectCast(DeserializeObject(DeserializeObject(tableobj).ToString, (GetType(dtSerialize))), dtSerialize)
            Dim oHash As Hashtable = oSqlClass.ExecuteQueryUpdateableDataset("SELECT * FROM " + dtser.name.ToLower.Trim + " WHERE dtype = 'NOT_EXIST'")
            Dim dt As DataTable = oHash("ds").tables(0)
            Dim da As MySqlDataAdapter = oHash("da")
            Dim dt2 As DataTable = DirectCast(DeserializeObject(dtser.data.ToString, (dt.GetType)), DataTable)
            Dim oRecord As DataRow = dt2(0)
            Dim drNew As DataRow = dt.NewRow
            For Each col In dt.PrimaryKey
                drNew(col.ColumnName) = oRecord(col.ColumnName)
            Next
            dt.LoadDataRow(drNew.ItemArray, True)
            dt.AcceptChanges()
            For Each col As DataColumn In dt2.Columns
                dt(0)(col.ColumnName) = oRecord(col.ColumnName)
            Next
            da.Update(dt)
            dsObj1.addTable(setMessage("SUCCESS"))
            oSqlClass.Close()
            oSqlClass = Nothing
        Catch ex As Exception
            dsObj1.addTable(setMessage("ERROR: " + ex.Message))
        End Try
        writeObjResponse(dsObj1.getDataset)
    End Sub

    <WebMethod()> _
    Public Sub updatePODetails(tableobj As String)
        Dim dsObj1 As New dsObj
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim trTableTransaction As MySqlTransaction = oSqlClass.SqlConnection_BeginTransaction()

            Dim dtser As dtSerialize = DirectCast(DeserializeObject(DeserializeObject(tableobj).ToString, (GetType(dtSerialize))), dtSerialize)
            Dim oHash As Hashtable = oSqlClass.ExecuteQueryUpdateableDataset("SELECT * FROM " + dtser.name.ToLower.Trim + " WHERE dtype = 'NOT_EXIST'")
            Dim dt As DataTable = oHash("ds").tables(0)
            Dim da As MySqlDataAdapter = oHash("da")
            Dim dt2 As DataTable = DirectCast(DeserializeObject(dtser.data.ToString, (dt.GetType)), DataTable)
            Dim oRecord As DataRow = dt2(0)
            Dim strPOCode As String = ""

            If oRecord("dtype") = "EDIT" Then
                Dim drNew As DataRow = dt.NewRow
                For Each col In dt.PrimaryKey
                    drNew(col.ColumnName) = oRecord(col.ColumnName)
                Next
                dt.LoadDataRow(drNew.ItemArray, True)
                dt.AcceptChanges()
                For Each col As DataColumn In dt2.Columns
                    dt(0)(col.ColumnName) = oRecord(col.ColumnName)
                Next
                da.Update(dt)
                strPOCode = dt.Rows(0)("vchPO_Code")
            ElseIf oRecord("dtype") = "DELETE" Then
                strPOCode = dt2.Rows(0)("vchPO_Code")
                With oSqlClass
                    .AddParameter("vchPO_Code", dt2.Rows(0)("vchPO_Code"))
                    .AddParameter("vchItem_Code", dt2.Rows(0)("vchItem_Code"))
                    .AddParameter("vchStatus", "CLOSED")
                    .ExecuteQuery("DELETE FROM tblpo_details WHERE vchPO_Code = @vchPO_Code AND vchItem_Code = @vchItem_Code AND vchStatus = @vchStatus")
                    .ClearParameter()
                End With
            End If

            '-- Recalculate Header and Detail
            recalculatePO(oSqlClass, strPOCode)

            '-- Commit
            trTableTransaction.Commit()
            dsObj1.addTable(setMessage("SUCCESS"))
            oSqlClass.Close()
            oSqlClass = Nothing
        Catch ex As Exception
            dsObj1.addTable(setMessage("ERROR: " + ex.Message))
        End Try
        writeObjResponse(dsObj1.getDataset)
    End Sub

    Private Sub recalculatePO(oSqlClass As SQLClass, POCode As String)
        oSqlClass.AddParameter("vchPO_Code", POCode)
        oSqlClass.AddParameter("vchStatus", "CLOSED")
        oSqlClass.ExecuteQuery("Update tblpo_details SET mnyAmount = COALESCE(intQty,0) * COALESCE(mnyPrice,0) WHERE vchPO_Code = @vchPO_Code AND vchStatus = @vchStatus")
        oSqlClass.ExecuteQuery("UPDATE tblpo_headers as A INNER JOIN  (SELECT vchPO_Code,vchStatus,SUM(mnyAmount) as mnyTotOrderAmt FROM tblpo_details GROUP BY vchPO_Code,vchStatus) as B on A.vchPO_Code = B.vchPO_Code AND A.vchStatus = B.vchStatus SET A.mnyTotOrderAmt = B.mnyTotOrderAmt WHERE A.vchPO_Code = @vchPO_Code AND A.vchStatus = @vchStatus")
    End Sub

    <WebMethod()> _
    Public Sub uploadOffset()
        Try
            Dim oSqlClass As New SQLClass(ConfigurationManager.ConnectionStrings(strConnectionString).ConnectionString.ToString)
            Dim trTableTransaction As MySqlTransaction = oSqlClass.SqlConnection_BeginTransaction()
            Context.Response.ContentType = "text/plain"
            Dim js As New JavaScriptSerializer()
            For Each file As String In Context.Request.Files
                Dim hpf As HttpPostedFile = TryCast(Context.Request.Files(file), HttpPostedFile)
                Dim encodedString As String = New StreamReader(hpf.InputStream).ReadToEnd()
                For Each strLine In encodedString.Split(vbCrLf)
                    Dim oCol As String() = strLine.Split(",")
                    If String.IsNullOrEmpty(oCol(0).Trim) Then
                        Exit For
                    End If
                    If oCol(0) = "refnum" Then
                        Continue For
                    End If
                    Dim vchRefnum As String = oCol(0)
                    Dim vchCust_Code As String = Context.Request.Form("hidd_cucode")
                    Dim vchItem_Code As String = oCol(1)
                    Dim intQtyoffset As Decimal = 0
                    intQtyoffset = Math.Abs(CDec(oCol(2)))
                    With oSqlClass
                        .AddParameter("vchRefnum", vchRefnum)
                        .AddParameter("vchCust_Code", vchCust_Code)
                        .AddParameter("vchItem_Code", vchItem_Code)
                        .AddParameter("intQtyOffset", intQtyoffset)
                        .AddParameter("dtRecordAdded", Now.ToString(strdtformat))
                        .ExecuteQuery("INSERT INTO tblstk_offset (vchRefnum,vchCust_Code,vchItem_Code,intQtyOffset,dtRecordAdded) VALUES (@vchRefnum,@vchCust_Code,@vchItem_Code,@intQtyOffset,@dtRecordAdded)")
                        .ClearParameter()
                    End With
                Next
            Next
            recalculateStockMovement(oSqlClass, Context.Request.Form("hidd_cucode")) ' Recalculate Stock Movement
            trTableTransaction.Commit()
            oSqlClass.Close()
            oSqlClass = Nothing
            TryDispose(trTableTransaction)
        Catch ex As Exception
            Throw New System.Exception(ex.Message)
        End Try
    End Sub

    Private Sub recalculateStockMovement(oSqlClass As SQLClass, customer_code As String)
        With oSqlClass
            .AddParameter("vchCust_Code", customer_code)
            .ExecuteQuery(My.Resources.string_recalcStockMovement)
        End With
    End Sub

    Public Sub writeObjResponse(obj As Object)
        Dim json As New JsonSerializerSettings
        json.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        Context.Response.AddHeader("Access-Control-Allow-Origin", "*")
        Context.Response.ContentType = "application/json"
        Context.Response.Write(JsonConvert.SerializeObject(obj, Formatting.None, json))
    End Sub

    Partial Class dsObj
        Private ds As DataSet
        Public Sub New()
            ds = New DataSet
        End Sub
        Public Sub addTable(dt As DataTable)
            ds.Tables.Add(dt.Copy)
        End Sub
        Public Function getDataset() As DataSet
            Return ds
        End Function
    End Class

End Class

Public Class dtSerialize
    Public Property name As String
    Public data As Object
    'IList(Of DataSetService.tblso_headersDataTable)
End Class


', tblMembers_Edit As String, tblMembers_Delete As String
'<WebMethod()> _
'Public Sub syncToServer(tblEmployees As String)
'    Dim strMessage As String = "Sync Successfully"
'    Dim ds As New DataSetService
'    Dim tblAdapter_Employee As New DataSetServiceTableAdapters.tblemployeesTableAdapter
'    Try
'        Dim oEmployees As DataSetService.tblemployeesDataTable = DirectCast(JsonConvert.DeserializeObject(tblEmployees, (GetType(DataSetService.tblemployeesDataTable))), DataSetService.tblemployeesDataTable)
'        For Each oRecord As DataRow In oEmployees.Rows
'            If oRecord("dtype") = "NEW" Then
'                ' NEW RECORD
'                ds.tblemployees.ImportRow(oRecord)
'            ElseIf oRecord("dtype") = "EDIT" Then
'                ' EDIT RECORD
'                ds.tblemployees.LoadDataRow(oRecord.ItemArray, True)
'                ds.tblemployees(ds.tblemployees.Rows.Count - 1).SetModified()
'            ElseIf oRecord("dtype") = "DELETE" Then
'                ' DELETE RECORD
'                ds.tblemployees.LoadDataRow(oRecord.ItemArray, True)
'                ds.tblemployees(ds.tblemployees.Rows.Count - 1).Delete()
'            End If
'            'oRecord("dtype") = ""
'        Next
'        tblAdapter_Employee.Update(ds.tblemployees)
'    Catch ex As Exception
'        strMessage = "Error SyncToServer: " + ex.Message
'    End Try

'    'Dim tblUsers = From t In dbtest.Users
'    'Context.Response.ClearHeaders()
'    'Context.Response.Clear()
'    'Context.Response.ContentType = "application/json"
'    'Context.Response.Write("Ext.util.JSONP.callback(" + JsonConvert.SerializeObject(tblUsers) + ");")

'    ds.Dispose()
'    ds = Nothing

'End Sub


'Private Function Update_SalesOrder(ds As DataSetService, dtheader As DataSetService.tblso_headersDataTable, dtdetail As DataSetService.tblso_detailsDataTable) As String
'    Update_SalesOrder = ""
'    '--- Header
'    If Not dtheader Is Nothing Then
'        Try
'            For Each oRecord As DataRow In dtheader.Rows
'                If oRecord("dtype") = "NEW" Then
'                    ' NEW RECORD
'                    ds.tblso_headers.ImportRow(oRecord)
'                ElseIf oRecord("dtype") = "EDIT" Then
'                    ' EDIT RECORD
'                    ds.tblso_headers.LoadDataRow(oRecord.ItemArray, True)
'                    ds.tblso_headers(ds.tblso_headers.Rows.Count - 1).SetModified()
'                ElseIf oRecord("dtype") = "DELETE" Then
'                    ' DELETE RECORD
'                    ds.tblso_headers.LoadDataRow(oRecord.ItemArray, True)
'                    ds.tblso_headers(ds.tblso_headers.Rows.Count - 1).Delete()
'                End If
'            Next
'        Catch ex As Exception
'            Update_SalesOrder = ex.Message
'            Exit Function
'        End Try
'    End If
'    '-- Detail
'    If Not dtdetail Is Nothing Then
'        Try
'            For Each oRecord As DataRow In dtdetail.Rows
'                If oRecord("dtype") = "NEW" Then
'                    ' NEW RECORD
'                    ds.tblso_details.ImportRow(oRecord)
'                ElseIf oRecord("dtype") = "EDIT" Then
'                    ' EDIT RECORD
'                    ds.tblso_details.LoadDataRow(oRecord.ItemArray, True)
'                    ds.tblso_details(ds.tblso_details.Rows.Count - 1).SetModified()
'                ElseIf oRecord("dtype") = "DELETE" Then
'                    ' DELETE RECORD
'                    ds.tblso_details.LoadDataRow(oRecord.ItemArray, True)
'                    ds.tblso_details(ds.tblso_details.Rows.Count - 1).Delete()
'                End If
'            Next
'        Catch ex As Exception
'            Update_SalesOrder = ex.Message
'            Exit Function
'        End Try
'    End If
'    If Not String.IsNullOrEmpty(Update_SalesOrder) Then
'        Update_SalesOrder = "ERROR: " + Update_SalesOrder
'    End If
'End Function

'Sub SetTableAdapterTransaction(ByVal toAdapter As MySqlDataAdapter, ByVal toTransaction As MySqlTransaction, sqlcls As SQLClass)
'    With toAdapter
'        .InsertCommand.Connection = sqlcls.Sql_Connection
'        .UpdateCommand.Connection = sqlcls.Sql_Connection
'        .DeleteCommand.Connection = sqlcls.Sql_Connection
'        .InsertCommand.CommandTimeout = 720
'        .UpdateCommand.CommandTimeout = 720
'        .DeleteCommand.CommandTimeout = 720
'        .UpdateCommand.UpdatedRowSource = UpdateRowSource.None
'        .InsertCommand.UpdatedRowSource = UpdateRowSource.None
'        .DeleteCommand.UpdatedRowSource = UpdateRowSource.None
'    End With
'    With toAdapter
'        .InsertCommand.Transaction = toTransaction
'        .UpdateCommand.Transaction = toTransaction
'        .DeleteCommand.Transaction = toTransaction
'    End With
'End Sub

'<WebMethod()> _
'Public Sub verifyAccessCode(accesscode As String)
'    Dim tblAdapter_Access As New DataSetServiceTableAdapters.tblacessTableAdapter
'    Dim dtAccess As New DataSetService.tblacessDataTable
'    tblAdapter_Access.Fill(dtAccess, accesscode)
'    Dim jsSettings As New JsonSerializerSettings
'    jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
'    Context.Response.AddHeader("Access-Control-Allow-Origin", "*")
'    Context.Response.ContentType = "application/json"
'    Context.Response.Write(JsonConvert.SerializeObject(dtAccess, Formatting.None, jsSettings))
'End Sub