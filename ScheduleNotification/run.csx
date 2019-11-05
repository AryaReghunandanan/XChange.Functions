#r "Newtonsoft.Json"

using System.Net;
using System.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Microsoft.Azure.NotificationHubs;
using System.Threading.Tasks;

public static async Task<IActionResult> Run(HttpRequestMessage req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    string body = await req.Content.ReadAsStringAsync();
    dynamic data = JObject.Parse(body);

    string title = data.title;
    string message = data.message;
    string time = data.time;

    var notificationHubSas = "Endpoint=sb://appnotificationtest007.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=w6dJyRPvS7heX02JgHLqCNxahWChrHiM2aA1yopLZ3Q=";
    var notificationHubName = "testnotificationhub007";
    
    var hub = NotificationHubClient.CreateClientFromConnectionString(notificationHubSas, notificationHubName);
    
    var tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
    DateTime dt = Convert.ToDateTime(time);
    dt = DateTime.SpecifyKind(dt,DateTimeKind.Unspecified);

    // Define an iOS alert  
    /*var iOSalert = 
        "{\"aps\":{\"alert\":\""+ message + "\", \"badge\":" + badgeValue + ", \"sound\":\"default\"},"
        + "\"inAppMessage\":\"" + message + "\"}";
    */
    //var iOSalert = $"\";

    //hub.SendAppleNativeNotificationAsync(iOSalert).Wait();

    // Define an Anroid alert.
    var androidPayload = "{\"data\":{\"message\": \"" + message + "\",\"title\": \"" + title +"\"}}";
    Notification androidNotification = new FcmNotification(androidPayload);
    
    try {
        var outcome = await hub.ScheduleNotificationAsync(androidNotification, dt);
        return new OkObjectResult(new {ScheduledNotificaionId = outcome.ScheduledNotificationId, payload = outcome.Payload}); // 200
    } catch (Exception ex) {
        return new BadRequestObjectResult("Invalid Time" + ex); // 400
    }

}
