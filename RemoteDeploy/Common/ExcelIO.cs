using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Globalization;
using System.Data;
using System.Windows.Forms;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.Common
{
    /// <summary>
    /// EXCEL读取类
    /// </summary>
    public static class ExcelIO
    {
        /// <summary>
        /// 读取Excel文件内数据信息，并返回数据类
        /// </summary>
        /// <param name="filePath">Excel文件路径</param>
        /// <returns>读取的数据类</returns>
        public static DataSet ReadDataByOledb(string filePath)
        {
            //以数据库模式读取Excel文件
            DataSet dataSet = new DataSet();

            //Excel文件内表名称集合  
            List<string> tableNameList = new List<string>();

            //表名称（用作抛异常时定位）
            string tbName = string.Empty;

            //判定文件是否存在
            if (System.IO.File.Exists(filePath))
            {
                //构造数据库连接语句
                string connectionString = @"Provider = Microsoft.ACE.OLEDB.12.0 ; Data Source =" + filePath + @"; Extended Properties= 'Excel 12.0;IMEX=1;HDR=no;'";
                string sqlCommand = string.Empty;

                //建立odbc连接
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    try
                    {
                        //开启连接，以数据库模式读取文件内容
                        connection.Open();

                        //获取文件内表名称列表
                        tableNameList = GetTableNames(connection);

                        //遍历所有的表
                        foreach (string tableName in tableNameList)
                        {
                            OleDbDataAdapter dataAdapter = new OleDbDataAdapter();

                            dataSet.Locale = CultureInfo.InvariantCulture;
                            dataSet.EnforceConstraints = false;

                            //获取除表头外的数据
                            sqlCommand = "select * from [" + tableName + "$A2:I28]";
                            //连接该数据表
                            dataAdapter = new OleDbDataAdapter(sqlCommand, connection);
                            //获取数据
                            dataAdapter.Fill(dataSet, "[" + tableName + "]");

                            //当前只需要读取一个表，读取之后即跳出循环
                            break;

                        }

                    }
                    catch (System.Exception e)
                    {
                        //关闭连接
                        connection.Close();
                        MessageBox.Show("打开Excel发生异常，请检查系统配置及文件占用情况！");
                        LogManager.InfoLog.LogCommunicationError("MainWindow", "ProductReport",
                            "文件[" + filePath.Substring(filePath.LastIndexOf('\\') + 1) + "]读取[" + tbName + "]信息时" + e.Message);
                    }
                    finally
                    {
                        //关闭连接
                        connection.Close();
                    }
                }
            }

            return dataSet;

        }

        /// <summary>
        /// 获取Excel文件内所有表名称，每个sheet为一个表
        /// </summary>
        /// <param name="connection">开启与该文件的数据库连接</param>
        /// <returns>文件内表名称列表</returns>
        private static List<string> GetTableNames(OleDbConnection connection)
        {
            List<string> tempList = new List<string>();

            //所有表的集合
            System.Data.DataTable table = connection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            if (table == null || table.Rows.Count == 0)
            {
                //列表为空，错误返回
                tempList = null;
                return tempList;
            }

            //获取每个表名
            foreach (System.Data.DataRow row in table.Rows)
            {
                string tempTableName = row["Table_Name"].ToString();
                /*20160524 by 宋殿生 
                    增加table有效性的判断  带'$'的table有效 屏蔽无效的table
                 */
                if (tempTableName.Contains("$"))
                {
                    int tempIndex = tempTableName.LastIndexOf('$');

                    //移除字符串后的符号,获取表名时可能带有符号
                    if (tempIndex < tempTableName.Length)
                    {
                        tempTableName = tempTableName.Remove(tempIndex);
                        //20160712 移除表名中的无效字符
                        tempTableName = tempTableName.Replace("'", "");
                    }

                    //跳过重复的表
                    if (tempList.Contains(tempTableName))
                    {
                        continue;
                    }
                    else
                    {
                        tempList.Add(tempTableName);
                    }
                }
            }

            return tempList;
        }

    }
}
