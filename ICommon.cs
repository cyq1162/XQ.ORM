using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
namespace XQData
{
    /// <summary>
    /// 数据操作公共接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommon<T>
    {
        /// <summary>
        /// 数据库表名
        /// </summary>
        string TableName { get;set;}
        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        bool Add(T entity);
        /// <summary>
        /// 更新记录[没有where条件时,默认以数据库第一列为关键字做为条件更新]
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        bool Update(T entity);
        /// <param name="where">更新条件如：id=1</param>
        bool Update(T entity, string where);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="where">条件：在表只有一个关键字的情况下，可直接传关键字的值</param>
        bool Delete(object where);
        /// <summary>
        /// 查询一条记录
        /// </summary>
        /// <param name="where">条件：在表只有一个关键字的情况下，可直接传关键字的值</param>
        T Select(object where);
        /// <param name="customTableName">自定义表名：可构造多表查询的视图字符串：如构造："(select aa from tableA A,tableB B where A.ID=B.ID ) view"</param>
        T Select(object where,string customTableName);
        /// <summary>
        /// 查询列表集合[查询所有时:pageIndex和pageSize请都设置为0]
        /// </summary>
        /// <param name="where">条件：如id>1</param>
        /// <param name="count">返回的记录总数</param>
        List<T> SelectList(int pageIndex, int pageSize, string where, out int count);
        /// <param name="tableName">自定义表名：可构造多表查询的视图字符串：如构造："(select aa from tableA A,tableB B where A.ID=B.ID ) view"</param>
        List<T> SelectList(int pageIndex, int pageSize, string where, out int count, string customTableName);
        /// <summary>
        /// 查询列表集合[查询所有时:pageIndex和pageSize请都设置为0]
        /// </summary>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页几条</param>
        /// <param name="where">条件：如id>1</param>
        /// <param name="count">返回的记录总数</param>
        DataTable SelectDataTable(int pageIndex, int pageSize, string where, out int count);

        /// <param name="tableName">自定义表名：可构造多表查询的视图字符串：如构造："(select aa from tableA A,tableB B where A.ID=B.ID ) view"</param>
        DataTable SelectDataTable(int pageIndex, int pageSize, string where, out int count, string customTableName);
        /// <summary>
        /// 查询记录总数
        /// </summary>
        /// <param name="where">条件：如id>1</param>
        int GetCountByWhere(string where);
        /// <summary>
        ///  绑定列表方式的控件：如DropDownList,等继承自System.Web.UI.WebControls.ListControl的控件
        /// </summary>
        /// <param name="listControl">控件如DropDownList</param>
        /// <param name="textFiled">文本域列</param>
        /// <param name="valueFiled">值域列</param>
        /// <param name="where">查询条件</param>
        /// <param name="isAddPleaseSlect">是否添加"请选择[值为空]"项,</param>
        void BindListControl(System.Web.UI.WebControls.ListControl listControl, string textFiled, string valueFiled, string where, bool isAddPleaseSlect);
    }
}
