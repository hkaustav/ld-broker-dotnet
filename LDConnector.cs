using System;
using System.Collections;
using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Server;
using System.Linq;
using System.Collections.Concurrent;

class LDConnector
{
    private LdClient? ldClient;
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

    public bool BoolVariation(string flagKey, bool defaultValue)
    {
        if (ldClient == null)
        {
            Console.WriteLine("LaunchDarkly client not initialized.");
            return defaultValue;
        }
        var context = Context.Builder("test").Build();
        return ldClient.BoolVariation(flagKey, context, defaultValue);
    }

    public FeatureFlagsState GetAllFlags()
    {
        if (ldClient == null)
        {
            Console.WriteLine("LaunchDarkly client not initialized.");
            throw new Exception("LaunchDarkly client not initialized.");
        }
        var flags = ldClient.AllFlagsState(Context.Builder("test").Build());

        return flags;
    }
}
