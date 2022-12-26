namespace Husty.RosBridge;

// https://github.com/ros2/rcl_interfaces
public struct builtin_interfaces
{
    public record struct Time(int sec, uint nanosec)
    {
        public Time() : this(0, 0)
        {
            this = Now;
        }
        public static Time Now 
        {
            get
            {
                var milli = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return new((int)(milli / 1000), (uint)(milli * 1000000));
            }
        }
    }
    public record struct Duration(int sec, uint nanosec);
}

public struct rcl_interfaces
{
    public record struct FloatingPointRange(double from_value, double to_value, double step);
    public record struct IntegerRange(long from_value, long to_value, long step);
    public record struct ListParametersResult(string[] names, string[] prefixes);
    public record struct Log(builtin_interfaces.Time time, byte level, string name, string msg, string file, string function, int line)
    {
        public const byte DEBUG = 10;
        public const byte INFO = 20;
        public const byte WARN = 30;
        public const byte ERROR = 40;
        public const byte FATAL = 50;
    }
    public record struct Parameter(string name, ParameterValue value);
    public record struct ParameterDescriptor(
        string name, byte type, string description, string additional_constraints,
        bool read_only, bool dynamic_typing, FloatingPointRange[] floating_point_range, IntegerRange[] integer_range
    );
    public record struct ParameterEvent(
        builtin_interfaces.Time stamp,  string node, 
        Parameter[] new_parameters, Parameter[] changed_parameters, Parameter[] deleted_parameters
    );
    public record struct ParameterEventDescriptors(
        ParameterDescriptor[] new_parameters,
        ParameterDescriptor[] changed_parameters,
        ParameterDescriptor[] deleted_parameters
    );
    public struct ParameterType
    {
        public const byte PARAMETER_NOT_SET = 0;
        public const byte PARAMETER_BOOL = 1;
        public const byte PARAMETER_INTEGER = 2;
        public const byte PARAMETER_DOUBLE = 3;
        public const byte PARAMETER_STRING = 4;
        public const byte PARAMETER_BYTE_ARRAY = 5;
        public const byte PARAMETER_BOOL_ARRAY = 6;
        public const byte PARAMETER_INTEGER_ARRAY = 7;
        public const byte PARAMETER_DOUBLE_ARRAY = 8;
        public const byte PARAMETER_STRING_ARRAY = 9;
    }
    public record struct ParameterValue(
        byte type, bool bool_value, long integer_value, double double_value, string string_value, byte[] byte_array_value, 
        bool[] bool_array_value, long[] integer_array_value, double[] double_array_value, string[] string_array_value
    );
    public record struct SetParametersResult(bool successful, string reason);
}

public struct rosgraph_msgs
{
    public record struct Clock(builtin_interfaces.Time clock);
}

public struct statistics_msgs
{
    public record struct MetricsMessage(
        string measurement_source_name, string metrics_source, string unit,
        builtin_interfaces.Time window_start, builtin_interfaces.Time window_stop, StatisticDataPoint[] statistics
    );
    public record struct StatisticDataPoint(byte data_type, double data);
    public struct StatisticDataType
    {
        public const byte STATISTICS_DATA_TYPE_UNINITIALIZED = 0;
        public const byte STATISTICS_DATA_TYPE_AVERAGE = 1;
        public const byte STATISTICS_DATA_TYPE_MINIMUM = 2;
        public const byte STATISTICS_DATA_TYPE_MAXIMUM = 3;
        public const byte STATISTICS_DATA_TYPE_STDDEV = 4;
        public const byte STATISTICS_DATA_TYPE_SAMPLE_COUNT = 5;
    }
}

