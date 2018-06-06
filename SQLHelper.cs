using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
namespace XQData
{
    /// <summary>
    /// SQL数据库操作帮助类
    /// </summary>
    public class SQLHelper : IDisposable
    {
        private SqlConnection _con = null;
        public static SqlConnection DefaltConn
        {
            get
            {
              return  GetConn(null);
            }
        }
        internal static SqlConnection GetConn(string configKey)
        {
            configKey = string.IsNullOrEmpty(configKey) ? "Conn" : configKey;
            return new SqlConnection(ConfigurationManager.ConnectionStrings[configKey].ConnectionString);
        }
        private SqlCommand com = new SqlCommand(); 
        public bool WriteLog = true;
        private bool outException = true;
        /// <summary>
        ///是否抛出异常
        ///配置AppSettings,Name="OutException", value="true/false"
        /// </summary>
        public bool OutException
        {
            get 
            {
                if (outException)
                {
                    bool.TryParse(Convert.ToString(ConfigurationManager.AppSettings["OutException"]), out outException);
                }
                return outException;
            }
            set
            {
                OutException = value;
            }
        }
        /// <summary>
        ///默认连接字符串Conn
        /// </summary>
        public SQLHelper()
        {
            _con = DefaltConn;
            InitiHelper(_con);
        }
        public SQLHelper(SqlConnection con)
        {
            InitiHelper(con == null ? DefaltConn : con);
        }
        private void InitiHelper(SqlConnection con)
        {
            _con = con;
            com.Connection = con;
        }
        private int returnValue;
        public int ReturnValue
        {
            get
            {
                if (com != null && com.Parameters != null)
                {
                    int.TryParse(Convert.ToString(com.Parameters["ReturnValue"].Value), out returnValue);
                }
                return returnValue;
            }
            set { returnValue = value; }
        }
        #region 方法
        public SqlDataReader ExeDataReader(string procName, bool isProc)
        {
            SetCommandText(procName, isProc);
            SqlDataReader sdr = null;
            try
            {
                OpenCon();
                sdr = com.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (SqlException err)
            {
                Log("ExeDataReader:" + err.Message + com.CommandText);
            }
            return sdr;
        }
        public int ExeNonQuery(string procName, bool isProc)
        {
            SetCommandText(procName, isProc);
            int rowCount = 1;
            try
            {
                OpenCon();
                com.ExecuteNonQuery();
            }
            catch (SqlException err)
            {
                rowCount = 0;
                Log("ExeNonQuery:" + err.Message + com.CommandText);
            }
            return rowCount;
        }
        public object ExeScalar(string procName, bool isProc)
        {
            SetCommandText(procName, isProc);
            object returnValue = null;
            try
            {
                OpenCon();
                returnValue = com.ExecuteScalar();
                if (Convert.ToString(returnValue).Length == 2033)
                {
                    throw new Exception("2033dfdf");
                }
            }
            catch (SqlException err)
            {
                Log("ExeScalar:" + err.Message + com.CommandText);
            }
            return returnValue;
        }
        public string ExeXmlScalar(string procName, bool isProc)
        {
            SetCommandText(procName, isProc);
            string returnValue =string.Empty;
            try
            {
                OpenCon();
                System.Xml.XmlReader xReader = com.ExecuteXmlReader();
                if (xReader != null)
                {
                    xReader.Read();
                    returnValue = xReader.ReadOuterXml();
                    xReader.Close();
                }
            }
            catch (SqlException err)
            {
                Log(err.Message + com.CommandText);
            }
            return returnValue;
        }
        public DataTable ExeDataTable(string procName, bool isProc)
        {
            SetCommandText(procName, isProc);
            SqlDataAdapter sdr = new SqlDataAdapter(com);
            DataTable dataTable = new DataTable("default");
            try
            {
                OpenCon();
                sdr.Fill(dataTable);
            }
            catch (SqlException err)
            {
                if (com.CommandType == CommandType.StoredProcedure)
                {
                    string commandText = string.Format("\r\n存储过程:{0}\r\n", com.CommandText);
                    for (int i = 0; i < com.Parameters.Count; i++)
                    {
                        commandText += string.Format("{0}:{1}\r\n", com.Parameters[i].ParameterName, com.Parameters[i].Value);
                    }
                    Log(err.Message + commandText);
                }
                else
                {
                    Log("ExeDataTable:" + err.Message + com.CommandText);
                }
            }
            finally
            {
                sdr.Dispose();
            }
            return dataTable;
        }
	    /// <summary>
	    /// 将查询结果读到自定义实体中
	    /// </summary>
	    /// <typeparam name="T">实体类型</typeparam>
	    /// <param name="procName">存储过程/sql语句</param>
	    /// <param name="isProc">是否存储过程/否为sql语句</param>
	    /// <returns>返回实体</returns>
        public T GetObject<T>(string procName, bool isProc)
        {
            T obj = (T)Activator.CreateInstance(typeof(T));
            try
            {
                SqlDataReader reader = ExeDataReader(procName, isProc);
                while (reader.Read())
                {
                    ReaderToObject(reader, obj);
                }
                reader.Close();
            }
            catch (SqlException err)
            {
                Log("GetObject:" + err.Message + com.CommandText);
            }
            return obj;
        }
        /// <summary>
        /// 将查询结果读到自定义实体列表中
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="procName">存储过程/sql语句</param>
        /// <param name="isProc">是否存储过程/否为sql语句</param>
        /// <returns>返回实体</returns>
        public List<T> GetObjectList<T>(string procName, bool isProc)
        {
            try
            {
                SqlDataReader reader = ExeDataReader(procName, isProc);
                return GetObjectList<T>(reader);
            }
            catch (SqlException err)
            {
                Log(err.Message + com.CommandText);
            }
            return null;
        }
        private List<T> GetObjectList<T>(IDataReader reader)
        {
            List<T> list = new List<T>();
            try
            {
                while (reader.Read())
                {
                    T obj = (T)Activator.CreateInstance(typeof(T));
                    ReaderToObject(reader, obj);
                    list.Add(obj);
                }
                reader.Close();
            }
            catch (SqlException err)
            {
                Log("GetObjectList:" + err.Message + com.CommandText);
            }
            return list;
        }
        public void ReaderToObject(IDataReader reader, object obj)
        {
            Type valueType = null;
            object value = null;
            string name = null;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                
                name = reader.GetName(i);
                value = reader.GetValue(i);
                System.Reflection.PropertyInfo propertyInfo = obj.GetType().GetProperty(name);//reader.GetName(i));
                if (propertyInfo != null)
                {
                    if (reader.GetValue(i) != DBNull.Value)
                    {
                        try
                        {
                            if (propertyInfo.PropertyType.IsEnum)
                            {
                                propertyInfo.SetValue(obj, Enum.ToObject(propertyInfo.PropertyType, reader.GetValue(i)), null);
                            }
                            else
                            {
                                if (propertyInfo.PropertyType.Name.Contains("Nullable"))
                                {
                                    valueType = Type.GetType(propertyInfo.PropertyType.FullName.Substring(19, propertyInfo.PropertyType.FullName.IndexOf(",") - 19));
                                }
                                else
                                {
                                    valueType = propertyInfo.PropertyType;
                                }
                                if (valueType.Name != "DateTime" || Convert.ToString(value) != "")
                                {
                                    if (valueType.Name == "Boolean" && (Convert.ToString(value)=="1" || Convert.ToString(value)=="0"))
                                    {
                                        value = (Convert.ToString(value) == "1");
                                    }
                                    else
                                    {
                                        value = System.ComponentModel.TypeDescriptor.GetConverter(valueType).ConvertFrom(Convert.ToString(value));
                                    }
                                    propertyInfo.SetValue(obj, value, null);
                                }
                            }
                        }
                        catch (SqlException err)
                        {
                            Log("ReaderToObject:" + err.Message);
                        }
                    }
                }
            }
        }
        #endregion

