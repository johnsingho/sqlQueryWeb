using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace sqlQueryWeb.MainLogic
{
    //public delegate void DelegateRefreshTree(IList<string> ls);
    public class SqlConnUtil : IDisposable
    {
        public string DBHost { get; private set; }
        public string DBUser { get; private set; }
        public string DBPassword { get; private set; }
        public string CurDBName { get; set; }


        /// <summary>
        /// 用来连接master数据库
        /// </summary>
        private NativeDBHelper m_nativeDBHelper;

        //private DelegateRefreshTree fnRefreshTree;
        //private Action fnClearTree;
        

        public SqlConnUtil()
        {
            //NativeDBHelper.FnErrorPrompt = FrmMain.PromptError; //报错方式
        }

        public void SetConnectInfo(string dbHost, string dbUser, string dbPassword)
        {
            this.DBHost = dbHost;
            this.DBUser = dbUser;
            this.DBPassword = dbPassword;
        }

        /// <summary>
        /// 尝试连接
        /// </summary>
        /// <returns>是否成功</returns>
        public bool TryConnect()
        {
            if (string.IsNullOrEmpty(DBHost) || string.IsNullOrEmpty(DBUser))
            {
                return false;
            }

            m_nativeDBHelper = new NativeDBHelper(DBHost, DBUser, DBPassword);
            bool bConn = m_nativeDBHelper.IsConnected();
            return bConn;
        }

        public bool IsConnect()
        {
            return m_nativeDBHelper != null && m_nativeDBHelper.IsConnected();
        }

        public IEnumerable<string> GetAllDatabases()
        {
            if(IsConnect())
            {
                //var databases = m_nativeDBHelper.GetAllDatabases();
                var databases = m_nativeDBHelper.GetAllDatabases2(); //test
                var userDBs = from x in databases
                              where !IsSysDatabase(x)
                              select x;
                Debug.Print("table cnt=" + userDBs.Count());
                return userDBs;
            }
            return new List<string>();
        }

        private static readonly string[] mSysDatabases = new[]
            {
                "master",
                "model",
                "msdb",
                "tempdb",
                "distribution"
            };
        private static bool IsSysDatabase(string sDBName)
        {
            return mSysDatabases.Contains(sDBName, StringComparer.InvariantCultureIgnoreCase);
        }

        private void DisConnect()
        {
            if (null != m_nativeDBHelper)
            {
                m_nativeDBHelper.Dispose();
                m_nativeDBHelper = null;
            }
            //if (null != fnClearTree)
            //{
            //    fnClearTree();
            //}
        }

        //public void SetRefreshTreeDelegate(DelegateRefreshTree refreshTree)
        //{
        //    fnRefreshTree = refreshTree;
        //}
        //public void SetClearTreeDelegate(Action clearTree)
        //{
        //    fnClearTree = clearTree;
        //}

        public void Dispose()
        {
            DisConnect();
        }
    }
}