// https://github.com/ros2/common_interfaces
public struct std_msgs
{
    public record struct Bool(bool data);
    public record struct Byte(byte data);
    public record struct ByteMultiArray(MultiArrayLayout layout, byte[] data);
    public record struct Char(sbyte data);
    public record struct ColorRGBA(float r, float g, float b, float a);
    public record struct Duration(builtin_interfaces.Duration data);
    public record struct Empty();
    public record struct Float32(float data);
    public record struct Float32MultiArray(MultiArrayLayout[] layout, float[] data);
    public record struct Float64(double data);
    public record struct Float64MultiArray(MultiArrayLayout[] layout, double[] data);
    public record struct Header(builtin_interfaces.Time stamp, string frame_id = "")
    {
        public Header() : this(new(), "") { }
        public static Header Default => new();
    }
    public record struct Int16(short data);
    public record struct Int16MultiArray(MultiArrayLayout[] layout, short[] data);
    public record struct Int32(int data);
    public record struct Int32MultiArray(MultiArrayLayout[] layout, int[] data);
    public record struct Int64(long data);
    public record struct Int64MultiArray(MultiArrayLayout[] layout, long[] data);
    public record struct Int8(sbyte data);
    public record struct Int8MultiArray(MultiArrayLayout[] layout, sbyte[] data);
    public record struct MultiArrayDimension(string label, int size, int stride);
    public record struct MultiArrayLayout(MultiArrayDimension[] dim, int data_offset);
    public record struct String(string data);
    public record struct Time(builtin_interfaces.Time data);
    public record struct UInt16(ushort data);
    public record struct UInt16MultiArray(MultiArrayLayout[] layout, ushort[] data);
    public record struct UInt32(uint data);
    public record struct UInt32MultiArray(MultiArrayLayout[] layout, uint[] data);
    public record struct UInt64(ulong data);
    public record struct UInt64MultiArray(MultiArrayLayout[] layout, ulong[] data);
    public record struct UInt8(byte data);
    public record struct UInt8MultiArray(MultiArrayLayout[] layout, byte[] data);
}

public struct geometry_msgs
{
    public record struct Accel(Vector3 linear, Vector3 angular);
    public record struct AccelStamped(std_msgs.Header header, Accel accel);
    public record struct AccelWithCovariance(Accel accel, double covariance);
    public record struct AccelWithCovarianceStamped(std_msgs.Header header, AccelWithCovariance accel);
    public record struct Inertia(double m, Vector3 com, double ixx, double ixy, double ixz, double iyy, double iyz, double izz);
    public record struct InertiaStamped(std_msgs.Header header, Inertia inertia);
    public record struct Point(double x, double y, double z);
    public record struct PointStamped(std_msgs.Header header, Point point);
    public record struct Point32(float x, float y, float z);
    public record struct Polygon(Point32[] points);
    public record struct PolygonStamped(std_msgs.Header header, Polygon polygon);
    public record struct Pose(Point position, Quaternion orientation);
    public record struct Pose2D(double x, double y, double theta);
    public record struct PoseArray(std_msgs.Header header, Pose[] poses);
    public record struct PoseStamped(std_msgs.Header header, Pose pose);
    public record struct PoseWithCovariance(Pose pose, double[] covariance);
    public record struct PoseWithCovarianceStamped(std_msgs.Header header, PoseWithCovariance pose);
    public record struct Quaternion(double x, double y, double z, double w);
    public record struct QuaternionStamped(std_msgs.Header header, Quaternion quaternion);
    public record struct Transform(Vector3 translation, Quaternion rotation);
    public record struct TransformStamped(std_msgs.Header header, Transform transform);
    public record struct Twist(Vector3 linear, Vector3 angular);
    public record struct TwistStamped(std_msgs.Header header, Twist twist);
    public record struct TwistWithCovariance(Twist twist, double[] covariance);
    public record struct TwistWithCovarianceStamped(std_msgs.Header header, TwistWithCovariance twist);
    public record struct Vector3(double x, double y, double z);
    public record struct Vector3Stamped(std_msgs.Header header, Vector3 vector);
    public record struct Wrench(Vector3 force, Vector3 torque);
    public record struct WrenchStamped(std_msgs.Header header, Wrench wrench);
}

