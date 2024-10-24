using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace FirebasePushNotify
{
    internal static class Program
    {
        private const string SCOPE_URL = "https://www.googleapis.com/auth/firebase.messaging";
        private static string PUSH_NOTIF_URL = "https://fcm.googleapis.com/v1/projects/{Project-Name}/messages:send";

        static async Task Main(string[] args)
        {
            await SendDataNotify();
        }

        private static async Task SendDataNotify()
        {
            WebRequest tRequest = WebRequest.Create(PUSH_NOTIF_URL);
            tRequest.Method = "post";
            tRequest.ContentType = "application/json";
            
            var data = new
            {
                message = new
                {
                    token = "xxxxx-xxxx-xxxxx-xxxxxxx-xxxxxx-xxxxx",
                    notification = new
                    {
                        title = "Message from God",
                        body = "You are an lucky fellow!!!"
                    },
                    android = new
                    {
                        ttl = "120s",
                        priority = "high",
                    },
                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            // Use OAuth 2.0 for authorization instead of legacy server key
            string accessToken = await GetAuthToken(); // Implement this method to obtain a valid access token
            tRequest.Headers.Add("Authorization: Bearer " + accessToken);
            tRequest.ContentLength = byteArray.Length;

            using (Stream dataStream = await tRequest.GetRequestStreamAsync())
            {
                await dataStream.WriteAsync(byteArray, 0, byteArray.Length);

                using (WebResponse tResponse = await tRequest.GetResponseAsync())
                {
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    {
                        using (StreamReader tReader = new StreamReader(dataStreamResponse))
                        {
                            _ = await tReader.ReadToEndAsync();
                        }
                    }
                }
            }
        }

        private static async Task<string> GetAuthToken()
        {
            string fileName = Environment.CurrentDirectory.Replace("\\bin\\Debug", "") + "\\GoogleAuth.json"; //Download from Firebase Console ServiceAccount

            string scopes = SCOPE_URL;
            var bearertoken = "";

            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    bearertoken = await GoogleCredential
                        .FromStream(stream)
                        .CreateScoped(scopes)
                      .UnderlyingCredential
                      .GetAccessTokenForRequestAsync();
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.Message);
                }
                return bearertoken;
            }
        }
    }
}
