using Export.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
        private static readonly Dictionary<Type, TableAttribute> tables = new Dictionary<Type, TableAttribute>();

        /// <summary>
        /// 缓存表相关字段
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, PropertyInfo>> tableFileds = new Dictionary<string, Dictionary<string, PropertyInfo>>();

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
            string tableName = t.Name;
            TableAttribute tableAttribute = null;
            if (!tables.ContainsKey(t))
            {
                tableAttribute = t.GetCustomAttribute<TableAttribute>();
                if (tableAttribute != null)
                {
                    tableName = tableAttribute.Name;
                }
                tables.Add(t, tableAttribute);
                string fieldName = string.Empty;
                TableColumnAttribute tableColumnAttribute = null;
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList().ForEach(x =>
                 {
                     tableColumnAttribute = x.GetCustomAttribute<TableColumnAttribute>();
                     if (!tableFileds.ContainsKey(tableName))
                     {
                         tableFileds.Add(tableName, new Dictionary<string, PropertyInfo>());
                     }
                     fieldName = x.Name;
                     if (tableColumnAttribute != null)
                     {
                         fieldName = tableColumnAttribute.Name;
                     }
                     tableFileds[tableName].Add(fieldName, x);
                 });
            }
            else
            {
                tableAttribute = tables[t];
            }

            stringBuilder.AppendFormat("if object_id('{0}') is null begin ", tables[t].Name);
            stringBuilder.AppendFormat("create table {0}(", tables[t].Name);
            StringBuilder commitBuilder = new StringBuilder();
            foreach (var item in tableFileds[tables[t].Name])
            {
                stringBuilder.AppendFormat("{0} {1} {2},", item.Key, GetDbType(item.Value.PropertyType), GetOther(item.Value, tableName, commitBuilder));
            }
            if (stringBuilder.ToString().IndexOf(',') >= 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append(")");
            if (tableAttribute != null && !string.IsNullOrEmpty(tableAttribute.Comments))
            {
                commitBuilder.AppendFormat("exec sp_addextendedproperty N'MS_Description', N'{0}', N'SCHEMA', N'dbo',N'table', N'{1}';", tableAttribute.Comments, tableAttribute.Name);
            }
            stringBuilder.Append(commitBuilder);
            stringBuilder.Append(" end ");
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
            switch (type.FullName)
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
        /// 获取数据库其他描述
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetOther(PropertyInfo property, string tableName, StringBuilder commits)
        {
            StringBuilder filedDesc = new StringBuilder();
            TableColumnAttribute tableColumnAttribute = property.GetCustomAttribute<TableColumnAttribute>();

            if (tableColumnAttribute != null)
            {
                switch (tableColumnAttribute.ColumnType)
                {
                    case TableColumnType.PrimaryKey:
                        filedDesc.Append(" primary key ");
                        break;
                    case TableColumnType.ForeignKey:
                        filedDesc.AppendFormat(" constraint fk_{0}_{1} references {2} ", property.Name, tableColumnAttribute.ForeignKeyDec.Substring(0, tableColumnAttribute.ForeignKeyDec.IndexOf("(")).ToString(CultureInfo.InvariantCulture), tableColumnAttribute.ForeignKeyDec);
                        break;
                    case TableColumnType.None:
                        break;
                    default:
                        break;
                }

                if (tableColumnAttribute.IsIdentity)
                {
                    filedDesc.Append(" identity(1,1) ");
                }

                if (!string.IsNullOrEmpty(tableColumnAttribute.DefaultValue))
                {
                    filedDesc.AppendFormat(" default({0}) ", tableColumnAttribute.DefaultValue);
                }

                if (property.PropertyType.IsGenericType && typeof(Nullable<>) == property.PropertyType)
                {
                    filedDesc.Append(" null ");
                }
                else
                {
                    filedDesc.Append(" not null ");
                }
                if (!string.IsNullOrEmpty(tableColumnAttribute.Comments))
                {
                    commits.AppendFormat(" EXECUTE sp_addextendedproperty 'MS_Description', '{0}', 'user', 'dbo', 'table', '{1}', 'column', '{2}';", tableColumnAttribute.Comments, tableName, tableColumnAttribute.Name);
                }
            }
            return filedDesc.ToString();
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
        public IDbContext BatchInsert<T>(DataTable dataTable, string targetTableName = "")
        {
            return InternalExecute<T, DbContext>(() =>
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
        private R InternalExecute<T, R>(Func<R> func)
        {
            try
            {
                Set<T>();
                R result = func();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"{System.Reflection.MethodBase.GetCurrentMethod().Name}:{ex.Message}");
                throw new ArgumentNullException(ex.Message, ex);
            }
            return default(R);
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
        IDbContext BatchInsert<T>(DataTable dataTable, string targetTableName = "");
    }
}
