using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;

if (!TestCRuntime.IsVC14RuntimeInstalled())
{
    Console.Error.WriteLine("The Visual C++ 2015-2019 Redistributable is not installed. Please install it from https://aka.ms/vs/16/release/vc_redist.x64.exe");
    return;
}

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

if (config["Proxy:Host"] is string proxyHost && 
    config["Proxy:Port"] is string proxyPort && 
    int.TryParse(proxyPort, out int port))
{
    ssc.SetProxy(proxyHost, port);
}

ssc.SpeechRecognitionLanguage = config["Speech:Language"] ?? "en-US";

if (authToken is not null)
    ssc.AuthorizationToken = authToken;

switch (config.GetValue("Speech:Type", "sst"))
{
    case "stt":
        {
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
        }
        break;

    case "tts":
        {
            using var synthesizer = new SpeechSynthesizer(ssc);
            Console.WriteLine("Enter text to synthesize (or type 'exit' to quit):");
            string? input;
            while ((input = Console.ReadLine()) != null && input.ToLower() != "exit")
            {
                using var result = await synthesizer.SpeakTextAsync(input).ConfigureAwait(false);
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    Console.WriteLine("Speech synthesized successfully.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    Console.WriteLine($"Cancelled");
                }

                Console.WriteLine("Enter text to synthesize (or type 'exit' to quit):");
            }
        }
        break;
}

static class TestCRuntime
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    public static bool IsVC14RuntimeInstalled()
    {
        // This check is specific to Windows
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return true;
        }

        // Check if the Visual C++ 2015-2019 Redistributable is installed
        IntPtr handle = LoadLibrary("MSVCP140.dll");
        
        return handle != IntPtr.Zero;
    }
}