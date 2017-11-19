using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using sqlQueryWeb.MainLogic;
using sqlQueryWeb.Models;

namespace sqlQueryWeb.Controllers
{
    public class QueryController : Controller
    {
        private static readonly string KEY_LOGINFO = "LogInfo";

        // GET: query/Index
        [Authorize(Roles = "admins")]
        public ActionResult Index()
        {
            return View();
        }
        
        //
        // GET: /query/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            //ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /query/Login
        [HttpPost]
        public ActionResult Login(ConnInfo connInfo)
        {
            bool bConn = false;
            var ret = new QueryResult();
            SqlConnUtil connector = null;
            if (connInfo != null)
            {
                connector = new SqlConnUtil();
                connector.SetConnectInfo(connInfo.host, connInfo.user, connInfo.pass);
                bConn = connector.TryConnect();
            }

            if (bConn)
            {
                Session[KEY_LOGINFO] = connector;
                //return RedirectToAction("QueryPage");
                ret.retCode = true;
                ret.sData = "QueryPage";
                return Json(ret);
            }
            else
            {
                ModelState.AddModelError("", "无法登录到目标数据库。");
                //return Json(Url.Action("Index"));
                ret.retCode = false;
                ret.sData = "Index";
                return Json(ret);
            }
            //return View();
        }

        // GET
        public ActionResult QueryPage()
        {
            SqlConnUtil connInfo = Session[KEY_LOGINFO] as SqlConnUtil;
            if (connInfo == null)
            {
                return RedirectToAction("Index");
            }
            return View(connInfo);
        }

        //
        // POST: /query/GetAllTables
        [HttpPost]
        public ActionResult GetAllTables(string sDBname)
        {
            var ret = new QueryResult();
            ret.retCode = false;
            SqlConnUtil connInfo = Session[KEY_LOGINFO] as SqlConnUtil;
            if (connInfo == null)
            {
                return Json(ret);
            }

            //取数据库的所有用户表
            using (
                var dbConn = new NativeDBHelper(connInfo.DBHost, connInfo.DBUser, connInfo.DBPassword, sDBname)
                )
            {
                var lsTabs = dbConn.GetTables();
                var jss = new JavaScriptSerializer();
                ret.sData = jss.Serialize(lsTabs);
                ret.retCode = true;
            }
            
            return Json(ret);
        }

        [HttpGet]
        public ActionResult GetTabData(string sDBname, string sTable)
        {
            var tinfo = new TableInfo();
            SqlConnUtil connInfo = Session[KEY_LOGINFO] as SqlConnUtil;
            if (null == connInfo)
            {
                return View(tinfo);
            }

            //取数据库的所有用户表
            using (
                var dbConn = new NativeDBHelper(connInfo.DBHost, connInfo.DBUser, connInfo.DBPassword, sDBname)
                )
            {
                var dt = dbConn.GetDataFromTableTopN(sTable, TableInfo.QUERY_LIMIT_LINE);
                if (null == dt)
                {
                    return View(tinfo);
                }

                tinfo.FillBy(dt);
            }

            return View(tinfo);
        }

#region old code
        //
        // POST: /query/GetTabData
        //[HttpPost]
        //public ActionResult GetTabData(string sDBname, string sTable)
        //{
        //    var ret = new QueryResult();
        //    ret.retCode = false;
        //    SqlConnUtil connInfo = Session[KEY_LOGINFO] as SqlConnUtil;
        //    if (null==connInfo)
        //    {
        //        return Json(ret);
        //    }

        //    //取数据库的所有用户表
        //    using (
        //        var dbConn = new NativeDBHelper(connInfo.DBHost, connInfo.DBUser, connInfo.DBPassword, sDBname)
        //        )
        //    {
        //        var dt = dbConn.GetDataFromTableTopN(sTable, QUERY_LIMIT_LINE);
        //        if (null == dt)
        //        {
        //            return Json(ret);    
        //        }
        //        ret.sData = DatatableToJson(dt);
        //        ret.retCode = true;

        //        Debug.Print("recs = {0}\n", dt.Rows.Count);
        //        Debug.Print(ret.sData);
        //    }
        //    return Json(ret);
        //}

        //#region validColName
        //private static readonly string[] mInvalidSqls = new[]
        //{
        //    "_image",
        //    "_blob"
        //};
        //private static bool CheckValid(string strSql)
        //{
        //    var sBadReg = string.Join(@"|", mInvalidSqls);
        //    sBadReg = String.Format(@"{0}", sBadReg);
        //    var reg = new Regex(sBadReg, RegexOptions.IgnoreCase);
        //    if (reg.IsMatch(strSql))
        //    {
        //        return false;
        //    }
        //    return true;
        //}
        
        ////卡号列
        //private const string msRegCardNum = @"\s*card_num\s*";
        //#endregion

        ///// <summary>
        ///// DataTable 转换为Json字符串
        ///// </summary>
        //public static string DatatableToJson(DataTable dt)
        //{
        //    var javaScriptSerializer = new JavaScriptSerializer();
        //    javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
        //    var arrayList = new ArrayList();
        //    Regex regCardNum = new Regex(msRegCardNum, RegexOptions.IgnoreCase);
        //    foreach (DataRow dataRow in dt.Rows)
        //    {
        //        var dictionary = new Dictionary<string, object>();
        //        foreach (DataColumn dataColumn in dt.Columns)
        //        {
        //            var sCol = dataColumn.ColumnName;
        //            if (!CheckValid(sCol))
        //            {
        //                continue;
        //            }
                    
        //            var sCell = dataRow[sCol].ToString();
        //            if (regCardNum.IsMatch(sCol))
        //            {
        //                //chage to base64 string
        //                var sTemp = MyCrypt.Encode(sCell, 0x21); //简单加一加密码
        //                byte[] bytes = Encoding.ASCII.GetBytes(sTemp ?? string.Empty);
        //                string sBase64 = Convert.ToBase64String(bytes);
        //                sCell = sBase64;
        //            }
        //            dictionary.Add(sCol, sCell);
        //        }
        //        arrayList.Add(dictionary);
        //    }

        //    //如果没有记录行的话，也将列名返回
        //    if (0 == dt.Rows.Count)
        //    {
        //        var dictionary = new Dictionary<string, object>();
        //        foreach (DataColumn dataColumn in dt.Columns)
        //        {
        //            dictionary.Add(dataColumn.ColumnName, string.Empty);
        //        }
        //        arrayList.Add(dictionary);
        //    }

        //    return javaScriptSerializer.Serialize(arrayList);
        //}
#endregion
    }
}
