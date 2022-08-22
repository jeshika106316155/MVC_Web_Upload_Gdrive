using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Net;
using static System.Drawing.Image;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Windows;
using EO.WebBrowser.DOM;
using System.Windows.Forms;

namespace WebUploadtoGdrive.Controllers
{
    public class UploadController : Controller
    {
        static string[] Scopes = { DriveService.Scope.Drive, DriveService.Scope.DriveFile };
        static string ApplicationName = "Drive API .NET Quickstart";
        private const string PathToServiceAccountKeyFile = @"D:\Jeshika\Other\Xiao_Laoshi\MVC_Web_Upload_Gdrive\MVC_Web_Upload_Gdrive\WebUploadtoGdrive\WebUploadtoGdrive\Json\credentials.json";
        //public static string UploadFileName = @"D:\Jeshika\Other\Xiao_Laoshi\ConsoleTestDriveApi\ConsoleTestDriveApi\fhir_logo.png"; //"D:\\Jeshika\\Other\\Xiao_Laoshi\\ConsoleTestDriveApi\\ConsoleTestDriveApi\\test_hello.txt";
        public static string mediaJson = @"D:\Jeshika\Other\Xiao_Laoshi\ConsoleTestDriveApi\ConsoleTestDriveApi\FHIR_Media.json";
        public static string fhirUrl = "https://hapi.fhir.org/baseR4/Media/";
        public static string folderName = "SLI_UploadImage";
        public DriveService service;
        public UserCredential credential;
        
        // GET: Upload
        public ActionResult Index()
        {
            // Load client secrets.
            using (var stream =
                   new FileStream(PathToServiceAccountKeyFile, FileMode.Open, FileAccess.Read))
            {

                /* The file token.json stores the user's access and refresh tokens, and is created
                 automatically when the authorization flow completes for the first time. */
                string credPath = @"D:\Jeshika\Other\Xiao_Laoshi\MVC_Web_Upload_Gdrive\MVC_Web_Upload_Gdrive\WebUploadtoGdrive\WebUploadtoGdrive\Json\token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            return View();
        }
        /// <summary>
        /// Run: Each time UploadFile.cshmtl open
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UploadFile()
        {
            // Check whether page contain user information from login web
            string url = Request.Url.AbsoluteUri;
            Uri myUri = new Uri(url);
            string user = HttpUtility.ParseQueryString(myUri.Query).Get("User");
            if(user == "" || user == null){
                // Redirect to login website get user authentication
                Response.Redirect("Page2CS.aspx", false);
            }
            else
            {
                // User authenticated, make google drive credential
            }

            Session["UserName"] = "b";

            // Load client secrets.
            using (var stream =
                   new FileStream(PathToServiceAccountKeyFile, FileMode.Open, FileAccess.Read))
            {
                /* The file token.json stores the user's access and refresh tokens, and is created
                 automatically when the authorization flow completes for the first time. */

                string credPath = @"D:\Jeshika\Other\Xiao_Laoshi\MVC_Web_Upload_Gdrive\MVC_Web_Upload_Gdrive\WebUploadtoGdrive\WebUploadtoGdrive\Json\token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    Session["UserName"].ToString(),
                    CancellationToken.None,
                    null).Result;
                Console.WriteLine("Credential file saved to: " + credPath);

                // Create Drive API service.
                service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });
            }
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> UploadFile(HttpPostedFileBase file)
        {
            try
            {

                var reqMessage = "";
                if (file.ContentLength > 0)
                {
                    using (var stream =
                    new FileStream(PathToServiceAccountKeyFile, FileMode.Open, FileAccess.Read))
                    {
                        /* The file token.json stores the user's access and refresh tokens, and is created
                         automatically when the authorization flow completes for the first time. */
                        string credPath = @"D:\Jeshika\Other\Xiao_Laoshi\MVC_Web_Upload_Gdrive\MVC_Web_Upload_Gdrive\WebUploadtoGdrive\WebUploadtoGdrive\Json\token.json";
                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromStream(stream).Secrets,
                            Scopes,
                            Session["UserName"].ToString(),
                            CancellationToken.None,
                            null).Result;
                        Console.WriteLine("Credential file saved to: " + credPath);
                    }

                    string _FileName = Path.GetFileName(file.FileName);
                    HttpPostedFileBase UploadFileName = file;
                    // Create Drive API service.
                    var service = new DriveService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = ApplicationName
                    });

                    // Create Folder
                    string folderId = CreateFolder(service, folderName);

