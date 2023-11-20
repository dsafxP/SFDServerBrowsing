using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SFDServerBrowsing;

internal class Program {
  static async Task Main() {
    using HttpClient httpClient = new();

    string url = "http://mythologicinteractive.com/SFDGameServices.asmx";
    string soapRequest = @"<?xml version='1.0' encoding='utf-8'?>
<soap12:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap12='http://www.w3.org/2003/05/soap-envelope'>
  <soap12:Body>
    <GetGameServers xmlns='https://mythologicinteractive.com/Games/SFD/'>
      <validationToken></validationToken>
    </GetGameServers>
  </soap12:Body>
</soap12:Envelope>";
    var content = new StringContent(soapRequest, System.Text.Encoding.UTF8, "application/soap+xml");
    HttpResponseMessage response = await httpClient.PostAsync(url, content);

    Console.CursorVisible = false;

    if (!response.IsSuccessStatusCode) {
      Console.WriteLine($"HTTP Request Failed with status code {response.StatusCode}");
      Console.WriteLine("Press any key to close this window...");
      Console.ReadKey();
    }

    while (response.IsSuccessStatusCode) {
      int originalWindowTop = Console.WindowTop;

      Console.Clear();

      StringBuilder outputBuffer = new StringBuilder();

      outputBuffer.AppendLine("Server Information:");
      outputBuffer.AppendLine("--------------------");

      string responseContent = await response.Content.ReadAsStringAsync();
      XDocument soapResponse = XDocument.Parse(responseContent);
      var gameServers = soapResponse.Descendants("{https://mythologicinteractive.com/Games/SFD/}SFDGameServer")
          .Select(server => new {
            AddressIPv4 = server.Element("{https://mythologicinteractive.com/Games/SFD/}AddressIPv4")!.Value,
            Port = server.Element("{https://mythologicinteractive.com/Games/SFD/}Port")!.Value,
            GameName = server.Element("{https://mythologicinteractive.com/Games/SFD/}GameName")!.Value,
            Players = Convert.ToUInt16(server.Element("{https://mythologicinteractive.com/Games/SFD/}Players")!.Value),
            Bots = Convert.ToUInt16(server.Element("{https://mythologicinteractive.com/Games/SFD/}Bots")!.Value),
            Version = server.Element("{https://mythologicinteractive.com/Games/SFD/}Version")!.Value,
            MaxPlayers = Convert.ToUInt16(server.Element("{https://mythologicinteractive.com/Games/SFD/}MaxPlayers")!.Value),
            Protected = Convert.ToBoolean(server.Element("{https://mythologicinteractive.com/Games/SFD/}HasPassword")!.Value),
            MapName = server.Element("{https://mythologicinteractive.com/Games/SFD/}MapName")!.Value,
            Description = server.Element("{https://mythologicinteractive.com/Games/SFD/}Description")!.Value
          });

      foreach (var server in gameServers) {
        outputBuffer.AppendLine($"Address: {server.AddressIPv4}");
        outputBuffer.AppendLine($"Port: {server.Port}");
        outputBuffer.AppendLine($"Game Name: {server.GameName}");
        outputBuffer.AppendLine($"Players: {server.Players}{(server.Bots == 0 ? string.Empty : $"+{server.Bots}")}/{server.MaxPlayers}");
        outputBuffer.AppendLine($"Protected: {server.Protected}");
        outputBuffer.AppendLine($"Map Name: {server.MapName}");
        outputBuffer.AppendLine($"Version: {server.Version}");
        outputBuffer.AppendLine($"Description: {server.Description}");
        outputBuffer.AppendLine("--------------------");
      }

      Console.Write(outputBuffer.ToString());

      Console.WindowTop = originalWindowTop;

      await Task.Delay(5000);
    }
  }
}