using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace sqlQueryWeb.MainLogic
{
    public class NativeDBHelper : IDisposable
    {
        private string _con_str = "server={0};  database={1};  uid={2};  pwd={3};Pooling=false";
        private string _database = null;
        private SqlConnection _conn = null;
        public static Action<string> FnErrorPrompt { set; private get; }

        private const string SQL_COMMAND_QUERY_DATABASES = "select name from sysdatabases order by name";
        private const string SQL_COMMAND_QUERY_ALL_TABLES = "select name from sys.tables order by name";
        private const string SQL_COMMAND_QUERY_ALL_PROCEDURE = "select name from sys.procedures  order by name";

        private const string SQL_COMMAND_QUERY_ALL_COLUMENS =
            "select c.name, c.max_length, t.name from sys.columns c left join sys.tables on c.object_id=sys.tables.object_id left join sys.types t on c.user_type_id=t.user_type_id where sys.tables.name='{0}'";

        private const string SQL_COMMAND_GET_DATA_FROM_TABLE = "select * from {0}";
        private const string SQL_COMMAND_GET_DATA_FROM_TABLE_TOP = "SELECT TOP {0} * from {1}";

        public NativeDBHelper(string server, string user, string paswd, string database)
        {
#if false
            //USE_LOCAL_DB
            //server=(localDB)\hedb;database=FavLink;
            _con_str = string.Format("server={0};database={1}", server, database);
#else
            _con_str = string.Format(_con_str, server, database, user, paswd);
#endif

            _database = database;
            try
            {
                _conn = new SqlConnection(_con_str);
                _conn.Open();
            }
            catch (System.Exception ex)
            {
                _conn = null;
                //Console.WriteLine(ex.Message);
                ErrorPrompt(ex.Message);
            }
        }


        private void ErrorPrompt(string str)
        {
            Debug.Print(str);
            if (null != FnErrorPrompt)
            {
                FnErrorPrompt(str);
            }
        }

        public bool IsConnected()
        {
            bool bConn = (null != _conn) && (
                ConnectionState.Open == (_conn.State & ConnectionState.Open)
                );
            return bConn;
        }

        public NativeDBHelper(string con_str)
        {
            _conn = new SqlConnection(con_str);
            _conn.Open();
            _database = _conn.Database;
        }

        public NativeDBHelper(string server, string user, string paswd)
            : this(server, user, paswd, "master")
        {
        }

        public NativeDBHelper(SqlConnection dbcon, string dbName)
        {
            _conn = dbcon;
            _database = dbName;
        }

        private void GetObjects(string sql, Action<SqlDataReader> ac)
        {
            try
            {
                //_conn.Open();
                using (SqlCommand cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ac(reader);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorPrompt(ex.Message);
                //Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 原始的，通过查询系统表来枚举数据库
        /// </summary>
        /// <returns></returns>
        public IList<string> GetAllDatabases()
        {
            List<string> retval = new List<string>();

            GetObjects(SQL_COMMAND_QUERY_DATABASES, (reader) =>
            {
                retval.Add(reader.GetString(0));
            });

            return retval;
        }

        /// <summary>
        /// 通过SMO枚举数据库
        /// </summary>
        /// <returns></returns>
        public IList<string> GetAllDatabases2()
        {
            List<string> retval = new List<string>();
            Server srv = default(Server);
            ServerConnection scon = new ServerConnection(_conn);
            srv = new Server(scon);
            foreach (Database db in srv.Databases)
            {
                retval.Add(db.Name);
            }

            return retval;
        }


        public IList<string> GetTables()
        {
            List<string> retval = new List<string>();
            GetObjects(SQL_COMMAND_QUERY_ALL_TABLES, (reader) =>
            {
                retval.Add(reader.GetString(0));
            });

            return retval;
        }

        public IList<string> GetProcedures()
        {
            List<string> retval = new List<string>();
            GetObjects(SQL_COMMAND_QUERY_ALL_PROCEDURE, (r) => { retval.Add(r.GetString(0)); });
            return retval;
        }

        public string GetTableScript(string table)
        {
            string retval = "";
            try
            {
                ServerConnection scon = new ServerConnection(_conn);
                Server server = new Server(scon);
                foreach (string sql in server.Databases[_database].Tables[table].Script())
                {
                    retval += " " + sql;
                }
            }
            catch
            {
            }
            return retval;
        }

        public static List<string> GetPrimaryKeys(DbConnection con, string table)
        {
            List<string> pks = new List<string>();
            ServerConnection scon = new ServerConnection(con as SqlConnection);
            Server server = new Server(scon);
            foreach (var item in server.Databases[con.Database].Tables[table].Columns)
            {
                if ((item as Column).InPrimaryKey)
                {
                    pks.Add((item as Column).Name);
                }
            }
            return pks;
        }

        public static List<string> GetColumns(DbConnection con, string table)
        {
            List<string> cls = new List<string>();
            ServerConnection scon = new ServerConnection(con as SqlConnection);
            Server server = new Server(scon);
            foreach (var item in server.Databases[con.Database].Tables[table].Columns)
            {
                cls.Add((item as Column).Name);
            }
            return cls;
        }

        /// <summary>
        /// add by weilin 2016-05-18
        /// 获取数据表的字段和字段类型信息
        /// </summary>
        /// <param name="con"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetColumnsObj(DbConnection con, string table)
        {
            Dictionary<string, string> obj_cls = new Dictionary<string, string>();
            ServerConnection scon = new ServerConnection(con as SqlConnection);
            Server server = new Server(scon);
            foreach (var item in server.Databases[con.Database].Tables[table].Columns)
            {
                obj_cls.Add((item as Column).Name, (item as Column).DataType.Name);
            }
            return obj_cls;
        }

        public static List<string> GetIdentityColumns(DbConnection con, string table)
        {
            List<string> cls = new List<string>();
            ServerConnection scon = new ServerConnection(con as SqlConnection);
            Server server = new Server(scon);
            foreach (var item in server.Databases[con.Database].Tables[table].Columns)
            {
                if ((item as Column).Identity)
                {
                    cls.Add((item as Column).Name);
                }
            }
            return cls;
        }

        public List<string> GetPrimaryKeys(string table)
        {
            List<string> pKeys = new List<string>();
            try
            {
                ServerConnection scon = new ServerConnection(_conn);
                Server server = new Server(scon);
                foreach (Column c in server.Databases[_database].Tables[table].Columns)
                {
                    if (c.InPrimaryKey)
                    {
                        pKeys.Add(c.ToString());
                    }
                }
            }
            catch
            {
            }

            return pKeys;
        }

        public string GetProcedureScript(string Name)
        {
            string retval = "";
            ServerConnection scon = new ServerConnection(_conn);
            Server server = new Server(scon);

            int count = server.Databases[_database].StoredProcedures[Name].Script().Count;
            var cls = server.Databases[_database].StoredProcedures[Name].Script();
            retval = cls[count - 1];

            return retval;
        }

        public bool IsTableExist(string tableName)
        {
            ServerConnection scon = new ServerConnection(_conn);
            Server server = new Server(scon);
            List<string> columns = new List<string>();
            if (server.Databases[_database].Tables[tableName] == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// add by weilin 2016-05-20
        /// 判断某个数据库的某张数据表是否存在
        /// </summary>
        /// <param name="con"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool IsTableExistEx(DbConnection con, string tableName)
        {
            ServerConnection scon = new ServerConnection(con as SqlConnection);
            Server server = new Server(scon);
            List<string> columns = new List<string>();
            if (server.Databases[con.Database].Tables[tableName] == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 使用SMO来枚举列
        /// </summary>
        /// <param name="Table"></param>
        /// <returns></returns>
        public List<string> GetColumnsEx(string Table)
        {
            ServerConnection scon = new ServerConnection(_conn);
            Server server = new Server(scon);
            List<string> columns = new List<string>();
            if (_database == null || server.Databases[_database].Tables[Table] == null)
            {
                return columns;
            }

            foreach (var item in server.Databases[_database].Tables[Table].Columns)
            {
                columns.Add((item as Column).Name);
            }
            return columns;
        }

        public IList<Tuple<string, string, string>> GetColumns(string table)
        {
            List<Tuple<string, string, string>> retval = new List<Tuple<string, string, string>>();
            string sql = string.Format(SQL_COMMAND_QUERY_ALL_COLUMENS, table);
            GetObjects(sql, (reader) =>
            {
                retval.Add(new Tuple<string, string, string>(reader.GetString(0), reader.GetInt16(1).ToString(),
                    reader.GetString(2)));
            });
            return retval;
        }

        public void Dispose()
        {
            if (_conn != null)
            {
                _conn.Dispose();
                _conn = null;
            }
        }

        /// <summary>
        /// 取一个表的所有记录
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public DataTable GetDataFromTable(string table)
        {
            string sql = string.Format(SQL_COMMAND_GET_DATA_FROM_TABLE, table);
            try
            {
                if (!IsConnected())
                {
                    _conn.Open();
                }
                using (DataSet ds = new DataSet())
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, _conn);
                    adapter.Fill(ds);
                    DataTable dt = ds.Tables[0];
                    dt.TableName = table;
                    return dt;
                }
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine(ex.Message);
                ErrorPrompt(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 取一个表的前N条记录
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="nRec">记录数</param>
        /// <returns></returns>
        public DataTable GetDataFromTableTopN(string table, int nRec)
        {
            string sql = string.Format(SQL_COMMAND_GET_DATA_FROM_TABLE_TOP, nRec, table);
            try
            {
                if (!IsConnected())
                {
                    _conn.Open();
                }
                using (DataSet ds = new DataSet())
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(sql, _conn))
                    {
                        adapter.Fill(ds);
                        DataTable dt = ds.Tables[0];
                        dt.TableName = table;
                        return dt;
                    }
                }
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine(ex.Message);
                ErrorPrompt(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 用来执行任意SQL
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public DataTable Execute(string strSql)
        {
            try
            {
                if (!IsConnected())
                {
                    _conn.Open();
                }

                using (SqlCommand cmd = new SqlCommand(strSql, _conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }

            }
            catch (System.Exception ex)
            {
                //Console.WriteLine(ex.Message);
                ErrorPrompt(ex.Message);
                return null;
            }
        }
    }
}