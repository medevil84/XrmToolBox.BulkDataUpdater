﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

public class AppInsights
{
    private readonly AiConfig _aiConfig;
    private int seq = 1;

    /*Envelope names
    * Microsoft.ApplicationInsights.SessionState
    * Microsoft.ApplicationInsights.PerformanceCounter
    * Microsoft.ApplicationInsights.Message - Trace data
    * Microsoft.ApplicationInsights.Exception - Handled or unhandled exceptions
    * Microsoft.ApplicationInsights.Metric - Single or pre-aggregated metrics
    * Microsoft.ApplicationInsights.Event - Events that occurred in the application
    * Microsoft.ApplicationInsights.RemoteDependency - Interaction with a remote component
    * Microsoft.ApplicationInsights.Request - Events trigger by an external request
    */

    public AppInsights(AiConfig aiConfig)
    {
        _aiConfig = aiConfig;
    }

    public AppInsights(string endpoint, string ikey) : this(new AiConfig(endpoint, ikey)) { }

    public void WriteMessage(string message, AiTraceSeverity severity, Action<string> resultHandler = null)
    {
        if (!_aiConfig.LogTraces) return;
        var logRequest = GetLogRequest("Message");
        logRequest.Data.BaseData.Message = message;
        logRequest.Data.BaseData.SeverityLevel = severity.ToString();
        var json = Serialization.SerializeRequest<AiLogRequest>(logRequest);
        SendToAi(json, resultHandler);
    }

    public void WriteEvent(string eventName, double? count = null, double? duration = null, Action<string> resultHandler = null)
    {
        if (!_aiConfig.LogEvents) return;
        var logRequest = GetLogRequest("Event");
        logRequest.Data.BaseData.Name = eventName;
        if (count != null || duration != null)
        {
            logRequest.Data.BaseData.Measurements = new AiMeasurements
            {
                Count = count ?? 0,
                Duration = duration ?? 0
            };
        }
        var json = Serialization.SerializeRequest<AiLogRequest>(logRequest);
        SendToAi(json, resultHandler);
    }

    public void WriteException(Exception exception, AiExceptionSeverity severity, Action<string> resultHandler = null)
    {
        if (!_aiConfig.LogExceptions) return;
        var logRequest = GetLogRequest("Exception");
        logRequest.Data.BaseData.Exceptions = new List<AiException> { ExceptionHelper.GetAiException(exception) };
        logRequest.Data.BaseData.SeverityLevel = severity.ToString();
        var json = Serialization.SerializeRequest<AiLogRequest>(logRequest);
        SendToAi(json, resultHandler);
    }

    public void WriteMetric(string methodName, double duration, Action<string> resultHandler = null)
    {
        if (!_aiConfig.LogMetrics) return;
        var logRequest = GetLogRequest("Metric");
        logRequest.Data.BaseData.Metrics = new List<AiDataPoint> {
            new AiDataPoint {
                Kind = DataPointType.Measurement,
                Name = methodName,
                Value = duration
            }
        };
        var json = Serialization.SerializeRequest<AiLogRequest>(logRequest);
        SendToAi(json, resultHandler);
    }

    private async void SendToAi(string json, Action<string> handleresult = null)
    {
        var result = string.Empty;
#if DEBUG
#else
        try
        {
            using (HttpClient client = HttpHelper.GetHttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/x-json-stream");
                var response = await client.PostAsync(_aiConfig.AiEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    result = response.ToString();
                }
            }
        }
        catch (Exception e)
        {
            result = e.ToString();
        }
#endif
        handleresult?.Invoke(result);
    }

    private AiLogRequest GetLogRequest(string action)
    {
        return new AiLogRequest(seq++, _aiConfig, action);
    }
}