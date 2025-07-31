# TTSSpeachSDKWSS

A .NET 8 console application that uses Azure Cognitive Services Speech SDK to perform speech recognition.

## Usage

You can run the program by passing all configuration values as command line parameters. The following parameters are supported:

- `--Speech:SubscriptionKey <key>`: Your Azure Speech service subscription key (required if not using AuthorizationToken)
- `--Speech:AuthorizationToken <token>`: Azure Speech service authorization token (optional, alternative to SubscriptionKey)
- `--Speech:Endpoint <endpoint>`: The endpoint URL for your Azure Speech service (required)
- `--Speech:Language <language>`: The language code for speech recognition (optional, default: `en-US`)
- `--Proxy:Host <host>`: Proxy host (optional); use 'localhost' for Fiddler
- `--Proxy:Port <port>`: Proxy port (optional); use '8888' for Fiddler

### Example

```
./TTSSpeachSDKWSS.exe --Speech:SubscriptionKey YOUR_KEY --Speech:Endpoint https://YOUR_REGION.stt.speech.microsoft.com --Speech:Language en-US
```

Or, using an authorization token:

```
./TTSSpeachSDKWSS.exe --Speech:AuthorizationToken YOUR_TOKEN --Speech:Endpoint https://YOUR_REGION.stt.speech.microsoft.com --Speech:Language en-US
```

With proxy settings:

```
./TTSSpeachSDKWSS.exe --Speech:SubscriptionKey YOUR_KEY --Speech:Endpoint https://YOUR_REGION.stt.speech.microsoft.com --Proxy:Host proxy.example.com --Proxy:Port 8080
```

## Notes
- If both `SubscriptionKey` and `AuthorizationToken` are provided, `AuthorizationToken` takes precedence.
- The endpoint must be a valid Azure Speech endpoint URL.
- If no language is specified, the default is `en-US`.
- Proxy settings are optional and only needed if your network requires them.

## Output
- The program will prompt you to speak and display recognized text in the console.
- Press any key to stop the recognition session.
