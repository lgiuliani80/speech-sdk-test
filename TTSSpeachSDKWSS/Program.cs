using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Configuration;
using System.Reflection;

var hb = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                   .AddUserSecrets(Assembly.GetExecutingAssembly())
                                   .AddEnvironmentVariables()
                                   .AddCommandLine(args);

var config = hb.Build();

// Replace with your Azure Speech service subscription key and region
string? subscriptionKey = config["Speech:SubscriptionKey"];
string? authToken = config["Speech:AuthorizationToken"];
string endpoint = config["Speech:Endpoint"]!;

var ssc = authToken switch {
    null => SpeechConfig.FromEndpoint(new Uri(endpoint), subscriptionKey),
    _ => SpeechConfig.FromEndpoint(new Uri(endpoint))
};

ssc.SpeechRecognitionLanguage = config["Speech:Language"] ?? "en-US";

if (authToken is not null)
    ssc.AuthorizationToken = authToken;

using var recognizer = new SpeechRecognizer(ssc);

Console.WriteLine($"Say something in {ssc.SpeechRecognitionLanguage}...");

recognizer.Recognizing += (s, e) =>
{
    Console.WriteLine($"Recognizing: {e.Result.Text}");
};

recognizer.Recognized += (s, e) =>
{
    if (e.Result.Reason == ResultReason.RecognizedSpeech)
    {
        Console.WriteLine($"Recognized: {e.Result.Text}");
    }
    else if (e.Result.Reason == ResultReason.NoMatch)
    {
        Console.WriteLine("No speech could be recognized.");
    }
};

recognizer.Canceled += (s, e) =>
{
    Console.WriteLine($"Canceled: Reason={e.Reason}");
    if (e.Reason == CancellationReason.Error)
    {
        Console.WriteLine($"ErrorDetails={e.ErrorDetails}");
    }
};

recognizer.SessionStopped += (s, e) =>
{
    Console.WriteLine("Session stopped.");
};

await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

Console.WriteLine("Press any key to stop...");
Console.ReadKey();

await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
