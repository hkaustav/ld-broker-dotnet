# LaunchDarkly Integration Project

This project integrates with LaunchDarkly to manage feature flags. It includes an `LDConnector` class that initializes the LaunchDarkly client, listens for flag changes, and provides methods to get flag variations and all flags.

## Files

### `LDConnector.cs`

This file contains the `LDConnector` class, which is responsible for interacting with the LaunchDarkly SDK.

#### Class: `LDConnector`

##### Fields

- `private LdClient? ldClient`: The LaunchDarkly client instance.

##### Methods

- `public void InitLDClient(ConcurrentDictionary<HttpResponse, bool> responseCollection)`

  Initializes the LaunchDarkly client and sets up a listener for flag changes.

  ```csharp
  public void InitLDClient(ConcurrentDictionary<HttpResponse, bool> responseCollection)
  {
      var sdkKey = "sdk-2bb81481-6d1e-403d-ab81-ead8e3ef6cda";
      var context = Context.Builder("test").Build();

      ldClient = new LdClient(sdkKey);

      if (!ldClient.Initialized)
      {
          throw new Exception("LaunchDarkly client failed to initialize.");
      }

      ldClient.FlagTracker.FlagChanged += (s, e) =>
      {
          Console.WriteLine("Flag \"{0}\" has changed to \"{1}\"", e.Key, ldClient.BoolVariation(e.Key, context, false));
          Console.WriteLine("Sending flag change to {0} client(s)...", responseCollection.Count);

          responseCollection.ToList().ForEach(response =>
          {
              try
              {
                  response.Key.WriteAsync($"data: Flag \"{e.Key}\" has changed to \"{ldClient.BoolVariation(e.Key, context, false)}\n\n");
              }
              catch (IOException ex)
              {
                  Console.WriteLine("Error sending flag change: {0}", ex.Message);
                  responseCollection.TryRemove(response);
              }
          });
      };

      Console.WriteLine("Listening for flag changes...");
  }
  
- public bool BoolVariation(string flagKey, bool defaultValue)

    Gets the boolean variation of a feature flag for the given context.

- public FeatureFlagsState GetAllFlags()

    Gets the state of all feature flags for the given context.

### Program.cs
This file contains the main entry point for the application and sets up the web server.

## Configuration

Adds OpenAPI services.
Configures HTTPS redirection.
Initializes the LDConnector and sets up endpoints.

## Endpoints
- `/health`: Returns a health check response.
- `/flag/{name}`: Returns the boolean variation of the specified feature flag.
- `/flags`: Returns the state of all feature flags.
- `/sse`: Sets up a Server-Sent Events (SSE) endpoint to listen for flag changes.