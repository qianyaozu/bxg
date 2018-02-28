using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetData.Entities
{
   public class Database
    {
        private static readonly string _connectionString;
        static Database()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DB_CONN"].ConnectionString;
            SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder(connectionString);
            _connectionString = scsb.ToString();
        }
        public static SqlConnection GetDbConnection(bool mars = false)
        {
            if (mars)
            {
                SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder(_connectionString);
                scsb.MultipleActiveResultSets = true;
            }

            var connection = new SqlConnection(_connectionString);
            connection.Open();

            return connection;
        }
    }
}
