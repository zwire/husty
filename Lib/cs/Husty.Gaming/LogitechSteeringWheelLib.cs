using System.Runtime.InteropServices;
using System.Text;

namespace Husty.Gaming;

internal static class LogitechSteeringWheelLib
{

  //STEERING WHEEL SDK
  public const int LOGI_MAX_CONTROLLERS = 4;

  //Force types

  public const int LOGI_FORCE_NONE = -1;
  public const int LOGI_FORCE_SPRING = 0;
  public const int LOGI_FORCE_CONSTANT = 1;
  public const int LOGI_FORCE_DAMPER = 2;
  public const int LOGI_FORCE_SIDE_COLLISION = 3;
  public const int LOGI_FORCE_FRONTAL_COLLISION = 4;
  public const int LOGI_FORCE_DIRT_ROAD = 5;
  public const int LOGI_FORCE_BUMPY_ROAD = 6;
  public const int LOGI_FORCE_SLIPPERY_ROAD = 7;
  public const int LOGI_FORCE_SURFACE_EFFECT = 8;
  public const int LOGI_NUMBER_FORCE_EFFECTS = 9;
  public const int LOGI_FORCE_SOFTSTOP = 10;
  public const int LOGI_FORCE_CAR_AIRBORNE = 11;


  //Periodic types  for surface effect

  public const int LOGI_PERIODICTYPE_NONE = -1;
  public const int LOGI_PERIODICTYPE_SINE = 0;
  public const int LOGI_PERIODICTYPE_SQUARE = 1;
  public const int LOGI_PERIODICTYPE_TRIANGLE = 2;


  //Devices types

  public const int LOGI_DEVICE_TYPE_NONE = -1;
  public const int LOGI_DEVICE_TYPE_WHEEL = 0;
  public const int LOGI_DEVICE_TYPE_JOYSTICK = 1;
  public const int LOGI_DEVICE_TYPE_GAMEPAD = 2;
  public const int LOGI_DEVICE_TYPE_OTHER = 3;
  public const int LOGI_NUMBER_DEVICE_TYPES = 4;


  //Manufacturer types

  public const int LOGI_MANUFACTURER_NONE = -1;
  public const int LOGI_MANUFACTURER_LOGITECH = 0;
  public const int LOGI_MANUFACTURER_MICROSOFT = 1;
  public const int LOGI_MANUFACTURER_OTHER = 2;


  //Model types

  public const int LOGI_MODEL_G27 = 0;
  public const int LOGI_MODEL_DRIVING_FORCE_GT = 1;
  public const int LOGI_MODEL_G25 = 2;
  public const int LOGI_MODEL_MOMO_RACING = 3;
  public const int LOGI_MODEL_MOMO_FORCE = 4;
  public const int LOGI_MODEL_DRIVING_FORCE_PRO = 5;
  public const int LOGI_MODEL_DRIVING_FORCE = 6;
  public const int LOGI_MODEL_NASCAR_RACING_WHEEL = 7;
  public const int LOGI_MODEL_FORMULA_FORCE = 8;
  public const int LOGI_MODEL_FORMULA_FORCE_GP = 9;
  public const int LOGI_MODEL_FORCE_3D_PRO = 10;
  public const int LOGI_MODEL_EXTREME_3D_PRO = 11;
  public const int LOGI_MODEL_FREEDOM_24 = 12;
  public const int LOGI_MODEL_ATTACK_3 = 13;
  public const int LOGI_MODEL_FORCE_3D = 14;
  public const int LOGI_MODEL_STRIKE_FORCE_3D = 15;
  public const int LOGI_MODEL_G940_JOYSTICK = 16;
  public const int LOGI_MODEL_G940_THROTTLE = 17;
  public const int LOGI_MODEL_G940_PEDALS = 18;
  public const int LOGI_MODEL_RUMBLEPAD = 19;
  public const int LOGI_MODEL_RUMBLEPAD_2 = 20;
  public const int LOGI_MODEL_CORDLESS_RUMBLEPAD_2 = 21;
  public const int LOGI_MODEL_CORDLESS_GAMEPAD = 22;
  public const int LOGI_MODEL_DUAL_ACTION_GAMEPAD = 23;
  public const int LOGI_MODEL_PRECISION_GAMEPAD_2 = 24;
  public const int LOGI_MODEL_CHILLSTREAM = 25;
  public const int LOGI_MODEL_G29 = 26;
  public const int LOGI_MODEL_G920 = 27;
  public const int LOGI_NUMBER_MODELS = 28;