public struct actionlib_msgs
{
    public record struct GoalID(builtin_interfaces.Time stamp, string id);
    public record struct GoalStatus(GoalID goal_id, sbyte status, string text)
    {
        public const byte PENDING = 0;
        public const byte ACTIVE = 1;
        public const byte PREEMPTED = 2;
        public const byte SUCCEEDED = 3;
        public const byte ABORTED = 4;
        public const byte REJECTED = 5;
        public const byte PREEMPTING = 6;
        public const byte RECALLING = 7;
        public const byte RECALLED = 8;
        public const byte LOST = 9;
    }
    public record struct GoalStatusArray(GoalStatus[] status_list);
}

// https://docs.ros2.org/foxy/api/diagnostic_msgs/index-msg.html
public struct diagnostic_msgs
{
    public record struct DiagnosticArray(std_msgs.Header header, DiagnosticStatus[] status);
    public record struct DiagnosticStatus(byte level, string name, string message, string hardware_id, KeyValue[] values)
    {
        public const byte OK = 0;
        public const byte WARN = 1;
        public const byte ERROR = 2;
        public const byte STALE = 3;
    }
    public record struct KeyValue(string key, string value);
}

// https://docs.ros2.org/foxy/api/nav_msgs/index-msg.html
public struct nav_msgs
{
    public record struct GridCells(std_msgs.Header header, float cell_width, float cell_height, geometry_msgs.Point[] cells);
    public record struct MapMetaData(builtin_interfaces.Time map_load_time, float resolution, uint width, uint height, geometry_msgs.Pose origin);
    public record struct OccupancyGrid(std_msgs.Header header, MapMetaData info, sbyte[] data);
    public record struct Odometry(std_msgs.Header header, string child_frame_id, geometry_msgs.PoseWithCovariance pose, geometry_msgs.TwistWithCovariance twist);
    public record struct Path(std_msgs.Header header, geometry_msgs.PoseStamped[] poses);
}

