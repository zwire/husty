namespace Husty.RosBridge;

public struct RclInterfaces
{
    public struct Msg
    {
        public record struct Log(StdMsgs.Time time, byte level, string name, string msg, string file, string function, int line);
    }
}


// http://wiki.ros.org/std_msgs
public struct StdMsgs
{
    public record struct Empty();

    public record struct ColorRGBA(float r, float g, float b, float a);

    public record struct Time(long secs, long nsecs)
    {
        public static Time Now()
        {
            var milli = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return new(milli / 1000, milli * 1000000);
        }
    }

    public record struct Header(uint seq, Time stamp, string frame_id);
}

// http://wiki.ros.org/geometry_msgs
public struct GeometryMsgs
{

    public record struct Accel(Vector3 linear, Vector3 angular);

    public record struct AccelStamped(StdMsgs.Header header, Accel accel);

    public record struct AccelWithCovariance(Accel accel, double covariance);

    public record struct AccelWithCovarianceStamped(StdMsgs.Header header, AccelWithCovariance accel);

    public record struct Inertia(double m, Vector3 com, double ixx, double ixy, double ixz, double iyy, double iyz, double izz);

    public record struct InertiaStamped(StdMsgs.Header header, Inertia inertia);

    public record struct Point(double x, double y, double z);

    public record struct PointStamped(StdMsgs.Header header, Point point);

    public record struct Point32(float x, float y, float z);

    public record struct Polygon(Point32[] points);

    public record struct PolygonStamped(StdMsgs.Header header, Polygon polygon);

    public record struct Pose(Point position, Quaternion orientation);

    public record struct Pose2D(double x, double y, double theta);

    public record struct PoseArray(StdMsgs.Header header, Pose[] poses);

    public record struct PoseStamped(StdMsgs.Header header, Pose pose);

    public record struct PoseWithCovariance(Pose pose, double[] covariance);

    public record struct PoseWithCovarianceStamped(StdMsgs.Header header, PoseWithCovariance pose);

    public record struct Quaternion(double x, double y, double z, double w);

    public record struct QuaternionStamped(StdMsgs.Header header, Quaternion quaternion);

    public record struct Transform(Vector3 translation, Quaternion rotation);

    public record struct TransformStamped(StdMsgs.Header header, Transform transform);

    public record struct Twist(Vector3 linear, Vector3 angular);

    public record struct TwistStamped(StdMsgs.Header header, Twist twist);

    public record struct TwistWithCovariance(Twist twist, double[] covariance);

    public record struct TwistWithCovarianceStamped(StdMsgs.Header header, TwistWithCovariance twist);

    public record struct Vector3(double x, double y, double z);

    public record struct Vector3Stamped(StdMsgs.Header header, Vector3 vector);

    public record struct Wrench(Vector3 force, Vector3 torque);

    public record struct WrenchStamped(StdMsgs.Header header, Wrench wrench);

}