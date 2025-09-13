using System.Data;
using Discord;
using UnityEngine;

public class DiscordRPC : MonoBehaviour
{
    public long applicationID;
    public string details = "Main Menu";
    public string largeImage = "mini";
    public string largeText = "Playing Minimized";

    private Rigidbody rb;
    private long time;

    private static bool instanceExists;
    public Discord.Discord discord;

    private void Awake()
    {
        if (!instanceExists)
        {
            instanceExists = true;
            DontDestroyOnLoad(gameObject);
        }
        else if (FindObjectsByType(GetType(), FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

        UpdateStatus();
    }

    private void Update()
    {
        try
        {
            discord.RunCallbacks();
        }
        catch
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        try
        {
            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                Details = details,
                Assets =
                {
                    LargeImage = largeImage,
                    LargeText = largeText
                },
                Timestamps =
                {
                    Start = time
                }
            };

            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res != Discord.Result.Ok) Debug.LogWarning("Failed to connecting Discord");
            });
        }
        catch
        {
            Destroy(gameObject);
        }
    }
}