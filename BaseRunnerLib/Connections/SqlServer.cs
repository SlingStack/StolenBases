using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace BaseRunnerLib.Connections
{
    public class SqlServer : Connection
    {
        public SqlConnection Connection { get; private set; }

        public SqlServer(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
        }

        public void SetConnectionString(string connectionString)
        {
            Connection.ConnectionString = connectionString;
        }

        public bool TestConnection()
        {
            try
            {
                Connection.Open();
                Connection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
