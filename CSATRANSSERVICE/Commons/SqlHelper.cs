using System.Data;
using System.Configuration;
using System.Web;
using System.Collections;
using System.Data.SqlClient;
using System;

public abstract class SqlHelper
{
    //获取数据库连接字符串，其属于静态变量且只读，项目中所有文档可以直接使用，但不能修改
    public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["SQLConnString"].ConnectionString;

    /// <summary>
    /// Method: ExecuteNonQuery
    /// Description: 执行一个不需要返回结果的SqlCommand命令，通过指定专用的连接字符串，使用参数数组形式提供参数列表
    /// Author: Xiecg
    /// Date: 2019/06/09
    /// Parameter: connectionString 一个有效的数据库连接字符串
    /// Parameter: cmdType SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)
    /// Parameter: cmdText 存储过程的名字或者 T-SQL 语句
    /// Parameter: commandParameters 以数组形式提供SqlCommand命令中用到的参数列表
    /// Returns: int 返回一个数值表示此SqlCommand命令执行后影响的行数
    ///</summary>
    public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    {

        SqlCommand cmd = new SqlCommand();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            //通过PrePareCommand方法将参数逐个加入到SqlCommand的参数集合中
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();

            //清空SqlCommand中的参数列表
            cmd.Parameters.Clear();
            return val;
        }
    }

    /// <summary>
    /// Method: ExecuteNonQuery
    /// Description: 执行一条不返回结果的SqlCommand，通过一个已经存在的数据库连接，使用参数数组提供参数
    /// Author: Xiecg
    /// Date: 2019/06/09
    /// Parameter: connection 一个现有的数据库连接
    /// Parameter: cmdType SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)
    /// Parameter: cmdText 存储过程的名字或者 T-SQL 语句
    /// Parameter: commandParameters 以数组形式提供SqlCommand命令中用到的参数列表
    /// Returns: int 返回一个数值表示此SqlCommand命令执行后影响的行数
    ///</summary>
    public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    {

        SqlCommand cmd = new SqlCommand();

        PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
        int val = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    /// Method: ExecuteNonQuery
    /// Description:  执行一条不返回结果的SqlCommand，通过一个已经存在的数据库事物处理，使用参数数组提供参数
    /// Author: Xiecg
    /// Date: 2019/06/09
    /// Parameter: trans 一个存在的 sql 事物处理
    /// Parameter: cmdType SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)
    /// Parameter: cmdText 存储过程的名字或者 T-SQL 语句
    /// Parameter: commandParameters 以数组形式提供SqlCommand命令中用到的参数列表
    /// Returns: int 返回一个数值表示此SqlCommand命令执行后影响的行数
    ///</summary>
    public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    {
        SqlCommand cmd = new SqlCommand();
        PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
        int val = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    /// Method: ExecuteReader
    /// Description: 执行一条返回结果集的SqlCommand命令，通过专用的连接字符串，使用参数数组提供参数
    /// Author: Xiecg
    /// Date: 2019/06/09
    /// Parameter: connectionString 一个有效的数据库连接字符串
    /// Parameter: cmdType SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)
    /// Parameter: cmdText 存储过程的名字或者 T-SQL 语句
    /// Parameter: commandParameters 以数组形式提供SqlCommand命令中用到的参数列表
    /// Returns: System.Data.SqlClient.SqlDataReader 返回一个包含结果的SqlDataReader
    ///</summary>
    public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    {
        SqlCommand cmd = new SqlCommand();
        SqlConnection conn = new SqlConnection(connectionString);

        // 在这里使用try/catch处理是因为如果方法出现异常，则SqlDataReader就不存在，
        //CommandBehavior.CloseConnection的语句就不会执行，触发的异常由catch捕获。
        //关闭数据库连接，并通过throw再次引发捕捉到的异常。
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch
        {
            conn.Close();
            throw;
        }
    }

    /// <summary>
    /// Method: ExecuteScalar
    /// Description: 执行一条返回第一条记录第一列的SqlCommand命令，通过专用的连接字符串，使用参数数组提供参数
    /// Author: Xiecg
    /// Date: 2019/06/09
    /// Parameter: connectionString 一个有效的数据库连接字符串
    /// Parameter: cmdType SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)
    /// Parameter: cmdText 存储过程的名字或者 T-SQL 语句
    /// Parameter: commandParameters 以数组形式提供SqlCommand命令中用到的参数列表
    /// Returns: object 返回一个object类型的数据
    ///</summary>
    public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    {
        SqlCommand cmd = new SqlCommand();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }
    }

    /// <summary>
    /// Method: ExecuteScalar
    /// Description: 执行一条返回第一条记录第一列的SqlCommand命令，通过已经存在的数据库连接，使用参数数组提供参数
    /// Author: Xiecg
    /// Date: 2019/06/09
    /// Parameter: connection 一个已经存在的数据库连接
    /// Parameter: cmdType SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)
    /// Parameter: cmdText  存储过程的名字或者 T-SQL 语句
    /// Parameter: commandParameters 以数组形式提供SqlCommand命令中用到的参数列表
    /// Returns: object 返回一个object类型的数据
    ///</summary>
    public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    {

        SqlCommand cmd = new SqlCommand();

        PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
        object val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }

    /// <summary>
    /// Method: PrepareCommand
    /// Description: 为执行命令准备参数
    /// Author: Xiecg
    /// Date: 2019/06/09
    /// Parameter: cmd SqlCommand 命令
    /// Parameter: conn 已经存在的数据库连接
    /// Parameter: trans 数据库事物处理
    /// Parameter: cmdType SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)
    /// Parameter: cmdText Command text，T-SQL语句
    /// Parameter: cmdParms 返回带参数的命令
    /// Returns: void
    ///</summary>
    private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
    {

        //判断数据库连接状态
        if (conn.State != ConnectionState.Open)
            conn.Open();

        cmd.Connection = conn;
        cmd.CommandText = cmdText;

        //判断是否需要事物处理
        if (trans != null)
            cmd.Transaction = trans;

        cmd.CommandType = cmdType;

        if (cmdParms != null)
        {
            foreach (SqlParameter parm in cmdParms)
                cmd.Parameters.Add(parm);
        }
    }


    /// <summary>
    /// Method  ConvertDataReaderToDataTable
    /// Description  将SqlDataReade类型转换成DataTable类型
    /// Author: Xiecg
    /// Date: 2019/06/09
    /// Parameter: reader 数据类型为SqlDataReade
    /// Returns: System.Data.DataTable 返回DataTable类型数据
    ///</summary>
    public static DataTable ConvertSqlDataReadeToDataTable(SqlDataReader reader)
    {
        try
        {
            DataTable objDataTable = new DataTable();
            int intFieldCount = reader.FieldCount;
            for (int intCounter = 0; intCounter < intFieldCount; ++intCounter)
            {
                objDataTable.Columns.Add(reader.GetName(intCounter), reader.GetFieldType(intCounter));
            }
            objDataTable.BeginLoadData();
            object[] objValues = new object[intFieldCount];
            while (reader.Read())
            {
                reader.GetValues(objValues);
                objDataTable.LoadDataRow(objValues, true);
            }
            reader.Close();
            objDataTable.EndLoadData();
            return objDataTable;
        }
        catch (Exception ex)
        {
            throw new Exception("转换出错!", ex);
        }
    }
}