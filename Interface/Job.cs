using KAgent.Config;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace KAgent.Interface
{
    internal class Job
    {
        private DataTable dt;

        public Job()
        {
            if (dt == null)
            {
                dt = new DataTable();
            }
        }

        public List<Sender> Get()
        {
            string query = string.Format(@"SELECT tb_ftp_queue_pk as ftp_pk,
                        J.program_id,
                        J.program_code,
                        J.program_title,
                        J.channel,
                        SUBSTRING(F.filename, 12, 4) AS segment_code,
                        F.filename as filename,
                        F.srcpath,
                        F.dstpath,
                        F.status,
                        F.starttime,
                        F.customer_name,
                        J.mainstory,
                        J.intention,
                        J.program_subtitle,
                        C.ftp_id,
                        C.ftp_pw,
                        C.alias,
                        C.ftp_mode,
                        C.ftp_retry,
                        P.vid_bitrate
                        FROM TB_FTP_QUEUE F
                        INNER JOIN TB_CUSTOMER C ON F.tb_customer_pk_fk = C.tb_customer_pk
                        INNER JOIN TB_JOB_STATUS J ON F.tb_daily_pk_fk = J.tb_daily_pk_fk
                        INNER JOIN TB_PRESET P ON F.tb_profile_pk_fk = P.tb_preset_pk
                        WHERE STATUS = 'Pending'
                        AND J.program_ID != '(null)'
                        AND J.endtime >= DATE_ADD(NOW(), INTERVAL - 30 DAY)
                        GROUP BY tb_ftp_queue_pk
                        HAVING NOT segment_code LIKE 'SC%'
                        AND NOT segment_code LIKE 'SE%'
                        ORDER BY tb_ftp_queue_pk DESC
                        ");
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                conn.Open();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            return dt.AsEnumerable().Select(row => new Sender
            {
                ftp_pk = row.Field<UInt64>("ftp_pk"),
                segment_code = row.Field<string>("segment_code"),
                filename = row.Field<string>("filename"),
                customer_name = row.Field<string>("customer_name"),
                srcpath = row.Field<string>("srcpath"),
                ftpid = row.Field<string>("ftp_id"),
                ftppw = row.Field<string>("ftp_pw"),
                program_code = row.Field<string>("program_code"),
                program_id = row.Field<string>("program_id"),
                program_title = row.Field<string>("program_title"),
                dstpath = row.Field<string>("dstpath"),
                bitrate = row.Field<int>("vid_bitrate"),
                intention = row.Field<string>("intention"),
                mainstory = row.Field<string>("mainstory"),
                subtitle = row.Field<string>("program_subtitle"),
                alias = row.Field<int>("alias"),
                ftp_mode = row.Field<int>("ftp_mode"),
                ftpretry = row.Field<int>("ftp_retry"),
                channel = row.Field<int>("channel")
            }).ToList();
        }
    }
}