using ApiMVC.Models;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
namespace ApiMVC.Controllers
{
    public class CallHistoryController : Controller
    {
        // LẤY LỊCH SỬ CUỘC GỌI VÀ LƯU VÀO DATABASE
        public async Task<ActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("http://dial.voip24h.vn/dial/history?voip=76af0a0d5f8445fa649525123d713c6bc2b2f9b8&secret=1366b46c23edb28f61aeae42fd571e00");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var jsonObject = JsonConvert.DeserializeObject<ApiResult>(json);
                    var jsonData = jsonObject.Result.Data;
                    using (var db = new CallDbContext())
                    {
                        //var lastestCallDate = db.CallsHistory.OrderByDescending(x => x.CallDate).Select(x => x.CallDate).FirstOrDefault();
                        foreach (var item in jsonData)
                        {
                            var entity = new CallHistory
                            {
                                Id = item.Id,
                                CallDate = item.CallDate,
                                CallId = item.CallId,
                                Recording = item.Recording,
                                Play = item.Play,
                                Eplay = item.Eplay,
                                Download = item.Download,
                                Did = item.Did,
                                Src = item.Src,
                                Dst = item.Dst,
                                Status = item.Status,
                                Note = item.Note,
                                Disposition = item.Disposition,
                                LastApp = item.LastApp,
                                BillSec = item.BillSec,
                                Duration = item.Duration,
                                Type = item.Type,
                                Duration_Minutes = item.Duration_Minutes,
                                Duration_Seconds = item.Duration_Seconds
                            };

                            //Kiểm tra xem lịch sử cuộc gọi có tồn tại trong database hay không dựa theo callid
                            var existingData = db.CallsHistory.FirstOrDefault(x => x.CallId == entity.CallId);

                            //Nếu không tồn tại thì thêm mới
                            if (existingData == null /*&& entity.CallDate > lastestCallDate*/)
                            {
                                db.CallsHistory.Add(entity);
                            }
                        }
                        await db.SaveChangesAsync();
                    }
                    return View(jsonData);
                }
                else
                {
                    return View("Error");
                }
            }
        }

        // LẤY LỊCH SỬ CUỘC GỌI NHƯNG KHÔNG LƯU VÀO DATABASE
        public async Task<ActionResult> GetHistoryCall()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://dial.voip24h.vn/dial/history?voip=76af0a0d5f8445fa649525123d713c6bc2b2f9b8&secret=1366b46c23edb28f61aeae42fd571e00");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResult>(responseContent);
                    var apiData = apiResponse.Result.Data;
                    return View(apiData);
                }
                else
                {
                    return View("Error");
                }
            }
        }

        // XỬ LÝ TẢI TẬP TIN GHI ÂM VÀ LƯU VÀO THƯ MỤC RECORDINGS THEO NĂM, THÁNG, NGÀY
        public ActionResult DownloadForDate(string downloadUrl)
        {
            using (WebClient client = new WebClient())
            {
                // Tải về tập tin từ URL của API.
                byte[] result = client.DownloadData(downloadUrl);

                // Chỉ định đường dẫn để lưu tệp mới trong thư mục "Downloads" của ứng dụng.
                var dateNow = DateTime.Now;
                var folderPath = Path.Combine(Server.MapPath("~/Recordings/"), dateNow.Year.ToString(), dateNow.Month.ToString(), dateNow.Day.ToString());

                // Tạo thư mục lưu trữ nếu nó chưa tồn tại.
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Tạo tên tệp mới để lưu tập tin vào thư mục "Downloads" của ứng dụng.
                string fileName = DateTime.Now.ToString("dd-MM-yyyy_HHmmss") + ".wav";

                // Lưu tập tin vừa tải về vào đường dẫn chỉ định.
                System.IO.File.WriteAllBytes(Path.Combine(folderPath, fileName), result);

                string filePath = Path.Combine(folderPath, fileName);

                // Trả về file vừa tải về để tải xuống và lưu vào thư mục Downloads.
                return File(result, "audio/wav", fileName);
            }
        }

        // XỬ LÝ TẢI TẬP TIN GHI ÂM VÀ LƯU VÀO THƯ MỤC RECORDINGS CHUỖI LẤY RA TỪ URL
        public ActionResult DownloadForUrl(string downloadUrl)
        {
            using(WebClient client = new WebClient())
            {
                // Tải về tập tin từ tham số downloadUrl truyền từ thẻ a
                byte[] result = client.DownloadData(downloadUrl);

                // Lấy chuỗi từ URL
                int startUrl = downloadUrl.IndexOf("&pkeyID=") + "&pkeyID=".Length;
                int endUrl = downloadUrl.LastIndexOf(".gsm");

                // Tạo thư mục lưu trữ theo thời gian hiện tại
                var dateNow = DateTime.Now;
                var folderPath = Path.Combine(Server.MapPath("~/Recordings"), dateNow.Year.ToString(), dateNow.Month.ToString(), dateNow.Day.ToString());

                if(!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                // Tạo tên tệp mới cú pháp là ngày / tháng / năm để lưu tập tin vào thư mục "Downloads" của ứng dụng.
                string fileName = downloadUrl.Substring(startUrl, endUrl - startUrl) + ".wav";
                // Lưu tập tin vừa tải về vào đường dẫn chỉ định.
                System.IO.File.WriteAllBytes(Path.Combine(folderPath,fileName), result);

                // Trả về file vừa tải về để tải xuống và lưu vào thư mục Downloads.
                return File(result, "audio/wav", fileName);

            }
        }

        // XỬ LÝ TẬP TIN GHI ÂM V1 (lưu theo cú pháp năm-tháng-ngày_giờ:phút:giây)
        public string SaveFileV1(byte[] data)
        {
            //Chỉ định đường dẫn để lưu tệp mới trong thư mục "Downloads" của ứng dụng.
            string recordingsPath = Server.MapPath("~/Recordings");
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string day = DateTime.Now.Day.ToString().PadLeft(2, '0');
            string folderPath = Path.Combine(recordingsPath, year, month, day);

            //Tạo thư mục lưu trữ nếu nó chưa tồn tại.
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            //Tạo tên tệp mới để lưu tập tin vào thư mục "Downloads" của ứng dụng.
            string fileName = DateTime.Now.ToString("dd-MM-yyyy_HHmmss") + ".wav";

            //Lưu tập tin vừa tải về vào đường dẫn chỉ định.
            System.IO.File.WriteAllBytes(Path.Combine(folderPath, fileName), data);

            //Trả về đường dẫn tới tệp vừa tải về
            return Path.Combine(folderPath, fileName);


        }

        // XỬ LÝ TẢI TẬP TIN GHI ÂM V2 ( lưu theo cú pháp từ 1 chuỗi url)
        public string SaveFileV2(byte[] data, string fileName)
        {
            //Chỉ định đường dẫn để lưu tệp mới trong thư mục "Downloads" của ứng dụng.
            //var dateNow = DateTime.Now;
            //var folderPath = Path.Combine(Server.MapPath("~/Recordings/"), dateNow.Year.ToString(), dateNow.Month.ToString(), dateNow.Day.ToString());

            string recordingsPath = Server.MapPath("~/Recordings");
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string day = DateTime.Now.Day.ToString().PadLeft(2,'0');

            string folderPath = Path.Combine(recordingsPath, year, month, day);
            //Tạo thư mục lưu trữ nếu nó chưa tồn tại.
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            //Lưu tập tin vừa tải về vào đường dẫn chỉ định.
            System.IO.File.WriteAllBytes(Path.Combine(folderPath, fileName), data);

            //Trả về đường dẫn tới tệp vừa tải về
            return Path.Combine(folderPath, fileName);
        }
        // LƯU ĐƯỜNG DẪN CHỨA FILE VÀO DATBASE (VERSION 1)
        public async Task<ActionResult> DownloadV1(string downloadUrl)
        {
            using (WebClient client = new WebClient())
            {
                // Tải về tập tin từ URL của API.
                byte[] result = client.DownloadData(downloadUrl);

                // Lưu tập tin vừa tải về vào đường dẫn chỉ định.
                var filePath = SaveFileV1(result);

                using (var db = new CallDbContext())
                {
                    var callHistory = db.CallsHistory.FirstOrDefault(x => x.Download == downloadUrl);
                    if (callHistory != null)
                    {
                        // Cập nhật đường dẫn mới cho cuộc gọi đã có trong database.
                        callHistory.Download = filePath;
                    }
                    else
                    {
                        // Tạo mới đối tượng CallHistory với đường dẫn mới.
                        var entity = new CallHistory
                        {
                            Download = filePath
                        };

                        // Thêm đối tượng mới vào database
                        db.CallsHistory.Add(entity);
                    }

                    // Lưu các thay đổi vào database.
                    await db.SaveChangesAsync();
                }
                // Trả về file vừa tải về để tải xuống và lưu vào thư mục Downloads.
                return File(result, "audio/wav", Path.GetFileName(filePath));
            }
        }
        // LƯU ĐƯỜNG DẪN CHỨA FILE VÀO DATBASE (VERSION 2)
        public async Task<ActionResult> DownloadV2(string downloadUrl)
        {
            using (WebClient client = new WebClient())
            {
                byte[] result = client.DownloadData(downloadUrl);
                int startIndex = downloadUrl.IndexOf("&pkeyID=") + "&pkeyID=".Length;
                int endIndex = downloadUrl.LastIndexOf(".gsm");
                string fileName = downloadUrl.Substring(startIndex, endIndex - startIndex) + ".wav";

                var filePath = SaveFileV2(result, fileName);
                using (var db = new CallDbContext())
                {
                    var callHistory = db.CallsHistory.FirstOrDefault(x => x.Download == downloadUrl);
                    if (callHistory != null)
                    {
                        callHistory.Download = filePath;
                    }
                    else
                    {
                        var entity = new CallHistory
                        {
                            Download = filePath,
                        };

                        db.CallsHistory.Add(entity);
                    }

                    await db.SaveChangesAsync();
                }
                return File(result, "audio/wav", fileName);
            }
        }


    }
}
