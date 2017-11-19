using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace sqlQueryWeb.MainLogic
{
    public class ModelHelper
    {
        private static readonly string FAKE_TAB_NAME = "数据表";

        // 转换为长json数组
        public static string GetDBTreeModel(SqlConnUtil connector)
        {
            var lDbs = new List<JsTreeNode>();
            foreach (var sdb in connector.GetAllDatabases())
            {
                JsTreeNode nodDB = JsTreeNode.CreateParent(sdb, sdb, null);
                JsTreeNode nodeTab = JsTreeNode.CreateNode(nodDB, FAKE_TAB_NAME, FAKE_TAB_NAME, null);

                lDbs.Add(nodDB);
                lDbs.Add(nodeTab);
            }

            //转换为json字符串
            string jsonModel = new JavaScriptSerializer().Serialize(lDbs.ToArray());
            return jsonModel;
        }
    }
}