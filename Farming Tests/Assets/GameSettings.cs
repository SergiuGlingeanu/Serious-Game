
public static class GameSettings
{
    //Constants
    public const byte kInitialOrthographicSize = 16;
    public const float kDragPointPositionOffset = 1.5f;
    public const float kDefaultOccupiedSpaceHeight = 0.5f;
    public const byte kMoneyGenerationDivider = 1;
    public const byte kStartMoney = 100;
    public readonly static UnityEngine.Vector3 kDragPointDirection = new UnityEngine.Vector3(1, 0, 1);
    public readonly static UnityEngine.Vector3 kPredeterminedFenceControlPosition = new UnityEngine.Vector3(0.8f, 0.12f, -0.48f);

    //Settings
    public static bool enableMobileMode = true;
    public static bool panBlocked = false;
    public static GameMode currentGameMode = GameMode.Regular;
    public static UnityEngine.DeviceType deviceType = UnityEngine.DeviceType.Unknown;
    public static string currency = "\u20AC";
}

public enum GameMode : byte { 
    Regular,
    BuyMode,
    ReservedForFutureMode
}
