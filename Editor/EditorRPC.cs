#if UNITY_EDITOR
using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

#region Editor RPC
namespace EditorRPC
{
    #region Activity Extension
    /// <summary>
    /// Activity Extension, ported from act8.DiscordActivityManager
    /// </summary>
    public static class ActivityExtension
    {
        public static Activity AddState(this Activity activity, string value)
        {
            activity.State = value;
            return activity;
        }

        public static Activity AddDetails(this Activity activity, string value)
        {
            activity.Details = value;
            return activity;
        }

        public static Activity AddTimestamps(this Activity activity, ActivityTimestamps value)
        {
            activity.Timestamps = value;
            return activity;
        }

        public static Activity AddAssets(this Activity activity, ActivityAssets value)
        {
            activity.Assets = value;
            return activity;
        }
    }
    #endregion

    #region Editor RPC
    [InitializeOnLoad]
    public static class EditorRPC
    {
        public static Discord.Discord discord { get; private set; }
        static ActivityManager ActivityManager { get; set; }
        static readonly long CLIENT_ID = 838427392871366667;

        static Activity CurrentActivity;

        static string AppName;
        static string SceneName;
        static long StartTime;

        static EditorRPC()
        {
            UnityIntendedDelay();
        }
        static async void UnityIntendedDelay()
        {
            await Task.Delay(1000);
            Init();
        }
        public static void Init()
        {
            StartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            AppName = Application.productName;
            SceneName = EditorSceneManager.GetActiveScene().name;
            SetupDiscord();

            // Update Events
            EditorApplication.update += Update;
            EditorSceneManager.sceneOpened += SceneLoadedCallback;

        }

        static void SetDiscordActivity(Activity activity)
        {
            if (discord == null) return;

            ActivityManager.UpdateActivity(activity, (result) =>
            {
                if (result == Result.Ok) CurrentActivity = activity;
                else Debug.LogWarning("Discord was unable to set activity: " + result.ToString());
            }
            ); 
        }

        static void UpdateActivity()
        {
            SetDiscordActivity(new Activity().AddState($"on scene {SceneName}").AddDetails($"In project {AppName}").AddTimestamps(new ActivityTimestamps() { Start = StartTime }).AddAssets(new ActivityAssets() { LargeImage = "unity-black", LargeText = AppName }));
        }

        static void SetupDiscord()
        {
            try
            {
                discord = new Discord.Discord(CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
                ActivityManager = discord.GetActivityManager();
                UpdateActivity();
            }
            catch (Exception e)
            { Debug.LogWarning($"Failed to initiate Discord session: {e.Message}"); }
        }

        static void SceneLoadedCallback(Scene scene, OpenSceneMode openMode)
        {
            SceneName = scene.name;
            UpdateActivity();
        }

        private static void Update()
        {
            if (discord != null)
                discord.RunCallbacks();
        }
    }
    #endregion
}
#endregion
#endif
