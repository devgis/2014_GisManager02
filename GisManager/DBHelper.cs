using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace GisManager
{
    public class DBHelper
    {
        private static DBHelper instance;
        public static DBHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DBHelper();
                }
                return instance;
            }
        }

        private string dbConStr;
        public SqlConnection Conn;
        public DBHelper()
        {
            dbConStr = "Data Source=localhost;Initial Catalog=GisManagerDB;Persist Security Info=True;User ID=sa;Password=123456";
            Conn = new SqlConnection(dbConStr);
        }

        public DataTable GetDataTable(String SqlStr)
        {
            SqlDataAdapter dap = new SqlDataAdapter(SqlStr, Conn);
            DataTable dt = new DataTable();
            dap.Fill(dt);
            return dt;
        }

        public bool ExcuteSql(String SqlStr)
        {
            SqlCommand cmd = new SqlCommand(SqlStr, Conn);
            this.Conn.Open();
            int result = cmd.ExecuteNonQuery();
            this.Conn.Close();
            if (result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ExcuteSqlByTran(List<String> SqlStrList)
        {
            
            this.Conn.Open();
            SqlTransaction tran = this.Conn.BeginTransaction();
            foreach (String sSql in SqlStrList)
            {
                SqlCommand cmd = new SqlCommand(sSql, Conn);
                int result = cmd.ExecuteNonQuery();
                if (result <= 0)
                {
                    tran.Rollback();
                    this.Conn.Close();
                    return false;
                }
            }
            tran.Commit();
            this.Conn.Close();
            return true;
        }
    }
}