// https://docs.ros2.org/foxy/api/sensor_msgs/index-msg.html
public struct sensor_msgs
{
    public record struct BatteryState(
        std_msgs.Header header, float voltage, float temperature, float current,
        float charge, float capacity, float design_capacity, float percentage, 
        byte power_supply_status, byte power_supply_health, byte power_supply_technology, bool present,
        float[] cell_voltage, float[] cell_temperature, string location, string serial_number
    )
    {
        public const byte POWER_SUPPLY_STATUS_UNKNOWN = 0;
        public const byte POWER_SUPPLY_STATUS_CHARGING = 1;
        public const byte POWER_SUPPLY_STATUS_DISCHARGING = 2;
        public const byte POWER_SUPPLY_STATUS_NOT_CHARGING = 3;
        public const byte POWER_SUPPLY_STATUS_FULL = 4;
        public const byte POWER_SUPPLY_HEALTH_UNKNOWN = 0;
        public const byte POWER_SUPPLY_HEALTH_GOOD = 1;
        public const byte POWER_SUPPLY_HEALTH_OVERHEAT = 2;
        public const byte POWER_SUPPLY_HEALTH_DEAD = 3;
        public const byte POWER_SUPPLY_HEALTH_OVERVOLTAGE = 4;
        public const byte POWER_SUPPLY_HEALTH_UNSPEC_FAILURE = 5;
        public const byte POWER_SUPPLY_HEALTH_COLD = 6;
        public const byte POWER_SUPPLY_HEALTH_WATCHDOG_TIMER_EXPIRE = 7;
        public const byte POWER_SUPPLY_HEALTH_SAFETY_TIMER_EXPIRE = 8;
        public const byte POWER_SUPPLY_TECHNOLOGY_UNKNOWN = 0;
        public const byte POWER_SUPPLY_TECHNOLOGY_NIMH = 1;
        public const byte POWER_SUPPLY_TECHNOLOGY_LION = 2;
        public const byte POWER_SUPPLY_TECHNOLOGY_LIPO = 3;
        public const byte POWER_SUPPLY_TECHNOLOGY_LIFE = 4;
        public const byte POWER_SUPPLY_TECHNOLOGY_NICD = 5;
        public const byte POWER_SUPPLY_TECHNOLOGY_LIMN = 6;
    }
    public record struct CameraInfo(
        std_msgs.Header header, uint height, uint width, string distortion_model,
        double[] D, double[] K, double[] R, double[] P, uint binning_x, uint binning_y, RegionOfInterest roi
    );
    public record struct ChannelFloat32(string name, float[] values);
    public record struct CompressedImage(std_msgs.Header header, string format, byte[] data);
    public record struct FluidPressure(std_msgs.Header header, double fluid_pressure, double variance);
    public record struct Illuminance(std_msgs.Header header, double illuminance, double variance);
    public record struct Image(
        std_msgs.Header header, uint height, uint width, string encoding,
        byte is_bigendian, uint step, byte[] data
    );
    public record struct Imu(
        std_msgs.Header header, geometry_msgs.Quaternion orientation, double[] orientation_covariance,
        geometry_msgs.Vector3 angular_velocity, double[] angular_velocity_covariance,
        geometry_msgs.Vector3 linear_acceleration, double[] linear_acceleration_covariance
    );
    public record struct JointState(std_msgs.Header header, string[] name, double[] position, double[] velocity, double[] effort);
    public record struct Joy(std_msgs.Header header, float[] axes, int[] buttons);
    public record struct JoyFeedback(byte type, byte id, float intensity)
    {
        public const byte TYPE_LED = 0;
        public const byte TYPE_RUMBLE = 1;
        public const byte TYPE_BUZZER = 2;
    }
    public record struct JoyFeedbackArray(JoyFeedback[] array);
    public record struct LaserEcho(float[] echoes);
    public record struct LaserScan(
        std_msgs.Header header, float angle_min, float angle_max, float angle_increment, float time_increment,
        float scan_time, float range_min, float range_max, float[] ranges, float[] intensities
    );
    public record struct MagneticField(std_msgs.Header header, geometry_msgs.Vector3 magnetic_field, double[] magnetic_field_covariance);
    public record struct MultiDOFJointState(
        std_msgs.Header header, string[] joint_names, 
        geometry_msgs.Transform[] transforms, geometry_msgs.Twist[] twist, geometry_msgs.Wrench[] wrench
    );
    public record struct MultiEchoLaserScan(
        std_msgs.Header header, float angle_min, float angle_max, float angle_increment, float time_increment, 
        float scan_time, float range_min, float range_max, LaserEcho[] ranges, LaserEcho[] intensities
    );
    public record struct NavSatFix(
        std_msgs.Header header, NavSatStatus status, double latitude, double longitude,
        double altitude, double[] position_covariance, byte position_covariance_type
    )
    {
        public const byte COVARIANCE_TYPE_UNKNOWN = 0;
        public const byte COVARIANCE_TYPE_APPROXIMATED = 1;
        public const byte COVARIANCE_TYPE_DIAGONAL_KNOWN = 2;
        public const byte COVARIANCE_TYPE_KNOWN = 3;
    }
    public record struct NavSatStatus(sbyte status, ushort service)
    {
        public const sbyte STATUS_NO_FIX = -1;
        public const sbyte STATUS_FIX = 0;
        public const sbyte STATUS_SBAS_FIX = 1;
        public const sbyte STATUS_GBAS_FIX = 2;
        public const ushort SERVICE_GPS = 1;
        public const ushort SERVICE_GLONASS = 2;
        public const ushort SERVICE_COMPASS = 4;
        public const ushort SERVICE_GALILEO = 8;
    }
    public record struct PointCloud(std_msgs.Header header, geometry_msgs.Point32[] points, ChannelFloat32[] channels);
    public record struct PointCloud2(
        std_msgs.Header header, uint height, uint width, PointField[] fields,
        bool is_bigendian, uint point_step, uint row_step, byte[] data, bool is_dense
    );
    public record struct PointField(string name, uint offset, byte datatype, uint count)
    {
        public const byte INT8 = 1;
        public const byte UINT8 = 2;
        public const byte INT16 = 3;
        public const byte UINT16 = 4;
        public const byte INT32 = 5;
        public const byte UINT32 = 6;
        public const byte FLOAT32 = 7;
        public const byte FLOAT64 = 8;
    }
    public record struct Range(std_msgs.Header header, byte radiation_type, float field_of_view, float min_range, float max_range, float range)
    {
        public const byte ULTRASOUND = 0;
        public const byte INFRARED = 1;
    }
    public record struct RegionOfInterest(uint x_offset, uint y_offset, uint height, uint width, bool do_rectify);
    public record struct RelativeHumidity(std_msgs.Header header, double relative_humidity, double variance);
    public record struct Temperature(std_msgs.Header header, double temperature, double variance);
    public record struct TimeReference(std_msgs.Header header, builtin_interfaces.Time time_ref, string source);
}

