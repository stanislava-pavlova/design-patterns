using System;
using System.IO;
using System.Web.Http;
using System.Text.Json;
using System.Text;
using System.Net;

namespace DatabaseApp
{
    public class RequestParam
    {
        public string Action { get; set; }
        public dynamic Data { get; set; }
        public RequestParam(string Action, dynamic Data)
        {
            this.Action = Action;
            this.Data = Data;
        }
    }

    sealed class MyApiController : ApiController
    {
        public static Database? database;
        private ContactsHierachy hierarchy;
        private string fileName;

        public MyApiController(string fileName, string rootID)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException($"'{nameof(fileName)}' cannot be null or empty.", nameof(fileName));
            }

            MyApiController.database = Database.GetInstance(fileName);
            this.hierarchy = new ContactsHierachy(database, rootID);
            this.fileName = fileName;
        }

        [HttpGet]
        [Route("api/contacts/get")]
        public HttpResponseMessage PostContacts([FromBody] RequestParam param)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            
            if(param.Action == "get" && database != null)
            {
                List<Contact> contacts = database.RetrieveData("");
                response.Content = new StringContent(JsonSerializer.Serialize(contacts), 
                    Encoding.UTF8, "application/json");
            }
            return response;
        }

        [HttpGet]
        [Route("api/contacts/post")]
        public HttpResponseMessage InsertContacts([FromBody] RequestParam param)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            if (database != null)
            {
                database.StoreRecord(param.Data.name, param.Data.address, param.Data.email, param.Data.phone, param.Data.contacttype);
                response.Content = new StringContent("{'message', 'record inserted'}", Encoding.UTF8, "application/json");
            }
            return response;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            using (Database db = Database.GetInstance("KONTAKTI.db"))
            {
                db.StoreRecord("Rainy", "addressss", "test@email.com", "0864648", "University");
                db.StoreRecord("Sunny", "addressss2", "test2@email.com", "787980", "Work");

                List<Contact> contacts = db.RetrieveData
                    ("University");
                foreach (var item in contacts)
                {
                    Console.WriteLine($"Contact Name: {item.Name}, " +
                        $"Contact Email: {item.Email} " +
                        $"Contact Phone: {item.Phone}");
                }

                ContactsHierachy hierachy = new ContactsHierachy(db, "1");
                WorkContact contact = new WorkContact();
                hierachy.AddChildTo("1", "2", new TreeNode("1", contact));

            }

            MyApiController controller = new MyApiController("KONTAKTI.db", "1");

            Dictionary<string, Delegate> map = new Dictionary<string, Delegate>();

            map.Add("api/contacts/get", new Func<RequestParam, HttpResponseMessage>(controller.PostContacts));

            map.Add("api/contacts/post", new Func<RequestParam, HttpResponseMessage>(controller.InsertContacts));

            HttpListener listener = new HttpListener();
            
            listener.Prefixes.Add("http://localhost:81/api/contacts/get/");
            listener.Prefixes.Add("http://localhost:81/api/contacts/post/");
            listener.Start();
            Console.WriteLine("Listening...");
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                if (!request.HasEntityBody)
                {
                    Console.WriteLine("No client data was sent with the request.");
                    continue;
                }

                System.IO.Stream body = request.InputStream;
                System.Text.Encoding encoding = request.ContentEncoding;
                System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                if (request.ContentType != null)
                {
                    Console.WriteLine("Client data content type {0}", request.ContentType);
                }
                Console.WriteLine("Client data content length {0}", request.ContentLength64);
                Console.WriteLine("Start of client data:");

                // Convert the data to a string and display it on the console.
                string s = reader.ReadToEnd();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                dynamic reqData = JsonSerializer.Deserialize<dynamic>(s);
                if (reqData != null)
                {
                    if (reqData.Action != null && request != null)
                    {
                        RequestParam param = new RequestParam(reqData.Action, reqData.Data);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        if (map.ContainsKey(request.Url.LocalPath))
                        {
                            Delegate method = map[request.Url.LocalPath];
                            HttpResponseMessage response_message = (HttpResponseMessage) method.DynamicInvoke(param);
                            response.ContentType = "application/json";
                            response.StatusCode = 200;
                            Stream output = response.OutputStream;
                            byte[] buffer = new byte[4096];
                            buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response_message.Content));
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        }
                        else
                        {
                            response.StatusCode = 404;
                            response.StatusDescription = "Method not found";
                        }
                        response.Close();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }
                }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
        }
    }
}