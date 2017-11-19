using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sqlQueryWeb.MainLogic
{
    /// <summary>
    /// 转换为jsTree需要的对象
    /// </summary>
    public class JsTreeNode
    {
        public string id { get; set; }
        public string parent { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        
        public static JsTreeNode CreateParent(string sid, string sText, string sIcon)
        {
            return CreateNode("#", sid, sText, sIcon);
        }

        public static JsTreeNode CreateNode(string sParent, string sid, string sText, string sIcon)
        {
            JsTreeNode node = new JsTreeNode();
            node.parent = sParent;
            node.id = sid;
            node.text = sText;
            node.icon = sIcon;
            return node;
        }

        public static JsTreeNode CreateNode(JsTreeNode nodParent, string sid, string sText, string sIcon)
        {
            return CreateNode(nodParent.id, sid, sText, sIcon);
        }
    }
}
