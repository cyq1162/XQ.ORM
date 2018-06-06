using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.Web.UI.WebControls;

namespace XQData
{
    class SQLData<T> : ICommon<T>
    {
        private SQLHelper helper = null;
        #region ICommon<T> 成员
        private string tableName;
        /// <summary>
        /// 数据操作表名
        /// </summary>
        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }
        private EntityBase entity;
        public SQLData()
        {
            entity = Activator.CreateInstance<T>() as EntityBase;
            helper = new SQLHelper(entity.GetConn());
            tableName = entity.typeInfo.Name;
            tableName = tableName.Substring(0, tableName.LastIndexOf("Bean"));
        }
        public bool Add(T entity)
        {
            object result = helper.ExeScalar(GetSql(SqlTypeEnum.Add, entity, ref helper, null), false);
            if (result != null)
            {
                EntityBase eb = entity as EntityBase;
                if (eb.GetPropertyValue(eb.PrimaryKey) == null)
                {
                    eb.SetPropertyValue(eb.PrimaryKey, result);
                }
            }
            helper.Dispose();
            return result != null;
        }
        public bool Update(T entity)
        {
            return Update(entity, null);
        }
        public bool Update(T entity, string where)
        {
            int result = helper.ExeNonQuery(GetSql(SqlTypeEnum.Update, entity, ref helper, where), false);
            helper.Dispose();
            return result > 0;
        }
        private enum SqlTypeEnum
        {
            Add,
            Update,
            Delete
        }
        private string GetSql(SqlTypeEnum sqlTypeEnum, T entity, ref SQLHelper helper, string where)
        {
            string addSql = null, addSql2 = null, updateSql = null;
            PropertyInfo[] pis = typeof(T).GetProperties();
            string propName;
            object propValue;
            EntityBase eb = entity as EntityBase;
            for (int i = 0; i < pis.Length; i++)
            {
                propName = pis[i].Name;
                if (sqlTypeEnum == SqlTypeEnum.Update && propName == eb.PrimaryKey)
                {
                    continue;
                }
                propValue = pis[i].GetValue(entity, null);
                if (propValue == null) { continue; }
                helper.AddParameter("@" + propName, propValue);
                if (sqlTypeEnum == SqlTypeEnum.Add)
                {
                    addSql += "[" + propName + "],";
                    addSql2 += "@" + propName + ",";
                }
                else if (sqlTypeEnum == SqlTypeEnum.Update)
                {
                    updateSql += "[" + propName + "]=@" + propName + ",";
                }
            }
            if (sqlTypeEnum == SqlTypeEnum.Add)
            {
                return "insert into " + tableName + "(" + addSql.TrimEnd(',') + ") values(" + addSql2.TrimEnd(',') + ") select cast(scope_Identity() as int) as OutPutValue";
            }
            else
            {
                    propName = eb.PrimaryKey;
                    propValue = eb.GetPropertyValue(eb.PrimaryKey);
                    if (where == null)
                    {
                        helper.AddParameter("@" + propName, propValue);
                    }
                return "update " + tableName + " set " + updateSql.TrimEnd(',') + " where " + (where == null ? (propName + "=@" + propName) : where);
            }
        }
        private string FormatWhere(object beFormatWhere)
        {
            string where = Convert.ToString(beFormatWhere).ToLower();
            if (where.IndexOfAny(new char[] { '=', '>', '<' }) == -1 && !where.Contains("like") && !where.Contains("between") && !where.Contains("in"))
            {
                return entity.PrimaryKey + "=" + where;
            }
            return where;
        }
        public bool Delete(object where)
        {
            int result = helper.ExeNonQuery("delete from " + tableName + " where " + FormatWhere(where), false);
            helper.Dispose();
            return result > 0;
        }
        public T Select(object where)
        {
            return Select(where, string.Empty);
        }
        public T Select(object where, string customTableName)
        {
            int count;
            List<T> list = SelectList(1, 1, FormatWhere(where), out count, customTableName);
            if (list != null && list.Count > 0)
            {
                return list[0];
            }
            return default(T);
        }
        public List<T> SelectList(int pageIndex, int pageSize, string where, out int count)
        {
            return SelectList(pageIndex, pageSize, where, out count, string.Empty);
        }
        public List<T> SelectList(int pageIndex, int pageSize, string where, out int count, string customTableName)
        {
            helper.AddParameter("@PageIndex", pageIndex);
            helper.AddParameter("@PageSize", pageSize);
            helper.AddParameter("@TableName", customTableName == string.Empty ? tableName : customTableName);
            helper.AddParameter("@where", where);
            List<T> list = helper.GetObjectList<T>("SelectBase", true);
            count = helper.ReturnValue;
            helper.Dispose();
            return list;
        }

        public System.Data.DataTable SelectDataTable(int pageIndex, int pageSize, string where, out int count)
        {
            return SelectDataTable(pageIndex, pageSize, where, out count, string.Empty);
        }

        public System.Data.DataTable SelectDataTable(int pageIndex, int pageSize, string where, out int count, string customTableName)
        {
            helper.AddParameter("@PageIndex", pageIndex);
            helper.AddParameter("@PageSize", pageSize);
            helper.AddParameter("@Where", where);
            helper.AddParameter("@TableName", customTableName == string.Empty ? tableName : customTableName);
            DataTable dt = helper.ExeDataTable("SelectBase", true);
            count = helper.ReturnValue;
            dt.TableName = customTableName == string.Empty ? tableName : customTableName.Substring(customTableName.LastIndexOf(')') + 1).Trim();
            helper.Dispose();
            return dt;
        }

        public int GetCountByWhere(string where)
        {
            if (!string.IsNullOrEmpty(where)) { where = " where " + where; }
            object result = helper.ExeScalar("select count(*) from " + tableName + where, false);
            helper.Dispose();
            return result == null ? 0 : Convert.ToInt32(result);
        }

        public void BindListControl(System.Web.UI.WebControls.ListControl listControl, string textFiled, string valueFiled, string where, bool isAddPleaseSlect)
        {
            if (!string.IsNullOrEmpty(where)) { where = " where " + where; }
            DataTable dt = helper.ExeDataTable("select distinct " + textFiled + "," + valueFiled + " from " + tableName + " " + where, false);
            helper.Dispose();
            listControl.DataSource = dt;
            listControl.DataTextField = textFiled;
            listControl.DataValueField = valueFiled;
            listControl.DataBind();
            if (isAddPleaseSlect)
            {
                listControl.Items.Insert(0, new ListItem("请选择", ""));
            }
        }

        #endregion
    }
}