public struct shape_msgs
{
    public record struct Mesh(MeshTriangle[] triangles, geometry_msgs.Point[] vertices);
    public record struct MeshTriangle(uint[] vertex_indices);
    public record struct Plane(double[] coef);
    public record struct SolidPrimitive(byte type, double[] dimensions, geometry_msgs.Polygon polygon)
    {
        public const byte BOX = 1;
        public const byte SPHERE = 2;
        public const byte CYLINDER = 3;
        public const byte CONE = 4;
        public const byte PRISM = 5;
        public const byte BOX_X = 0;
        public const byte BOX_Y = 1;
        public const byte BOX_Z = 2;
        public const byte SPHERE_RADIUS = 0;
        public const byte CYLINDER_HEIGHT = 0;
        public const byte CYLINDER_RADIUS = 1;
        public const byte CONE_HEIGHT = 0;
        public const byte CONE_RADIUS = 1;
        public const byte PRISM_HEIGHT = 0;
    }
}

public struct stereo_msgs
{
    public record struct DisparityImage(
        std_msgs.Header header, sensor_msgs.Image image, float f, float t, 
        sensor_msgs.RegionOfInterest valid_window,
        float min_disparity, float max_disparity, float delta_d
    );
}

public struct trajectory_msgs
{
    public record struct JointTrajectory(std_msgs.Header header, string[] joint_names, JointTrajectoryPoint[] points);
    public record struct JointTrajectoryPoint(double[] positions, double[] velocities, double[] accelerations, double[] effort, builtin_interfaces.Duration time_from_start);
    public record struct MultiDOFJointTrajectory(std_msgs.Header header, string[] joint_names, MultiDOFJointTrajectoryPoint[] points);
    public record struct MultiDOFJointTrajectoryPoint(
        geometry_msgs.Transform[] transforms, geometry_msgs.Twist[] velocities,
        geometry_msgs.Twist[] accelerations, builtin_interfaces.Duration time_from_start
    );
}

