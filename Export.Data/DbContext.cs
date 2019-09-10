using Export.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Export.Data
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public class DbContext : IDbContext
    {
        /// <summary>
        /// sql执行
        /// </summary>
        private ISqlExecute sqlExecute;

        /// <summary>
        /// 缓存表
        /// </summary>
        private static readonly Dictionary<Type, string> tables = new Dictionary<Type, string>();

        /// <summary>
        /// 缓存表相关字段
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, Type>> tableFileds = new Dictionary<string, Dictionary<string, Type>>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="sqlExecute"></param>
        public DbContext(ISqlExecute sqlExecute)
        {
            this.sqlExecute = sqlExecute;
        }

        /// <summary>
        /// 初始化实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDbContext Set<T>()
        {
            StringBuilder stringBuilder = new StringBuilder();
            Type t = typeof(T);
            if (!tables.ContainsKey(t))
            {
                string tableName = t.Name;
                TableAttribute tableAttribute = t.GetCustomAttribute<TableAttribute>();
                if (tableAttribute != null)
                {
                    tableName = tableAttribute.Name;
                }
                tables.Add(t, tableName);
                string fieldName = string.Empty;
                TableColumnAttribute tableColumnAttribute = null;
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList().ForEach(x =>
                 {
                     tableColumnAttribute = x.GetCustomAttribute<TableColumnAttribute>();
                     if (!tableFileds.ContainsKey(tableName))
                     {
                         tableFileds.Add(tableName, new Dictionary<string, Type>());
                     }
                     fieldName = x.Name;
                     if (tableColumnAttribute != null)
                     {
                         fieldName = tableColumnAttribute.Name;
                     }
                     tableFileds[tableName].Add(fieldName, x.PropertyType);
                 });
            }
            stringBuilder.AppendFormat("if object_id('{0}') is null", tables[t]);
            stringBuilder.AppendFormat("create table {0}(", tables[t]);
            foreach (var item in tableFileds[tables[t]])
            {
                stringBuilder.AppendFormat("{0} {1},", item.Key, GetDbType(item.Value));
            }
            if (stringBuilder.ToString().IndexOf(',') > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append(")");
            sqlExecute.ExecuteSql(stringBuilder.ToString());
            return this;
        }

        /// <summary>
        /// 获取数据库类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetDbType(Type type)
        {
            string dbType = "nvarchar(400)";
            switch (type.Name)
            {
                case "System.String":
                    dbType = "nvarchar(400)";
                    break;
                case "System.Decimal":
                    dbType = "decimal(7,2)";
                    break;
                case "System.Double":
                    dbType = "float";
                    break;
                case "System.Int32":
                    dbType = "int";
                    break;
                case "System.DateTime":
                    dbType = "datetime";
                    break;
                case "System.Boolean":
                    dbType = "bit";
                    break;
                case "System.Byte":
                    dbType = "binary";
                    break;
                case "System.Guid":
                    dbType = "uniqueidentifier";
                    break;
                default:
                    break;
            }
            return dbType;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public IDbContext Insert<T>(T model)
        {
            return this;
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <returns></returns>
        public IDbContext BatchInsert(DataTable dataTable, string targetTableName = "")
        {
            return InternalExecute<DbContext>(() =>
             {
                 sqlExecute.BatchInsert(dataTable, targetTableName);
                 return this;
             });
        }

        /// <summary>
        /// 关闭数据库上下文
        /// </summary>
        public void Dispose()
        {
            sqlExecute.Close();
            sqlExecute = null;
        }

        /// <summary>
        /// 内部执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        private T InternalExecute<T>(Func<T> func)
        {
            try
            {
                Set<T>();
                T result = func();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"{System.Reflection.MethodBase.GetCurrentMethod().Name}:{ex.Message}");
            }
            return default(T);
        }
    }

    /// <summary>
    /// 数据库上下文接口
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// 初始化程序集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDbContext Set<T>();

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        IDbContext Insert<T>(T model);

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="targetTableName"></param>
        /// <returns></returns>
        IDbContext BatchInsert(DataTable dataTable, string targetTableName = "");
    }
}