                    // Upload file Metadata
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = "Test",
                        Parents = new List<string>() { folderId } //"1trDvdnurc7YkTJc-BqdeIBUMVYAIHhfM"
                    };

                    //string uploadedFileId;
                    // Create a new file on Google Drive

                    // 2 method
                    Google.Apis.Drive.v3.Data.File deliverable = new Google.Apis.Drive.v3.Data.File();
                    deliverable.Parents = new List<string>();
                    deliverable.Name = file.FileName;
                    deliverable.Parents.Add(folderId);
                    FilesResource.CreateMediaUpload request = service.Files.Create(deliverable, file.InputStream, file.ContentType);
                    request.Fields = "*";
                    var results = request.Upload();
                    if (results.Status == Google.Apis.Upload.UploadStatus.Failed) // == UploadStatus.Failed
                    {
                        Console.WriteLine($"Error uploading file: {results.Exception.Message}");
                        reqMessage = results.Exception.Message;
                    }
                    else
                    {
                        StreamReader r = new StreamReader(mediaJson);

                        JObject data = JObject.Parse(r.ReadToEnd());

                        System.Drawing.Image image = System.Drawing.Image.FromStream(file.InputStream);
                        var size = image.Height.ToString();
                        data["identifier"][0]["system"] = request.ResponseBody.WebContentLink;
                        data["content"]["url"] = request.ResponseBody.WebViewLink;
                        data["content"]["contentType"] = request.ContentType;
                        data["content"]["title"] = request.ResponseBody.Id;
                        data["content"]["creation"] = request.ResponseBody.CreatedTime;
                        data["identifier"][0]["value"] = request.ResponseBody.Id;
                        data["height"] = image.Height.ToString();
                        data["width"] = image.Width.ToString();
                        data["issued"] = DateTime.Now;
                        data["content"]["size"] = file.ContentLength;

                        try{
                            var requestHttp = (HttpWebRequest)WebRequest.Create(fhirUrl);
                            requestHttp.ContentType = "application/json";
                            requestHttp.Method = "POST";

                            using (var streamWriter = new StreamWriter(requestHttp.GetRequestStream()))
                            {
                                streamWriter.Write(data);
                            }

                            var response = (HttpWebResponse)requestHttp.GetResponse();
                            if (response.StatusCode == HttpStatusCode.Created)
                            {
                                using (var streamReader = new StreamReader(response.GetResponseStream()))
                                {
                                    var result = streamReader.ReadToEnd();
                                    JObject resultJson = JObject.Parse(result);
                                    var id = resultJson["id"];
                                    //Response.Redirect("https://hapi.fhir.org/baseR4/Media/" + id);
                                    reqMessage = "File Uploaded Successfully!!";
                                    ViewBag.FHIRResUrl = "https://hapi.fhir.org/baseR4/Media/" + id;
                                }
                            }
                            else { reqMessage = "Error upload to FHIR Server!"; }
                        }
                        catch (Exception e)
                        {

                        }
                        
                        
                    }
                    //await using (var fsSource = new FileStream(file, FileMode.Open, FileAccess.Read))
                    //{
                    //    // Create a new file, with metadata and stream.
                    //    var request = service.Files.Create(fileMetadata, fsSource, GetMimeType(UploadFileName));
                    //    request.Fields = "*";
                    //    var results = await request.UploadAsync(CancellationToken.None);

                    //    if (results.Status == Google.Apis.Upload.UploadStatus.Failed) // == UploadStatus.Failed
                    //    {
                    //        Console.WriteLine($"Error uploading file: {results.Exception.Message}");
                    //    }
                    //    else
                    //    {
                    //        JObject data = JObject.Parse(File.ReadAllText(mediaJson));

                    //        System.Drawing.Image image = System.Drawing.Image.FromStream(fsSource);

                    //        var size = image.Height.ToString();
                    //        data["identifier"][0]["system"] = request.ResponseBody.WebContentLink;
                    //        data["content"]["url"] = request.ResponseBody.WebViewLink;
                    //        data["content"]["contentType"] = request.ContentType;
                    //        data["content"]["title"] = request.ResponseBody.Id;
                    //        data["content"]["creation"] = request.ResponseBody.CreatedTime;
                    //        data["identifier"][0]["value"] = request.ResponseBody.Id;
                    //        data["height"] = image.Height.ToString();
                    //        data["width"] = image.Width.ToString();
                    //        data["issued"] = DateTime.Now;
                    //        data["content"]["size"] = fsSource.Length;

                    //        var requestHttp = (HttpWebRequest)WebRequest.Create(fhirUrl);
                    //        requestHttp.ContentType = "application/json";
                    //        requestHttp.Method = "POST";

                    //        using (var streamWriter = new StreamWriter(requestHttp.GetRequestStream()))
                    //        {
                    //            streamWriter.Write(data);
                    //        }

                    //        var response = (HttpWebResponse)requestHttp.GetResponse();
                    //        using (var streamReader = new StreamReader(response.GetResponseStream()))
                    //        {
                    //            var result = streamReader.ReadToEnd();
                    //            JObject resultJson = JObject.Parse(result);
                    //            var id = resultJson["id"];
                    //            Console.WriteLine("https://hapi.fhir.org/baseR4/Media/" + id);
                    //        }
                    //    }
                    //    uploadedFileId = request.ResponseBody?.Id;
                    //}


                    //string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
                    //file.SaveAs(_path);
                }
                ViewBag.Message = reqMessage;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "File upload failed!! "+ex;
                return View();
            }
        }

        public string CreateFolder(DriveService service, string folderName)
        {
            string folderId = Exists(service, folderName);
            if (folderId != "None")
            { return folderId; }

            var file = new Google.Apis.Drive.v3.Data.File();
            file.Name = folderName;
            file.MimeType = "application/vnd.google-apps.folder";
            var request = service.Files.Create(file);
            request.Fields = "id";
            return request.Execute().Id;
        }
        private string Exists(DriveService service, string name)
        {
            

            var listRequest = service.Files.List();
            listRequest.PageSize = 100;
            listRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{name}' and 'root' in parents and trashed = false";
            listRequest.Fields = "files(id,name)";
            var files = listRequest.Execute().Files;

            foreach (var file in files)
            {
                if (name == file.Name)
                { return file.Id; }
            }
            return "None";
        }
    }
}