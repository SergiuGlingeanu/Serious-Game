
public static class GameSettings
{
    public const byte kInitialOrtographicSize = 16;
    public static bool enableMobileMode = true;
    public static bool panBlocked = false;
    public static GameMode currentGameMode = GameMode.Regular;
    public static UnityEngine.DeviceType deviceType = UnityEngine.DeviceType.Unknown;
}

public enum GameMode { 
    Regular,
    BuyMode,
    ReservedForFutureMode
}
