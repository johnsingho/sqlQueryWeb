using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using sqlQueryWeb.MainLogic;

//using RowType = System.Collections.Generic.Dictionary<string, object> ;
using RowType = System.Collections.Generic.List<string>;

namespace sqlQueryWeb.Models
{

    /// <summary>
    /// 代表一个表的所有信息
    /// </summary>
    public class TableInfo
    {
        //写死记录数限制
        public static readonly int QUERY_LIMIT_LINE = 100;

        public TableInfo()
        {
            this.Cols = new List<string>();
            this.Rows = new List<RowType>();
        }

        public List<string> Cols{get;set;}
        public List<RowType> Rows { get; set; }
        public int RowLimit { get; set; }

        /// <summary>
        /// 使用DataTable来填充
        /// </summary>
        /// <param name="dt"></param>
        public void FillBy(DataTable dt)
        {
            Cols.Clear();
            Rows.Clear();
            var regCardNum = new Regex(msRegCardNum, RegexOptions.IgnoreCase);
            var bInitCols = true;
            foreach (DataRow dataRow in dt.Rows)
            {
                var row = new RowType();
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    var sCol = dataColumn.ColumnName;
                    if (!CheckValid(sCol))
                    {
                        continue;
                    }

                    var sCell = dataRow[sCol].ToString();
                    if (regCardNum.IsMatch(sCol))
                    {
                        //chage to base64 string
                        var sTemp = MyCrypt.Encode(sCell, 0x21); //简单加一加密码
                        byte[] bytes = Encoding.ASCII.GetBytes(sTemp ?? string.Empty);
                        string sBase64 = Convert.ToBase64String(bytes);
                        sCell = sBase64;
                    }
                    if (bInitCols)
                    {
                        Cols.Add(sCol);
                    }
                    row.Add(sCell);
                }
                bInitCols = false;
                Rows.Add(row);
            }

            //没有记录集的话，也要输出列名
            if (0 == dt.Rows.Count)
            {
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    var sCol = dataColumn.ColumnName;
                    if (!CheckValid(sCol))
                    {
                        continue;
                    }
                    Cols.Add(sCol);
                }
            }
        }

#region validColName
        //卡号列
        private const string msRegCardNum = @"\s*card_num\s*";
        private static readonly string[] mInvalidSqls = new[]
        {
            "_image",
            "_blob"
        };
        private static bool CheckValid(string strSql)
        {
            var sBadReg = string.Join(@"|", mInvalidSqls);
            sBadReg = String.Format(@"{0}", sBadReg);
            var reg = new Regex(sBadReg, RegexOptions.IgnoreCase);
            if (reg.IsMatch(strSql))
            {
                return false;
            }
            return true;
        }
       
#endregion
    }

}