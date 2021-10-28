using KAgent.Config;
using KAgent.Util;
using log4net;
using MediaInfo;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Net;
using System.Text;
using KAgent.Singleton;

namespace KAgent.Interface
{
    internal class Sender
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Item status { get; set; }

        public string filename { get; set; }

        public UInt64 ftp_pk { get; set; }
        public string customer_name { get; set; }
        public string srcpath { get; set; }
        public string strTail { get; set; }
        public string ftpid { get; set; }
        public string ftppw { get; set; }

        private string _program_code;

        public string program_code
        {
            get
            {
                return _program_code;
            }
            set
            {
                if (value == "T2010-0301")
                {
                    // TV유치원 코드 교체
                    _program_code = "T2007-0154";

                    if (segment_code == "S001")
                    {
                        // 토마스와 친구들
                        cornerID = "2";
                    }
                    else if (segment_code == "S003")
                    {
                        // 우루루
                        cornerID = "3";
                    }
                }
                else if (value == "T2004-0941")
                {
                    // 콘서트 7080
                    _program_code = "T2004-0105";
                }
                else if (value == "T2010-1446")
                {
                    // 안녕하세요
                    _program_code = "T2010-1328";
                }
                else if (value == "T2011-0001")
                {
                    // 사랑의 가족
                    _program_code = "T2000-0061";
                }
                else if (value == "T2010-0015")
                {
                    // KBS 바둑왕전
                    _program_code = "T2001-1091";
                }
                else
                {
                    _program_code = value;
                }
            }
        }

        public string program_id { get; set; }
        public string program_title { get; set; }
        public string dstpath { get; set; }
        public int bitrate { get; set; }
        public string segment_code { get; set; }
        public string intention { get; set; }
        public string mainstory { get; set; }
        public string subtitle { get; set; }
        public int alias { get; set; }
        public int ftp_mode { get; set; }
        public int ftpretry { get; set; }
        public int channel { get; set; }
        public string strftpRoot { get; set; }

        public string contentHead
        {
            get
            {
                return string.Format($"K0{channel + 1}");
            }
        }

        public string contentID
        {
            get
            {
                return contentHead + "_" + program_id;
            }
        }

        public string programid
        {
            get
            {
                return contentHead + "_" + program_code;
            }
        }

        private string _cornerID;

        public string cornerID
        {
            get
            {
                if (string.IsNullOrEmpty(_cornerID))
                {
                    if (segment_code == "S000")
                    {
                        return "1";
                    }
                    else if (segment_code == "S001")
                    {
                        return "1";
                    }
                    else if (segment_code.IndexOf("SC") == 0)
                    {
                        return "1";
                    }
                    else
                    {
                        return "2";
                    }
                }
                else
                {
                    return _cornerID;
                }
            }
            set
            {
                if (program_code == "T2010-0301")
                {
                    if (segment_code == "S001")
                    {
                        // 토마스와 친구들
                        value = "2";
                    }
                    else if (segment_code == "S003")
                    {
                        // 우루루
                        value = "3";
                    }
                }
                else
                {
                    _cornerID = value;
                }
            }
        }

        public long filesize { get; set; }
        public int duration { get; set; }

        public string regdate { get; set; }
        public string modifydate { get; set; }

        public string ftpdestpath
        {
            get
            {
                if (alias == 0)
                {
                    return Path.GetFileName(srcpath);
                }
                else if (alias == 1)
                {
                    return program_title + "_" + DateTime.Now.ToString("yyyyMMdd") + Path.GetExtension(srcpath);
                }
                else if (alias == 2) // pooq 전용
                {
                    return Path.GetFileNameWithoutExtension(srcpath) + strTail + Path.GetExtension(srcpath);
                }
                else if (alias == 3)
                {
                    return Path.GetFileNameWithoutExtension(srcpath) + strTail + Path.GetExtension(srcpath);
                }
                else
                {
                    //alias 가 0~3이 아니다
                    return null;
                }
            }
        }

        public string ftppath
        {
            get
            {
                return string.Format("{0}/{1}/", dstpath, program_code);
            }
        }

