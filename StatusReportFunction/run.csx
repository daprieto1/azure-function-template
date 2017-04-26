#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using Microsoft.Win32;

public static async void Run(TimerInfo FactsTimer, CloudTable incidents, TraceWriter log)
{
    await Ping(incidents, DateTime.UtcNow);
}

enum StatusCodes
{
    OK = 200,
    TIMEOUT = 503,
    ERROR = 500,
    UNKNOWN_ERROR = 509,
    HTTP_ERROR = 505
}

private static async Task Ping(CloudTable incidents, DateTime utcDateFact)
{
    var settings = System.Configuration.ConfigurationManager.AppSettings;

    using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(Convert.ToDouble(settings.Get("timeout"))) })
    {
        client
            .GetAsync(new Uri(settings.Get("engineurl")))
            .ContinueWith(async (task) =>
            {
                StatusCodes statusCode = GetStatusCode(task);
                await IncidentEngine(incidents, settings.Get("environment"), utcDateFact, statusCode);
            })
            .Wait();
    }
}

private static StatusCodes GetStatusCode(Task<HttpResponseMessage> task)
{
        StatusCodes statusCode = StatusCodes.OK;

        if (task.IsFaulted)
            statusCode = (task.Exception.GetBaseException() is HttpRequestException) ? StatusCodes.HTTP_ERROR : StatusCodes.UNKNOWN_ERROR;
        else if (task.IsCanceled)
            statusCode = StatusCodes.TIMEOUT;
        else
            statusCode = task.Result.StatusCode == HttpStatusCode.OK ? StatusCodes.OK : StatusCodes.ERROR;

        return statusCode;
}

private static async Task IncidentEngine(CloudTable incidents, string pk, DateTime utcDateFact, StatusCodes statusCode)
{
    var gt = $"ENGINE_{utcDateFact.Year}-{utcDateFact.Month}-{utcDateFact.Day}";
    var rk = $"{gt}-{utcDateFact.Ticks}";

    var results = incidents.ExecuteQuery(new TableQuery<Incidents>().Where($"(PartitionKey eq '{pk}') and ((RowKey gt '{gt}') and (RowKey le '{rk}')) "));

    var incident = GetIncident(results, ((int)statusCode).ToString(), pk, rk, utcDateFact);
    await incidents.ExecuteAsync(TableOperation.InsertOrReplace(incident));
}

private static Incidents GetIncident(IEnumerable<Incidents> results, string statusCode, string pk, string rk, DateTime utcDateFact) 
{
    if (results.Any())
    {
        Incidents lastResult = results.OrderByDescending(r => r.Timestamp).FirstOrDefault();

        if (lastResult.StatusCode == statusCode)
        {
            lastResult.FinalDate = utcDateFact;
            return lastResult;
        }
        else
        {
            return new Incidents { PartitionKey = pk, RowKey = rk, StatusCode = statusCode, InitialDate = utcDateFact, FinalDate = utcDateFact };
        }
    }
    else
    {
        return new Incidents { PartitionKey = pk, RowKey = rk, StatusCode = statusCode, InitialDate = utcDateFact, FinalDate = utcDateFact };
    }
}

public class Incidents : TableEntity
{
    public string StatusCode { get; set; }
    public DateTime InitialDate { get; set; }
    public DateTime FinalDate { get; set; }
}