  [StructLayout(LayoutKind.Sequential, Pack = 2)]
  public struct LogiControllerPropertiesData
  {
    public bool forceEnable;
    public int overallGain;
    public int springGain;
    public int damperGain;
    public bool defaultSpringEnabled;
    public int defaultSpringGain;
    public bool combinePedals;
    public int wheelRange;
    public bool gameSettingsEnabled;
    public bool allowGameSettings;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 2)]
  public struct DIJOYSTATE2ENGINES
  {
    public int lX;              /* x-axis position              */
    public int lY;              /* y-axis position              */
    public int lZ;              /* z-axis position              */
    public int lRx;             /* x-axis rotation              */
    public int lRy;             /* y-axis rotation              */
    public int lRz;             /* z-axis rotation              */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] rglSlider;     /* extra axes positions         */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] rgdwPOV;      /* POV directions               */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public byte[] rgbButtons;   /* 128 buttons                  */
    public int lVX;             /* x-axis velocity              */
    public int lVY;             /* y-axis velocity              */
    public int lVZ;             /* z-axis velocity              */
    public int lVRx;            /* x-axis angular velocity      */
    public int lVRy;            /* y-axis angular velocity      */
    public int lVRz;            /* z-axis angular velocity      */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] rglVSlider;    /* extra axes velocities        */
    public int lAX;             /* x-axis acceleration          */
    public int lAY;             /* y-axis acceleration          */
    public int lAZ;             /* z-axis acceleration          */
    public int lARx;            /* x-axis angular acceleration  */
    public int lARy;            /* y-axis angular acceleration  */
    public int lARz;            /* z-axis angular acceleration  */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] rglASlider;    /* extra axes accelerations     */
    public int lFX;             /* x-axis force                 */
    public int lFY;             /* y-axis force                 */
    public int lFZ;             /* z-axis force                 */
    public int lFRx;            /* x-axis torque                */
    public int lFRy;            /* y-axis torque                */
    public int lFRz;            /* z-axis torque                */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] rglFSlider;    /* extra axes forces            */
  };

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool SteeringInitialize(bool ignoreXInputControllers);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool Update();

  [DllImport("LogitechSteeringWheelLib")]
  public static extern nint GetStateENGINES(int index);

  public static DIJOYSTATE2ENGINES? GetState(int index)
  {
    var ptr = GetStateENGINES(index);
    if (Marshal.PtrToStructure(ptr, typeof(DIJOYSTATE2ENGINES)) is DIJOYSTATE2ENGINES o) return o;
    return null;
  }

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool GetDevicePath(int index, StringBuilder str, int size);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool GetFriendlyProductName(int index, StringBuilder str, int size);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool IsConnected(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool IsDeviceConnected(int index, int deviceType);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool IsManufacturerConnected(int index, int manufacturerName);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool IsModelConnected(int index, int modelName);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool ButtonTriggered(int index, int buttonNbr);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool ButtonReleased(int index, int buttonNbr);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool ButtonIsPressed(int index, int buttonNbr);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool GenerateNonLinearValues(int index, int nonLinCoeff);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern int GetNonLinearValue(int index, int inputValue);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool HasForceFeedback(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool IsPlaying(int index, int forceType);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlaySpringForce(int index, int offsetPercentage, int saturationPercentage, int coefficientPercentage);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool StopSpringForce(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlayConstantForce(int index, int magnitudePercentage);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool StopConstantForce(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlayDamperForce(int index, int coefficientPercentage);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool StopDamperForce(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlaySideCollisionForce(int index, int magnitudePercentage);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlayFrontalCollisionForce(int index, int magnitudePercentage);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlayDirtRoadEffect(int index, int magnitudePercentage);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool StopDirtRoadEffect(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlayBumpyRoadEffect(int index, int magnitudePercentage);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool StopBumpyRoadEffect(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlaySlipperyRoadEffect(int index, int magnitudePercentage);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool StopSlipperyRoadEffect(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlaySurfaceEffect(int index, int type, int magnitudePercentage, int period);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool StopSurfaceEffect(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlayCarAirborne(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool StopCarAirborne(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlaySoftstopForce(int index, int usableRangePercentage);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool StopSoftstopForce(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool SetPreferredControllerProperties(LogiControllerPropertiesData properties);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool GetCurrentControllerProperties(int index, ref LogiControllerPropertiesData properties);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern int GetShifterMode(int index);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool SetOperatingRange(int index, int range);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool GetOperatingRange(int index, ref int range);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern bool PlayLeds(int index, float currentRPM, float rpmFirstLedTurnsOn, float rpmRedLine);

  [DllImport("LogitechSteeringWheelLib")]
  public static extern void SteeringShutdown();

}
