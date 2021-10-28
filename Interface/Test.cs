using KAgent.Config;
using MySql.Data.MySqlClient;

namespace KAgent.Interface
{
    internal class Test
    {
        public Test()
        {
        }

        public string Test1()
        {
            string query = "SELECT uuid() as id";
            string id = null;
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    id = rdr["id"].ToString();
                }
                rdr.Close();
            }
            return id;
        }
    }
}