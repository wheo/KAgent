using KAgent.Config;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace KAgent.Interface
{
    internal class ApiMeta
    {
        private DataTable dt;

        public string contentid { get; set; }
        public string cornerid { get; set; }
        public string segment_code { get; set; }
        public int bitrate { get; set; }
        public string regdate { get; set; }
        public string modifydate { get; set; }

        public string filepath { get; set; }
        public long filesize { get; set; }
        public int duration { get; set; }

        public string uri { get; set; }

        public string httpgetData { get; set; }

        public string targetName { get; set; }

        public string responseString { get; set; }

        public ApiMeta()
        {
            if (dt == null)
            {
                dt = new DataTable();
            }
        }

        public void GetPooqInfomation(string contentID, string cornerID, int bitrate)
        {
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                conn.Open();
                string sql = String.Format(@"SELECT tb_pooq_pk, contentid, cornerid, bitrate, regdate FROM TB_POOQ
                                                                WHERE contentid = '{0}'
                                                                AND cornerid = '{1}'
                                                                AND bitrate = '{2}'", contentID, cornerID, bitrate);
                MySqlDataAdapter adpt = new MySqlDataAdapter(sql, conn);
                adpt.Fill(dt);
            }
            contentid = dt.Rows[0].Field<string>("contentid");
            cornerid = dt.Rows[0].Field<string>("cornerid");
            bitrate = dt.Rows[0].Field<int>("bitrate");
            regdate = dt.Rows[0].Field<string>("regdate");
        }
    }
}