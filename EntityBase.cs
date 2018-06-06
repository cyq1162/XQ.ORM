using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
namespace XQData
{
    /// <summary>
    /// 实体类基类.表的字段设置/获取值操作类
    /// </summary>
    [Serializable]
    public class EntityBase
    {
        internal Object entity;//实体对象
        internal Type typeInfo;//实体对象类型
        private string configKey;
        /// <summary>
        /// 实体的主键
        /// </summary>
        public string PrimaryKey;
        protected void SetEntity(Object entityInstance, string connConfigName, string primaryKey)
        {
            entity = entityInstance;
            typeInfo = entity.GetType();
            configKey = connConfigName;
            PrimaryKey = primaryKey;
        }
        /// <summary>
        /// 设置实体赋值
        /// </summary>
        /// <param name="entityInstance">传进表的实体</param>
        /// <param name="connConfigName">webconfig的数据库链接配置节点名称</param>
        protected void SetEntity(Object entityInstance, string connConfigName)
        {
            SetEntity(entityInstance, connConfigName, "ID");
        }
        protected void SetEntity(Object entityInstance)
        {
            SetEntity(entityInstance, null,"ID");
        }
        
        /// <summary>
        /// 获取数据库连接
        /// </summary>
        public SqlConnection GetConn()
        {
            return SQLHelper.GetConn(configKey);
        }
        /// <summary>
        /// 将实体的值设置到控件中
        /// </summary>
        /// <param name="isControlEnabled">控件是否可用</param>
        public void SetTo(Control ct, object value, bool isControlEnabled)
        {
            string propName = ct.ID.Substring(3);
            if (value == null)
            {
                value = GetPropertyValue(propName);
            }
            switch (ct.GetType().Name)
            {
                case "TextBox":
                    ((TextBox)ct).Text = Convert.ToString(value);
                    ((TextBox)ct).Enabled = isControlEnabled;
                    break;
                case "Literal":
                    ((Literal)ct).Text = Convert.ToString(value);
                    break;
                case "HiddenField":
                    ((HiddenField)ct).Value = Convert.ToString(value);
                    break;
                case "Label":
                    ((Label)ct).Text = Convert.ToString(value);
                    break;
                case "DropDownList":
                    ((DropDownList)ct).SelectedValue = Convert.ToString(value);
                    ((DropDownList)ct).Enabled = isControlEnabled;
                    break;
                case "CheckBox":
                    bool tempValue;
                    if (Convert.ToString(value) == "1")
                    {
                        tempValue = true;
                    }
                    else
                    {
                        bool.TryParse(Convert.ToString(value), out tempValue);
                    }
                    ((CheckBox)ct).Checked = tempValue;
                    ((CheckBox)ct).Enabled = isControlEnabled;
                    break;
                case "Image":
                    ((Image)ct).ImageUrl=Convert.ToString(value);
                    break;
                case "RadioButtonList":
                    ((RadioButtonList)ct).SelectedValue=Convert.ToString(value);
                    ((RadioButtonList)ct).Enabled = isControlEnabled;
                    break;
            }

        }
        /// <param name="ct">控件</param>
        public void SetTo(Control ct)
        {
            SetTo(ct, null);
        }
        /// <param name="value">自定义值,若此值存在，则不从实体中获取值</param>
        public void SetTo(Control ct, object value)
        {
            SetTo(ct, value, true);
        }
        /// <summary>
        /// 将控件的值设置到实体中[默认从控件中自动获取值]
        /// </summary>
        /// <param name="ct">控件</param>
        /// <param name="value">自定义值,若此值存在，则不从控件中获取值</param>
        public void GetFrom(Control ct, object value)
        {
            string propName = ct.ID.Substring(3);
            if (value == null)
            {
                switch (ct.GetType().Name)
                {
                    case "TextBox":
                        value = ((TextBox)ct).Text.Trim();
                        break;
                    case "Literal":
                        value = ((Literal)ct).Text;
                        break;
                    case "Label":
                        value = ((Label)ct).Text;
                        break;
                    case "DropDownList":
                        value = ((DropDownList)ct).SelectedValue;
                        break;
                    case "CheckBox":
                        value = ((CheckBox)ct).Checked;
                        break;
                    case "Image":
                        value = ((Image)ct).ImageUrl;
                        break;
                    case "RadioButtonList":
                        value = ((RadioButtonList)ct).SelectedValue;
                        break;
                }
            }
            SetPropertyValue(propName, value);
        }
        /// <summary>
        /// 将控件的值设置到实体中
        /// </summary>
        /// <param name="ct">控件</param>
        public void GetFrom(Control ct)
        {
            GetFrom(ct, null);
        }
        /// <summary>
        /// 获取对象指定属性的值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propName">属性名称</param>
        /// <returns></returns>
        internal object GetPropertyValue(string propName)
        {
            PropertyInfo prop = typeInfo.GetProperty(propName);
            return prop.GetValue(entity, null);
        }
        /// <summary>
        /// 设置对象指定属性的值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propName">属性名称</param>
        /// <returns></returns>
        internal void SetPropertyValue(string propName, object value)
        {
            PropertyInfo prop = typeInfo.GetProperty(propName);
            Type valueType = null;
            if (prop.PropertyType.Name.Contains("Nullable"))
            {
                valueType = Type.GetType(prop.PropertyType.FullName.Substring(19, prop.PropertyType.FullName.IndexOf(",") - 19));
            }
            else
            {
                valueType = prop.PropertyType;
            }
            try
            {
                if (valueType.Name != "DateTime" || Convert.ToString(value) != "")
                {
                    if (valueType.Name == "Boolean" && (Convert.ToString(value) == "1" || Convert.ToString(value) == "0"))
                    {
                        value = (Convert.ToString(value) == "1");
                    }
                    else
                    {
                        if (Convert.ToString(value) == "")//如果值为空
                        {
                            switch (valueType.Name)
                            {

                                case "Int16":
                                case "Int32":
                                case "Int64":
                                case "Single":
                                case "Double":
                                case "Decimal":
                                    value = 0;
                                    break;
                            }
                        }
                        value = System.ComponentModel.TypeDescriptor.GetConverter(valueType).ConvertFrom(Convert.ToString(value));
                    }
                    prop.SetValue(entity, value, null);
                }
            }
            catch
            {
            }
        }
    }
}