        public string uploadpath
        {
            get
            {
                return string.Format("{0}/{1}/{2}", dstpath, program_code, ftpdestpath);
            }
        }

        public ApiMeta apimeta { get; set; }

        private double percent { get; set; }

        public void SetStatus(string status)
        {
            this.status.status = status;
            string percent_query = "";
            if (status != "Running")
            {
                this.status.end_at = DateTime.Now;
                TimeSpan dateDiff = this.status.end_at - this.status.start_at;
                this.status.elapsed = dateDiff.Hours * 3600 + dateDiff.Minutes * 60 + dateDiff.Seconds;
            }
            if (status == "Completed")
            {
                percent_query = ", percent = 100";
            }
            else if (status == "Failed")
            {
                this.status.api_status = "Failed";
            }
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                conn.Open();
                //업로드 후 update
                //string sql = String.Format("UPDATE TB_FTP_QUEUE SET status ='Completed' WHERE srcpath='{0}'", srcpath);
                string sql = String.Format($"UPDATE TB_FTP_QUEUE SET starttime = CURRENT_TIMESTAMP(), endtime = NULL, status ='{status}'{percent_query} WHERE tb_ftp_queue_pk ='{ftp_pk}'");
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                logger.Info(string.Format("{0} ({2}) | {1} is {3}", ftp_pk, srcpath, customer_name, status));
            }
        }

        public void MakeFtpPath()
        {
            Uri uri = new Uri(ftppath);
            logger.Info(uri.LocalPath);

            string[] uriArray = uri.LocalPath.Split('/');
            string baseuri = string.Format($"{uri.Scheme}://{uri.Host}:{uri.Port}/");
            string subpath = "";
            string makepath = "";

            for (int i = 0; i < uriArray.Length; i++)
            {
                if (!string.IsNullOrEmpty(uriArray[i]))
                    try
                    {
                        subpath = subpath + uriArray[i] + "/";
                        makepath = string.Format($"{baseuri}/{subpath}");
                        FtpWebRequest requestMkdir = (FtpWebRequest)WebRequest.Create(makepath);
                        requestMkdir.Method = WebRequestMethods.Ftp.MakeDirectory;
                        requestMkdir.Credentials = new NetworkCredential(ftpid, ftppw);

                        FtpWebResponse response = (FtpWebResponse)requestMkdir.GetResponse();
                        logger.Info(string.Format("Status : {0}", response.StatusDescription));
                        response.Close();
                        logger.Info(string.Format("{0} 폴더가 생성 되었습니다.", ftppath));
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.ToString());
                        //logger.Error(ex.ToString());
                    }
            }

            /*

            string[] folderArray = ftppath.Split('/');
            string folderName = "";
            for (int i = 0; i < folderArray.Length; i++)
            {
                if (!string.IsNullOrEmpty(folderArray[i]))
                {
                    folderName = string.IsNullOrEmpty(folderName) ? folderArray[i] : folderName + "/" + folderArray[i];
                    WebRequest request = WebRequest.Create("ftp://" + ftpAddress + "/" + folderName);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    request.Credentials = new NetworkCredential(ftpid, ftppw);
                    var response = request.GetResponse();
                    var test = response.ToString();
                }
            }
            */
        }

        public void ReportProgress()
        {
            //50메가 업로드에 한번씩 percent 갱신
            //UI에 해당내용 반영
            //Console.WriteLine("{3} | {0} MB 중 {1} MB 전송 | {2:F}%", inputStream.Length / 1024 / 1024, totalRealBytesCount / 1024 / 1024, percent, program_title);

            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                conn.Open();
                //업로드 후 update
                //string sql = String.Format("UPDATE TB_FTP_QUEUE SET status ='Completed' WHERE srcpath='{0}'", srcpath);
                string sql = String.Format($"UPDATE TB_FTP_QUEUE SET percent = {percent} WHERE tb_ftp_queue_pk ='{ftp_pk}'");
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        public void GetDuraion()
        {
#if true
            srcpath = srcpath.Replace("/mnt/output", "Y:").Replace("/", "\\");
#else
            srcpath = "test\\choco.mp4";
#endif
            try
            {
                filesize = new System.IO.FileInfo(srcpath).Length;
                if (Path.GetExtension(srcpath).ToLower() != ".xml" && Path.GetExtension(srcpath).ToLower() != ".png")
                {
                    MediaInfoWrapper mi = new MediaInfoWrapper(srcpath);

                    //string strDuration = pMi.Get(0, 0, "Duration");
                    //string strDuration = mi.Duration;
                    //s.duration = Convert.ToInt32(strDuration) / 1000;
                    duration = mi.Duration / 1000;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        public bool FileUpload()
        {
            FtpWebRequest requestUpload = (FtpWebRequest)WebRequest.Create(uploadpath);
            requestUpload.Method = WebRequestMethods.Ftp.UploadFile;
            requestUpload.Credentials = new NetworkCredential(ftpid, ftppw);

            if (ftp_mode == 0)
            {
                requestUpload.UsePassive = true;
            }
            else
            {
                requestUpload.UsePassive = false;
            }
            logger.Info(String.Format("{0} file is sending", srcpath));
            try
            {
                using (var inputStream = File.OpenRead(srcpath))

                using (var outputStream = requestUpload.GetRequestStream())
                {
                    var buffer = new byte[1024 * 1024];
                    ulong totalRealBytesCount = 0;
                    int readByteCount;

                    while ((readByteCount = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputStream.Write(buffer, 0, readByteCount);
                        totalRealBytesCount += (ulong)readByteCount;
                        percent = totalRealBytesCount * 100.0 / inputStream.Length;

                        if (percent <= 100)
                        {
                            status.progress = percent;
                            ReportProgress();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return false;
            }
            logger.Info(string.Format("File {0} send completed", srcpath));
            return true;
        }

        //public string destExt;

        public void requestMeta()
        {
            string destExt = Path.GetExtension(ftpdestpath).ToLower();
            if (alias >= 2 && alias < 4)
            {
                if (destExt != ".xml" && destExt != ".png")
                {
                    apimeta = new ApiMeta();
                    apimeta.GetPooqInfomation(contentID, cornerID, bitrate);
                    apimeta.filesize = filesize;
                    apimeta.duration = duration;
                    apimeta.segment_code = segment_code;

                    if (contentID.Equals(apimeta.contentid))
                    {
                        regdate = apimeta.regdate;
                        modifydate = DateTime.Now.ToString("yyyyMMddHHmmss");
                    }
                    else
                    {
                        regdate = DateTime.Now.ToString("yyyyMMddHHmmss");
                        modifydate = regdate;
                    }

                    if (strftpRoot.Length < 2)
                    {
                        apimeta.filepath = string.Format("{0}/{1}", program_code, ftpdestpath);
                    }
                    else
                    {
                        apimeta.filepath = string.Format("{0}/{1}/{2}", strftpRoot, program_code, ftpdestpath);
                    }

                    if (alias == 2)
                    {
                        apimeta.uri = Settings.GetInstance().apiuri;
                        apimeta.httpgetData = String.Format($"{apimeta.contentid}|{apimeta.cornerid}|{apimeta.bitrate}|{apimeta.filepath}|{apimeta.regdate}|{apimeta.modifydate}|{apimeta.filesize}|{apimeta.duration}|Y");
                        apimeta.targetName = "POOQ";
                    }
                    else if (alias == 3)
                    {
                        apimeta.targetName = "SMR";
                        apimeta.targetName = Settings.GetInstance().api_smr_uri;

                        if (subtitle.ToLower() == "(null)" || subtitle == "")
                        {
                            subtitle = contentID;
                        }
                        if (mainstory.ToLower() == "(null)" || mainstory == "")
                        {
                            mainstory = "null";
                        }
                        if (intention.ToLower() == "(null)" || intention == "")
                        {
                            intention = "null";
                        }
                        apimeta.httpgetData = String.Format($"{apimeta.segment_code}|{apimeta.contentid}|{apimeta.cornerid}|{apimeta.bitrate}|{apimeta.filepath}|{apimeta.regdate}|{apimeta.modifydate}|{apimeta.filesize}|{apimeta.duration}|N|{intention}|{subtitle}|{mainstory})");
                    }

#if false
                    Dictionary<String, String> map = new Dictionary<string, string>();

                    map.Add("contentid", contentID);
                    map.Add("cornerid", cornerID);
                    map.Add("programid", programid);
                    map.Add("originid", program_id);
                    map.Add("contentnumber", "");
                    map.Add("cornernumber", "");
                    map.Add("preview", intention);
                    map.Add("broaddate", "20160810");
                    map.Add("title", program_title);
                    map.Add("contentimg", "");
                    map.Add("searchkeyword", "");
                    map.Add("regdate", regdate);
                    map.Add("modifydate", modifydate);
                    map.Add("actor", "");
                    map.Add("isuse", "Y");

                    MakeXML mxml = new MakeXML(map);
                    DateTime NowDate = DateTime.Now;
                    mxml.MakeFile(NowDate.ToString("yyyyMMddHHmmss") + ".xml");

                    mxml.MakeXmlToString();
                    mxml.base64Encoding();
                    String base64Str = mxml.getbase64XmlString();
                    System.IO.File.WriteAllText(NowDate.ToString("yyyyMMddHHmmss") + ".txt", base64Str, Encoding.UTF8);
#endif
                    var response = Http.Post(apimeta.uri, new System.Collections.Specialized.NameValueCollection()
                    {
                        {"cmd", "001" },
                        {"msg", apimeta.httpgetData }
                    });
                    apimeta.responseString = Encoding.UTF8.GetString(response);

                    logger.Info(string.Format($"{apimeta.targetName} 메타정보 API 전송 성공"));
                    logger.Info(string.Format($"reponse : {apimeta.responseString}"));
                    this.status.api_status = "Completed";
                }
            }
            else
            {
                this.status.api_status = "해당없음";
            }
        }

        public void initModel()
        {
            status = new Item();
            status.filepath = string.Format($"{dstpath}/{program_code}/{filename}");
            status.customer = customer_name;
            status.status = "Running";
            status.api_status = "Pending";
            Status.GetInstance().items.InsertItem(0, status);
        }

        public bool FtpService()
        {
            SetStatus("Running");
            GetDuraion();
            MakeFtpPath();
            if (FileUpload())
            {
                SetStatus("Completed");
                requestMeta();
                DatabaseLogging();
            }
            else
            {
                SetStatus("Failed");
                return false;
            }

            return true;
        }

        private void DatabaseLogging()
        {
            using (MySqlConnection conn = new MySqlConnection(DatabaseManager.GetInstance().ConnectionString))
            {
                conn.Open();
                //업로드 후 update
                //string sql = String.Format("UPDATE TB_FTP_QUEUE SET status ='Completed' WHERE srcpath='{0}'", srcpath);
                string sql = "";
                mainstory = mainstory.Replace("'", "\\'");
                intention = intention.Replace("'", "\\'");
                if (alias == 2)
                {
                    sql = String.Format(@"INSERT INTO TB_POOQ (insert_time, cmd, ftp_queue_fk, segment_code, program_title, contentid, cornerid, bitrate, filepath, regdate, modifydate, filesize, playtime, issue, response, request)
                                                VALUES (CURRENT_TIMESTAMP(), '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}')",
                    "001", ftp_pk, segment_code, program_title, apimeta.contentid, apimeta.cornerid, apimeta.bitrate, apimeta.filepath, apimeta.regdate, apimeta.modifydate, apimeta.filesize, apimeta.duration, "Y", apimeta.responseString, apimeta.httpgetData);
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                else if (alias == 3)
                {
                    sql = String.Format(@"INSERT INTO TB_POOQ (insert_time, cmd, ftp_queue_fk, segment_code, program_title, contentid, cornerid, bitrate, filepath, regdate, modifydate, filesize, playtime, issue, response, intention, mainstory, request)
                                                VALUES (CURRENT_TIMESTAMP(), '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}')",
                    "001", ftp_pk, segment_code, program_title, apimeta.contentid, apimeta.cornerid, apimeta.bitrate, apimeta.filepath, apimeta.regdate, apimeta.modifydate, apimeta.filesize, apimeta.duration, "N", apimeta.responseString, intention, mainstory, apimeta.httpgetData);
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}