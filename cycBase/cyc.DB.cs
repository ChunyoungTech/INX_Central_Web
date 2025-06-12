using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;
using cyc.Data;

namespace cyc.DB
{
    public class SqlDapperConn : IDisposable
    {
        //public const int mID = int.MaxValue;
        //private const string ResultTitle = "DBConnectionCreate";
        public SqlConnection Connection = null;
        public SqlTransaction Transaction = null;
        public int CommandTimeout = 30;
        public string Command { get; set; }
        public object Object { get; set; } = null;
        public ExeResult Result { get; set; }

        public SqlDapperConn(ExeResult oResult = null, string ConnectSting = "", bool IsTransaction = false, int commandTime = 30)
        {
            Result = oResult ?? new ExeResult();
            CommandTimeout = commandTime;
            Connection = new SqlConnection(string.IsNullOrEmpty(ConnectSting) ? ConnString.Main : ConnectSting);
            if (IsTransaction) { Connection.Open(); Transaction = Connection.BeginTransaction(); }
        }

        #region #Query
        public IEnumerable<T> QueryList<T>(string sCommand, object oObj = null)
        {
            Command = sCommand; Object = oObj;
            return Shared.QueryList<T>(this);
        }
        public T QueryOne<T>(string sCommand, object oObj = null)
        {
            Command = sCommand; Object = oObj;
            return Shared.QueryOne<T>(this);
        }
        public SqlMapper.GridReader QueryMultiple(string sCommand, object oObj = null)
        {
            Command = sCommand; Object = oObj;
            return Shared.QueryMultiple(this);
        }
        public DataTable QueryDataTable(string sCommand, object oObj = null)
        {
            Command = sCommand; Object = oObj;
            return Shared.QueryDataTable(this);
        }
        #endregion

        #region #Query With SqlDBPara
        public DataTable QueryDataTable(SqlDBPara oPara)
        {
            return Shared.QueryDataTable(oPara, this);
        }
        public DataSet QueryDataSet(SqlDBPara oPara)
        {
            return Shared.QueryDataSet(oPara, this);
        }
        #endregion

        //Execute
        public int Execute(string sCommand, object oObject = null, int OldID = int.MaxValue)
        {
            Command = sCommand; Object = oObject;
            return Shared.Execute(this, OldID);
        }
        public int Execute(string sCommand, object oObject = null)
        {
            Command = sCommand; Object = oObject;
            return Shared.Execute(this);
        }

        //BulkCopy
        public void BulkCopy(DataTable oDT, string sTable, List<SqlBulkCopyColumnMapping> oMapping = null)
        {
            Shared.BulkCopy(this, oDT, sTable, oMapping);
        }

        #region #Transaction
        public void ResultTransaction()
        {
            if (Transaction != null)
                if (Result.Success) { Transaction.Commit(); } else { Transaction.Rollback(); }
        }
        #endregion