        public void AddParameter(string parameterName, object value)
        {
            if (com.Parameters.IndexOf(parameterName) < 0)
                com.Parameters.AddWithValue(parameterName, value);
            else
                com.Parameters[parameterName].Value = value;
        }
        public void AddParameter(string parameterName, object value, SqlDbType sqlDbType)
        {
            if (com.Parameters.IndexOf(parameterName) < 0)
                com.Parameters.Add(parameterName, sqlDbType).Value = value;
            else
                com.Parameters[parameterName].Value = value;
        }
        public void AddParameter(string ArgName, object ArgValue, DbType ArgDbType, int ArgSize, bool ArgIsOut)
        {
            if (com.Parameters.IndexOf(ArgName) < 0)
            {
                SqlParameter ObjParameter = new SqlParameter();
                ObjParameter.ParameterName = ArgName;
                ObjParameter.Value = ArgValue;
                ObjParameter.DbType = ArgDbType;
                if (ArgSize > 0) ObjParameter.Size = ArgSize;
                if (ArgIsOut) ObjParameter.Direction = ParameterDirection.Output;
                com.Parameters.Add(ObjParameter);
            }
        }
        public void ClearParameter()
        {
            if (com != null && com.Parameters != null)
            {
                com.Parameters.Clear();
            }
        }
        private void SetCommandText(string commandText, bool isProc)
        {
            com.CommandText = commandText;
            if (isProc)
            {
                com.CommandType = CommandType.StoredProcedure;
            }
            else
            {
                com.CommandType = CommandType.Text;
            }
            if (!com.Parameters.Contains("ReturnValue"))
            {
                com.Parameters.Add("ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
            }
            //if (!com.Parameters.Contains("OutPutValue"))
            //{
            //    com.Parameters.Add("OutPutValue", SqlDbType.NVarChar,50).Direction = ParameterDirection.Output;
            //}
        }

        #region IDisposable

        public void Dispose()
        {
            if (_con != null)
            {
                CloseCon();
                _con = null;
            }
            if (com != null)
            {
                com = null;
            }
        }
        private void OpenCon()
        {
            try
            {
                if (_con.State == ConnectionState.Closed)
                {
                    _con.Open();
                }
            }
            catch (SqlException err)
            {
                Log(err.Message);
            }

        }
        private void CloseCon()
        {
            try
            {
                if (_con.State == ConnectionState.Open)
                {
                    _con.Close();
                }
            }
            catch (SqlException err)
            {
                Log(err.Message);
            }

        }
        private void Log(string errMessage)
        {
            if (WriteLog)
            {
                SQLLog.WriteLog(errMessage);
            }
            if (OutException)
            {
                throw new Exception("SQL.Log:"+errMessage);
            }
        }
        #endregion
    }
}