public struct visualization_msgs
{
    public record struct ImageMarker(
        std_msgs.Header header, string ns, int id, int type, int action, geometry_msgs.Point position, float scale,
        std_msgs.ColorRGBA outline_color, byte filled, std_msgs.ColorRGBA fill_color, builtin_interfaces.Duration lifetime,
        geometry_msgs.Point[] points, std_msgs.ColorRGBA[] outline_colors
    )
    {
        public const int CIRCLE = 0;
        public const int LINE_STRIP = 1;
        public const int LINE_LIST = 2;
        public const int POLYGON = 3;
        public const int POINTS = 4;
        public const int ADD = 0;
        public const int REMOVE = 1;
    }
    public record struct InteractiveMarker(
        std_msgs.Header header, geometry_msgs.Pose pose, string name, string description,
        float scale, MenuEntry[] menu_entries, InteractiveMarkerControl[] controls
    );
    public record struct InteractiveMarkerControl(
        string name, geometry_msgs.Quaternion orientation, byte orientation_mode, byte interaction_mode, bool always_visible, 
        Marker[] markers, bool independent_marker_orientation, string description
    )
    {
        public const byte INHERIT = 0;
        public const byte FIXED = 1;
        public const byte VIEW_FACING = 2;
        public const byte NONE = 0;
        public const byte MENU = 1;
        public const byte BUTTON = 2;
        public const byte MOVE_AXIS = 3;
        public const byte MOVE_PLANE = 4;
        public const byte ROTATE_AXIS = 5;
        public const byte MOVE_ROTATE = 6;
        public const byte MOVE_3D = 7;
        public const byte ROTATE_3D = 8;
        public const byte MOVE_ROTATE_3D = 9;
    }
    public record struct InteractiveMarkerFeedback(
        std_msgs.Header header, string client_id, string marker_name, string control_name, byte event_type, 
        geometry_msgs.Pose pose, uint menu_entry_id, geometry_msgs.Point mouse_point, bool mouse_point_valid
    )
    {
        public const byte KEEP_ALIVE = 0;
        public const byte POSE_UPDATE = 1;
        public const byte MENU_SELECT = 2;
        public const byte BUTTON_CLICK = 3;
        public const byte MOUSE_DOWN = 4;
        public const byte MOUSE_UP = 5;
    }
    public record struct InteractiveMarkerInit(string server_id, ulong seq_num, InteractiveMarker[] markers);
    public record struct InteractiveMarkerPose(std_msgs.Header header, geometry_msgs.Pose pose, string name);
    public record struct InteractiveMarkerUpdate(
        string server_id, ulong seq_num, byte type,
        InteractiveMarker[] markers, InteractiveMarkerPose[] poses, string[] erases
    )
    {
        public const byte KEEP_ALIVE = 0;
        public const byte UPDATE = 1;
    }
    public record struct Marker(
        std_msgs.Header header, string ns, int id, int type, int action,
        geometry_msgs.Pose pose, geometry_msgs.Vector3 scale, std_msgs.ColorRGBA color,
        builtin_interfaces.Duration lifetime, bool frame_locked, geometry_msgs.Point[] points,
        std_msgs.ColorRGBA[] colors, string texture_resource, sensor_msgs.CompressedImage texture,
        UVCoordinate[] uv_coordinates, string text, string mesh_resource, MeshFile mesh_file, bool mesh_use_embedded_materials
    )
    {
        public const int ARROW = 0;
        public const int CUBE = 1;
        public const int SPHERE = 2;
        public const int CYLINDER = 3;
        public const int LINE_STRIP = 4;
        public const int LINE_LIST = 5;
        public const int CUBE_LIST = 6;
        public const int SPHERE_LIST = 7;
        public const int POINTS = 8;
        public const int TEXT_VIEW_FACING = 9;
        public const int MESH_RESOURCE = 10;
        public const int TRIANGLE_LIST = 11;
        public const int ADD = 0;
        public const int MODIFY = 0;
        public const int DELETE = 2;
        public const int DELETEALL = 3;
    }
    public record struct MarkerArray(Marker[] markers);
    public record struct MenuEntry(uint id, uint parent_id, string title, string command, byte command_type)
    {
        public const byte FEEDBACK = 0;
        public const byte ROSRUN = 1;
        public const byte ROSLAUNCH = 2;
    }
    public record struct MeshFile(string filename, byte[] data);
    public record struct UVCoordinate(float u, float v);
}