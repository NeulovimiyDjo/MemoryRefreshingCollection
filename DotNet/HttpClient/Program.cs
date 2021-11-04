using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace AppName
{
  class Program
  {
    static void Main(string[] args)
    {
      string url = "http://localhost:5000";

      //saveChar(url);

      //printCharacters(url);

      getCharacter(url);
    }

    static void getCharacter(string url)
    {
      string guid = "5dc3c20f-2767-4af9-7e2e-08d69800afb3";

      var client = new HttpClient();
      var request = new HttpRequestMessage{
        RequestUri = new Uri(url + "/api/planner/getcharacter?guid=" + guid),
        Method = HttpMethod.Get
      };

      
      request.Headers.Add("Accept", "*/*");
      request.Headers.Add("Referer", url + '/');
      request.Headers.Add("Accept-Language", "ru-Ru");
      request.Headers.Add("Accept-Encoding", "gzip, deflate");
      request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");
      request.Headers.Add("Connection", "Keep-Alive");
      request.Headers.Add("DNT", "1");


      var response = client.SendAsync(request).Result.Content.ReadAsStringAsync();


      Console.WriteLine(response.Result);
    }

    static void printCharacters(string url)
    {
      var client = new HttpClient();
      var request = client.GetAsync(url + "/api/planner/getcharacters");   
      var response = request.Result.Content.ReadAsStringAsync();

      Console.WriteLine(response.Result);
    }

    static void saveChar(string url)
    {
      string json = "{'stats':[8,14,11,11,8,15],'abilities':[22,-1,-1],'feats':[22,22,22,-1],'spells':[-1,1009,-1,1045,-1]}";
      
      var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

      var client = new HttpClient();
      var request = client.PostAsync(url + "/api/planner/savecharacter", content);   
      var response = request.Result.Content.ReadAsStringAsync();

      Console.WriteLine(response.Result);
    }
  }
}
