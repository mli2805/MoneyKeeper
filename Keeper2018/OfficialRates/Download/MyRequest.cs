using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class MyRequest
    {
        public static async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
            Stream stream = response.GetResponseStream();
            if (stream == null) return "";
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}