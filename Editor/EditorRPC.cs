using Discord;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorRPC
{
	#region Extensions

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

	#region Timestamps Extension

	/// <summary>
	/// Activity Timestamps Extension, ported from act8.DiscordActivityManager
	/// </summary>
	public static class ActivityTimestampsExtension
	{
		public static ActivityTimestamps AddStart(this ActivityTimestamps timestamps, long start)
		{
			timestamps.Start = start;
			return timestamps;
		}

		public static ActivityTimestamps AddEnd(this ActivityTimestamps timestamps, long end)
		{
			timestamps.End = end;
			return timestamps;
		}
	}
	#endregion

	#region Assets Extension

	/// <summary>
	/// Activity Assets Extension, ported from act8.DiscordActivityManager
	/// </summary>
	public static class ActivityAssetsExtension
	{
		public static ActivityAssets AddLargeImage(this ActivityAssets assets, string largeImage)
		{
			assets.LargeImage = largeImage;
			return assets;
		}

		public static ActivityAssets AddLargeText(this ActivityAssets assets, string largeText)
		{
			assets.LargeText = largeText;
			return assets;
		}

		public static ActivityAssets AddSmallImage(this ActivityAssets assets, string smallImage)
		{
			assets.SmallImage = smallImage;
			return assets;
		}

		public static ActivityAssets AddSmallText(this ActivityAssets assets, string smallText)
		{
			assets.SmallText = smallText;
			return assets;
		}
	}
	#endregion

	#endregion

	#region Editor RPC

	[InitializeOnLoad]
	public static class EditorRPC
	{
		static Discord.Discord DiscordInstance;
		static ActivityManager ActivityManager;
		static readonly long CLIENT_ID = 838427392871366667;

		static string AppName;
		static string SceneName;
		static long StartTime;

		static EditorRPC()
		{
			AsyncInit();
		}

		static async void AsyncInit()
		{
			await Task.Delay(1000);
			Init();
		}

		private static void Init()
		{
			StartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
			AppName = Application.productName;
			SceneName = SceneManager.GetActiveScene().name;

			SetupDiscord();

			EditorApplication.update += Update;
			EditorSceneManager.sceneOpened += OnSceneOpened;
		}

		private static void SetupDiscord()
		{
			try
			{
				DiscordInstance = new Discord.Discord(CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
				ActivityManager = DiscordInstance.GetActivityManager();
				UpdateActivity();
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Failed to initiate Discord session: {e.Message}");
			}
		}

		private static void Update()
		{
			if (DiscordInstance != null) DiscordInstance.RunCallbacks();
		}

		private static void OnSceneOpened(Scene scene, OpenSceneMode openMode)
		{
			AppName = Application.productName;
			SceneName = scene.name;
			UpdateActivity();
		}

		private static void UpdateActivity()
		{
			SetDiscordActivity(new Activity()
				.AddState($"on scene {SceneName}")
				.AddDetails($"In project {AppName}")
				.AddTimestamps(new ActivityTimestamps().AddStart(StartTime))
				.AddAssets(new ActivityAssets().AddLargeImage("unity-black").AddLargeText(AppName)));
		}

		private static void SetDiscordActivity(Activity activity)
		{
			if (DiscordInstance == null) return;

			ActivityManager.UpdateActivity(activity, (result) =>
			{
				if (result != Result.Ok) Debug.LogWarning("Discord was unable to set activity: " + result.ToString());
			}
			);
		}
	}

	#endregion
}