        public void Dispose()
        {
            if (Transaction != null) { Transaction.Dispose(); Transaction = null; }
            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Open) Connection.Close();
                Connection.Dispose(); Connection = null;
            }
        }
    }

    public static class Shared
    {
        #region SqlDBConn
        //public static DataTable QueryDT(SqlDBPara oPara, SqlDBConn oDB = null)
        //{
        //    DataTable oDT = new DataTable();
        //    Query(oPara, oDT, oDB);
        //    return oDT;
        //}
        //public static DataSet QueryDS(SqlDBPara oPara, string sTable = "", SqlDBConn oDB = null)
        //{
        //    DataSet oDS = new DataSet();
        //    Query(oPara, oDS, sTable, oDB);
        //    return oDS;
        //}
        //public static void Query(SqlDBPara oPara, DataTable oDT, SqlDBConn oDB = null)
        //{
        //    bool IsNew = ParaReset(oPara, ref oDB);
        //    try
        //    {
        //        if (oDB.oConn != null)
        //        {
        //            using (SqlDataAdapter oAdp = new SqlDataAdapter(oPara.Command, oDB.oConn))
        //            {
        //                if (oPara.Parameter != null) { oAdp.SelectCommand.Parameters.AddRange(oPara.Parameter.ToArray()); }
        //                if (oDB.oTran != null) { oAdp.SelectCommand.Transaction = oDB.oTran; }
        //                oAdp.Fill(oDT);
        //            }
        //        }
        //    }
        //    catch (Exception ex) { cyc.Log.WriteSysError(ex.Message + ":" + ex.StackTrace, oPara.Result); }
        //    if (IsNew) { oDB.Dispose(); }
        //}
        //public static void Query(SqlDBPara oPara, DataSet oDS, string sTable = "", SqlDBConn oDB = null)
        //{
        //    bool IsNew = ParaReset(oPara, ref oDB);
        //    try
        //    {
        //        if (oDB.oConn != null)
        //        {
        //            using (SqlDataAdapter oAdp = new SqlDataAdapter(oPara.Command, oDB.oConn))
        //            {
        //                if (oPara.Parameter != null) { oAdp.SelectCommand.Parameters.AddRange(oPara.Parameter.ToArray()); }
        //                if (oDB.oTran != null) { oAdp.SelectCommand.Transaction = oDB.oTran; }
        //                if (sTable == "")
        //                    oAdp.Fill(oDS);
        //                else
        //                    oAdp.Fill(oDS, sTable);
        //            }
        //        }
        //    }
        //    catch (Exception ex) { cyc.Log.WriteSysError(ex.Message + ":" + ex.StackTrace); cyc.Log.WriteSysError(ex.Message, oPara.Result); }
        //    if (IsNew) { oDB.Dispose(); }
        //}
        //public static void Execute(SqlDBPara oPara, ref int iID, SqlDBConn oDB = null)
        //{
        //    bool IsNew = ParaReset(oPara, ref oDB);
        //    try
        //    {
        //        if (oDB.oConn != null)
        //        {
        //            if (oDB.oConn.State == ConnectionState.Closed) { oDB.oConn.Open(); }
        //            using (SqlCommand oCmd = new SqlCommand(oPara.Command + ";SELECT CAST(SCOPE_IDENTITY() AS INT);", oDB.oConn))
        //            {
        //                if (oPara.Parameter != null) oCmd.Parameters.AddRange(oPara.Parameter.ToArray());
        //                if (oDB.oTran != null) oCmd.Transaction = oDB.oTran;
        //                iID = Convert.ToInt32(oCmd.ExecuteScalar());
        //            }
        //        }
        //    }
        //    catch (Exception ex) { cyc.Log.WriteSysError(ex.Message + ":" + ex.StackTrace); cyc.Log.WriteSysError(ex.Message, oPara.Result); }
        //    if (IsNew) { oDB.Dispose(); }
        //}
        //public static void Execute(SqlDBPara oPara, SqlDBConn oDB = null)
        //{
        //    bool IsNew = ParaReset(oPara, ref oDB);
        //    try
        //    {
        //        if (oDB.oConn != null)
        //        {
        //            if (oDB.oConn.State == ConnectionState.Closed) { oDB.oConn.Open(); }
        //            using (SqlCommand oCmd = new SqlCommand(oPara.Command, oDB.oConn))
        //            {
        //                if (oPara.Parameter != null) oCmd.Parameters.AddRange(oPara.Parameter.ToArray());
        //                if (oDB.oTran != null) oCmd.Transaction = oDB.oTran;
        //                oCmd.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //    catch (Exception ex) { cyc.Log.WriteSysError(ex.Message + ":" + ex.StackTrace); cyc.Log.WriteSysError(ex.Message, oPara.Result); }
        //    if (IsNew) { oDB.Dispose(); }
        //}
        //private static bool ParaReset(SqlDBPara oPara, ref SqlDBConn oDB)
        //{
        //    if (oPara.Result != null) { oPara.Result.Reset(); }
        //    if (oDB == null) { oDB = new SqlDBConn(); return true; }
        //    return false;
        //}
        #endregion

        #region DapperConnection
        public static IEnumerable<T> QueryList<T>(SqlDapperConn dDB)
        {
            dDB.Result?.Reset();
            try
            {
                return dDB.Connection.Query<T>(dDB.Command, dDB.Object, dDB.Transaction, true, dDB.CommandTimeout);
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorSQL(ex, dDB); }
            //catch (Exception ex) { dDB.Result.ErrorID = cyc.Error.AddError(ex); dDB.Result.Error(); }
            return null;
        }
        public static T QueryOne<T>(SqlDapperConn dDB)
        {
            dDB.Result?.Reset();
            try
            {
                return dDB.Connection.QueryFirstOrDefault<T>(dDB.Command, dDB.Object, dDB.Transaction, dDB.CommandTimeout);
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorSQL(ex, dDB); }
            return default;
        }
        public static SqlMapper.GridReader QueryMultiple(SqlDapperConn dDB)
        {
            dDB.Result?.Reset();
            try
            {
                return dDB.Connection.QueryMultiple(dDB.Command, dDB.Object, dDB.Transaction, dDB.CommandTimeout);
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorSQL(ex, dDB); }
            return null;
        }
        public static DataTable QueryDataTable(SqlDapperConn dDB)
        {
            dDB.Result?.Reset();
            try
            {
                using (IDataReader oReader = dDB.Connection.ExecuteReader(dDB.Command, dDB.Object, dDB.Transaction, dDB.CommandTimeout))
                {
                    using (DataTable oDT = new DataTable())
                    {
                        oDT.Load(oReader);
                        oReader.Close();
                        return oDT;
                    }
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorSQL(ex, dDB); }
            return null;
        }
        public static DataSet QueryDataSet(SqlDBPara oPara, SqlDapperConn dDB)
        {
            dDB.Result?.Reset();
            try
            {
                SqlCommand oCommand = new SqlCommand(oPara.Command, dDB.Connection, dDB.Transaction);
                if (oPara.Parameter != null && oPara.Parameter.Count > 0) { oCommand.Parameters.AddRange(oPara.Parameter.ToArray()); }
                using (SqlDataAdapter oAdapter = new SqlDataAdapter(oCommand))
                {
                    using (DataSet oDS = new DataSet())
                    {
                        oAdapter.Fill(oDS);
                        return oDS;
                    }
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorSQL(ex, dDB); }
            return null;
        }
        public static int Execute(SqlDapperConn dDB, int OldID)
        {
            int NewID = OldID;
            dDB.Result?.Reset();
            try
            {
                if (NewID > 0)
                    dDB.Connection.Execute(dDB.Command, dDB.Object, dDB.Transaction, dDB.CommandTimeout);
                else
                    NewID = dDB.Connection.QuerySingle<int>(dDB.Command + ";SELECT CAST(SCOPE_IDENTITY() AS INT);", dDB.Object, dDB.Transaction, dDB.CommandTimeout);
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorSQL(ex, dDB); }
            return NewID;
        }
        public static int Execute(SqlDapperConn dDB)
        {
            dDB.Result?.Reset();
            try
            {
                return dDB.Connection.Execute(dDB.Command, dDB.Object, dDB.Transaction, dDB.CommandTimeout);
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorSQL(ex, dDB); }
            return 0;
        }
        public static void BulkCopy(SqlDapperConn dDB, System.Data.DataTable oDT, string sTable, List<SqlBulkCopyColumnMapping> oMapping = null)
        {
            dDB.Result?.Reset();
            try
            {
                using (SqlBulkCopy oCopy = new SqlBulkCopy(dDB.Connection, SqlBulkCopyOptions.Default, dDB.Transaction))
                {
                    //設定一個批次量寫入多少筆資料
                    oCopy.BatchSize = 500;
                    //設定逾時的秒數
                    oCopy.BulkCopyTimeout = 60;
                    //設定要寫入的資料庫
                    oCopy.DestinationTableName = sTable;
                    if (oMapping != null)
                    {
                        foreach (SqlBulkCopyColumnMapping mapping in oMapping)
                            oCopy.ColumnMappings.Add(mapping);
                    }
                    //開始寫入
                    if (dDB.Connection.State != ConnectionState.Open) { dDB.Connection.Open(); }
                    oCopy.WriteToServer(oDT);
                }
            }
            catch (Exception ex) { cyc.Log.WriteSysErrorSQL(ex, dDB); }
        }
        #endregion

        #region SqlDapperConn + SqlDBPara (停用SqlDBConn前，暫時折衷作法)
        //public static IEnumerable<T> QueryList<T>(SqlDapperConn dDB, SqlDBPara oPara)
        //{
        //    TransSqlParaToObject(dDB, oPara);
        //    return QueryList<T>(dDB);
        //}
        public static DataTable QueryDataTable(SqlDBPara oPara, SqlDapperConn dDB)
        {
            TransSqlParaToObject(dDB, oPara);
            return QueryDataTable(dDB);
        }
        private static void TransSqlParaToObject(SqlDapperConn dDB, SqlDBPara oPara)
        {
            if (!string.IsNullOrEmpty(oPara.Command)) dDB.Command = oPara.Command;
            dDB.Object = TransSqlParaToObject(oPara.Parameter);
        }
        public static object TransSqlParaToObject(List<SqlParameter> lstPara)
        {
            if (lstPara != null && lstPara.Count > 0)
            {
                var Obj = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;
                foreach (var x in lstPara) Obj.Add(x.ParameterName, x.Value);
                return Obj;
            }
            return null;
        }
        #endregion

        public static void Execute(string sCommand, object oObject = null)
        {
            using (var oDB = new SqlConnection(ConnString.Main))
            {
                try
                {
                    oDB.Execute(sCommand, oObject);
                }
                catch (Exception ex) { cyc.Log.WriteSysErrorLog(ex); }
                finally
                {
                    oDB.Close();
                    oDB.Dispose();
                }
            }
        }
        public static bool CheckNewDB(ref SqlDapperConn oDB)
        {
            if (oDB == null)
            {
                oDB = new SqlDapperConn();
                return true;
            }
            return false;
        }
        public static string GetEditSQL(string sTable, string sColumnAndKey, bool IsInsert)
        {
            string[] sTemp = sColumnAndKey.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            if (sTemp.Length == 2)
                return GetEditSQL(sTable, sTemp[0].Split(','), sTemp[1].Split(','), IsInsert);
            return string.Empty;
        }
        public static string GetEditSQL(string sTable, string[] sColumn, string[] sKey, bool IsInsert)
        {
            if (IsInsert)
                return string.Format("insert into {0} ({1}) values (@{2})", sTable, string.Join(",", sColumn), string.Join(",@", sColumn));
            else
                return string.Format("update {0} set {1} where {2}", sTable, string.Join(",", sColumn.Select(p => string.Format("{0}=@{0}", p))), string.Join(" and ", sKey.Select(p => string.Format("{0}=@{0}", p))));
        }
    }

    public class SqlDBPara
    {
        public string Command { get; set; }
        public List<SqlParameter> Parameter { get; set; }
        public ExeResult Result { get; set; }
        //public object Object { get; set; }
        public SqlDBPara() { Parameter = new List<SqlParameter>(); }
        public void Reset() { this.Command.Remove(0); this.Parameter.Clear(); }
    }

    public class ConnString
    {
        //public static readonly string Main = cyc.Shared.SysQuery.GetConnectString("Main");
        //public static readonly string Main2 = cyc.Shared.SysQuery.GetConnectString("Main2");
        //public static readonly string CIM = cyc.Shared.SysQuery.GetConnectString("CIM");
        //public static readonly string Other = cyc.Shared.SysQuery.GetConnectString("Other");
        //public static readonly string Runtime = cyc.Shared.SysQuery.GetConnectString("Runtime");
        //public static readonly string MApp = cyc.Shared.SysQuery.GetConnectString("MApp");
        //public static readonly string F4CHSC_IACE = cyc.Shared.SysQuery.GetConnectString("F4CHSC_IACE");
        //public static readonly string INXWWALMDB = cyc.Shared.SysQuery.GetConnectString("INXWWALMDB");
        //public static readonly string INX4CDSS = cyc.Shared.SysQuery.GetConnectString("INX4CDSS");
        //public static readonly string INX4CDSA = cyc.Shared.SysQuery.GetConnectString("INX4CDSA");
        public static string Main { get; } = cyc.Shared.SysQuery.GetConnectString("Main");
        public static string Main2 { get; } = cyc.Shared.SysQuery.GetConnectString("Main2");
        public static string CIM { get; } = cyc.Shared.SysQuery.GetConnectString("CIM");
        public static string Other { get; } = cyc.Shared.SysQuery.GetConnectString("Other");
        public static string Runtime { get; } = cyc.Shared.SysQuery.GetConnectString("Runtime");
        public static string MApp { get; } = cyc.Shared.SysQuery.GetConnectString("MApp");
        public static string F4CHSC_IACE { get; } = cyc.Shared.SysQuery.GetConnectString("F4CHSC_IACE");
        public static string INXWWALMDB { get; } = cyc.Shared.SysQuery.GetConnectString("INXWWALMDB");
        public static string INX4CDSS { get; } = cyc.Shared.SysQuery.GetConnectString("INX4CDSS");
        public static string INX4CDSA { get; } = cyc.Shared.SysQuery.GetConnectString("INX4CDSA");
        public static string Report { get; } = cyc.Shared.SysQuery.GetConnectString("Report");
    }
}
