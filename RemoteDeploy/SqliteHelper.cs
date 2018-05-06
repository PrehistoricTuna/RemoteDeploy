using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Data.SQLite;

namespace RemoteDeploy
{
    public class SqliteHelper
    {

        /// <summary>
        /// 数据库文件路径
        /// </summary>
        public static string _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UsersManager");
       
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string connectString = string.Format("Data Source={0};Pooling=true", _dbPath);

       /// <summary>
       /// 读取数据表
       /// </summary>
       /// <param name="commandText"></param>
       /// <param name="paramList"></param>
       /// <returns></returns>
        public static DataTable ExecuteNonQuery(string commandText, object[] paramList)
        {
            SQLiteConnection cn = new SQLiteConnection(connectString);
            string SQLQuery = commandText;
            if (paramList != null)
            {
                SQLQuery = string.Format(commandText, paramList);
            }
            SQLiteCommand cmd = new SQLiteCommand(SQLQuery, cn);
            DataTable datatable = new DataTable();
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter();
            dataAdapter.SelectCommand = cmd;
            dataAdapter.Fill(datatable);

            return datatable;

        }
    }
}
