using Export.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Data
{
    /// <summary>
    /// sql帮助类
    /// </summary>
    public class SqlHelper : ISqlExecute
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings[CommonConst.DATABASELINK].ConnectionString;

        /// <summary>
        /// 数据库连接
        /// </summary>
        private IDbConnection dbConnection;

        /// <summary>
        /// 数据库命令
        /// </summary>
        private IDbCommand dbCommand;

        /// <summary>
        /// 数据库数据适配器
        /// </summary>
        private IDataAdapter dataAdapter;

        /// <summary>
        /// 数据库事务
        /// </summary>
        private IDbTransaction dbTransaction;

        /// <summary>
        /// sql类型
        /// </summary>
        public SqlType SqlType { get; set; } = SqlType.MSSQL;

        /// <summary>
        /// 初始化连接
        /// </summary>
        public SqlHelper()
        {

        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        public void Open()
        {
            switch (SqlType)
            {
                case SqlType.MSSQL:
                    dbConnection = new SqlConnection(connectionString);
                    break;
                case SqlType.ORACLE:
                    break;
                case SqlType.MYSQL:
                    break;
                default:
                    break;
            }
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>返回受影响的行数</returns>
        public int Execute(string commandText, CommandType commandType, params IDataParameter[] parameters)
        {
            return InternalExecute<int>(() =>
             {
                 CreateCommand(commandText, commandType, parameters);
                 return dbCommand.ExecuteNonQuery();
             });
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSql(string sql, params IDataParameter[] parameters)
        {
            return Execute(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteStoredProcedure(string procedureName, params IDataParameter[] parameters)
        {
            return Execute(procedureName, CommandType.StoredProcedure, parameters);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet Query(string commandText, CommandType commandType, params IDataParameter[] parameters)
        {
            return InternalExecute<DataSet>(() =>
            {
                DataSet dataSet = new DataSet();
                var sqlcommand = CreateCommand(commandText, commandType, parameters);
                dataAdapter = new SqlDataAdapter(sqlcommand);
                dataAdapter.Fill(dataSet);
                return dataSet;
            });
        }

        /// <summary>
        /// sql查询数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet QuerySql(string sql, params IDataParameter[] parameters)
        {
            return Query(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// 存储过程查询数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet QueryStoredProcedure(string procedureName, params IDataParameter[] parameters)
        {
            return Query(procedureName, CommandType.StoredProcedure, parameters);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="tragetTableName"></param>
        /// <returns></returns>
        public void BatchInsert(DataTable sourceTable, string tragetTableName = "")
        {
            InternalExecute(() =>
            {
                if (string.IsNullOrEmpty(tragetTableName))
                {
                    tragetTableName = sourceTable.TableName;
                }
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy((SqlConnection)dbConnection, SqlBulkCopyOptions.Default, (SqlTransaction)dbTransaction))
                {
                    sqlBulkCopy.DestinationTableName = tragetTableName;
                    sqlBulkCopy.BatchSize = 100000;
                    sqlBulkCopy.EnableStreaming = true;
                    sqlBulkCopy.BulkCopyTimeout = 60;
                    foreach (DataColumn dc in sourceTable.Columns)
                    {
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(dc.ColumnName, dc.ColumnName));
                    }
                    sqlBulkCopy.WriteToServer(sourceTable);
                }
            });
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        public void BeginTrans()
        {
            if (dbConnection == null)
            {
                throw new Exception("请先初始化数据库连接！");
            }
            if (dbTransaction == null)
            {
                dbTransaction = dbConnection.BeginTransaction();
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollBack()
        {
            if (dbConnection != null && dbTransaction != null)
            {
                dbTransaction.Rollback();
                //dbTransaction.Dispose();
                //dbConnection.Close();
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTrans()
        {
            if (dbConnection != null && dbTransaction != null)
            {
                dbTransaction.Commit();
                //dbTransaction.Dispose();
                //dbConnection.Close();
            }
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void Close()
        {
            if (dbTransaction != null)
            {
                dbTransaction.Dispose();
            }
            if (dbCommand != null)
            {
                dbCommand.Dispose();
            }
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }

        /// <summary>
        /// 创建数据库命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private SqlCommand CreateCommand(string commandText, CommandType commandType, params IDataParameter[] parameters)
        {
            if (dbConnection == null)
            {
                throw new Exception("请先初始化数据库连接！");
            }
            if (dbCommand == null)
            {
                dbCommand = new SqlCommand();
                dbCommand.CommandText = commandText;
                dbCommand.CommandType = commandType;
                if (parameters != null && parameters.Length > 0)
                {
                    foreach (var item in parameters)
                    {
                        dbCommand.Parameters.Add(item);
                    }
                }
                if (dbTransaction != null)
                {
                    dbCommand.Transaction = dbTransaction;
                }
                dbCommand.Connection = dbConnection;
            }
            return (SqlCommand)dbCommand;
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
                Open();
                T result = func();
                return result;
            }
            catch (SqlException ex)
            {
                Close();
                System.Diagnostics.Trace.TraceError($"{System.Reflection.MethodBase.GetCurrentMethod().Name}:{ex.Message}");
            }
            catch (Exception ex)
            {
                Close();
                System.Diagnostics.Trace.TraceError($"{System.Reflection.MethodBase.GetCurrentMethod().Name}:{ex.Message}");
            }
            return default(T);
        }

        /// <summary>
        /// 内部执行
        /// </summary>
        /// <param name="func"></param>
        private void InternalExecute(Action func)
        {
            try
            {
                Open();
                func();
            }
            catch (SqlException ex)
            {
                Close();
                System.Diagnostics.Trace.TraceError($"{System.Reflection.MethodBase.GetCurrentMethod().Name}:{ex.Message}");
            }
            catch (Exception ex)
            {
                Close();
                System.Diagnostics.Trace.TraceError($"{System.Reflection.MethodBase.GetCurrentMethod().Name}:{ex.Message}");
            }
        }

        public int Order { get; set; } = 1;
    }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum SqlType
    {
        MSSQL,
        ORACLE,
        MYSQL
    }

    /// <summary>
    /// 执行sql接口
    /// </summary>
    public interface ISqlExecute
    {
        /// <summary>
        /// 开启事务
        /// </summary>
        void BeginTrans();

        /// <summary>
        /// 提交事务
        /// </summary>
        void CommitTrans();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void RollBack();

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        void Close();

        /// <summary>
        /// sql查询数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataSet QuerySql(string sql, params IDataParameter[] parameters);

        /// <summary>
        /// 存储过程查询数据
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataSet QueryStoredProcedure(string procedureName, params IDataParameter[] parameters);

        /// <summary>
        /// sql执行
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteSql(string sql, params IDataParameter[] parameters);

        /// <summary>
        /// 存储过程执行
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteStoredProcedure(string procedureName, params IDataParameter[] parameters);

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="tragetTableName"></param>
        void BatchInsert(DataTable sourceTable, string tragetTableName = "");
    }
}
