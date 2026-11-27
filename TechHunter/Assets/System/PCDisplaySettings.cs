using UnityEngine;

/// <summary>
/// Keeps the entire standalone player (including overlay UI) at 16:9.
/// Unity's borderless fullscreen mode adds black bars as needed.
/// </summary>
internal static class PCDisplaySettings
{
#if UNITY_STANDALONE && !UNITY_EDITOR
    private const int AspectWidth = 16;
    private const int AspectHeight = 9;

    // Run once at application startup, not when returning to the gameplay scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyResolution()
    {
        int monitorWidth = Display.main.systemWidth;
        int monitorHeight = Display.main.systemHeight;

        if (monitorWidth < AspectWidth || monitorHeight < AspectHeight)
        {
            Resolution desktopResolution = Screen.currentResolution;
            monitorWidth = desktopResolution.width;
            monitorHeight = desktopResolution.height;
        }

        // Largest exact 16:9 resolution that fits inside the monitor.
        int scale = Mathf.Min(monitorWidth / AspectWidth, monitorHeight / AspectHeight);
        if (scale < 1)
            return;

        Screen.SetResolution(AspectWidth * scale, AspectHeight * scale,
            FullScreenMode.FullScreenWindow);
    }
#endif
}
