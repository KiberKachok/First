using UnityEngine;

using AlmostEngine.Screenshot;

namespace AlmostEngine.Preview
{
	public class DeviceInfo
	{
		#if UNITY_EDITOR
		public static ScreenshotResolution m_CurrentDevice;
		public static bool m_IsLivePreview = false;
		#endif

		// TODO: add delegate

		#region Device info

		public static string GetName ()
		{
			#if UNITY_EDITOR
			if (m_CurrentDevice != null) {
				return m_CurrentDevice.m_ResolutionName;
			}
			#endif

			return SystemInfo.deviceModel;
		}

		#endregion


		#region Screen info

		public static Vector2 GetResolution ()
		{
			#if UNITY_EDITOR
			if (m_CurrentDevice != null) {
				return new Vector2 (m_CurrentDevice.ComputeTargetWidth (), m_CurrentDevice.ComputeTargetHeight ());
			}
			#endif

			return new Vector2 (Screen.width, Screen.height);
		}

		public static int GetDPI ()
		{
			#if UNITY_EDITOR
			if (m_CurrentDevice != null) {
				return m_CurrentDevice.m_PPI;
			}
			#endif

			return (int)Screen.dpi;
		}

		public static bool IsPortrait ()
		{
			#if UNITY_EDITOR
			if (m_CurrentDevice != null) {
				return m_CurrentDevice.m_Orientation == ScreenshotResolution.Orientation.PORTRAIT;
			}
			#endif

			return Screen.height > Screen.width;
		}

		#endregion

		#region Safe Area

		#if UNITY_EDITOR
		public static bool HasSimulatedSafeAreaValue ()
		{
			return GetSimulatedSafeArea () != new Rect ();
			;
		}

		public static Rect GetSimulatedSafeArea ()
		{
			if (m_CurrentDevice != null) {
				if (m_CurrentDevice.m_Orientation == ScreenshotResolution.Orientation.LANDSCAPE) {
					return m_CurrentDevice.m_SafeAreaLandscapeLeft;
				}
				if (m_CurrentDevice.m_Orientation == ScreenshotResolution.Orientation.PORTRAIT) {
					return m_CurrentDevice.m_SafeAreaPortrait;
				}	
			}
			return new Rect ();
		}

		#endif

		public static Rect GetSafeArea ()
		{
			Rect safeArea = Screen.safeArea;

			#if UNITY_EDITOR
			safeArea = GetSimulatedSafeArea ();
			#endif

			if (safeArea == new Rect ()) {
				safeArea = new Rect (0f, 0f, Screen.width, Screen.height);
			}
			return safeArea;
		}

		#endregion

		#region Platforms

		public static bool IsIOS ()
		{
			#if UNITY_EDITOR
			if (m_CurrentDevice != null) {
				return m_CurrentDevice.m_Platform.Contains ("iOS");
			}
			#endif

			#if UNITY_IOS
			return true;
			#else 
			return false;
			#endif
		}

		public static bool IsAndroid ()
		{
			#if UNITY_EDITOR
			if (m_CurrentDevice != null) {
				return m_CurrentDevice.m_Platform.Contains ("Android");
			}
			#endif

			#if UNITY_ANDROID
			return true;
			#else 
			return false;
			#endif
		}

		public static bool IsStandalone ()
		{
			#if UNITY_EDITOR
			if (m_CurrentDevice != null) {
				return m_CurrentDevice.m_Platform.Contains ("Standalone");
			}
			#endif

			#if UNITY_STANDALONE
			return true;
			#else 
			return false;
			#endif
		}

		#endregion
	}
}

