using UnityEngine;

/// <summary>
/// Class for usefull calculations.
/// </summary>
public static class Calc
{
    /// <summary>
    /// Greatest common divisor of a and b
    /// </summary>
    /// <param name="a">an integer</param>
    /// <param name="b">an integer</param>
    public static int GCD(int a, int b)
    {
        if (a == b || b == 0)
            return a;
        else
            return GCD(b, a % b);
    }

    /// <summary>
    /// Least common multiple of a and b
    /// </summary>
    /// <param name="a">an integer</param>
    /// <param name="b">an integer</param>
    public static int LCM(int a, int b)
    {
        return (a * b) / GCD(a, b);
    }

    /// <summary>
    /// Reduce n as long as the difference between m and n is greater
    /// or equal to d and n is greater than d
    /// </summary>
    /// <param name="m">an integer</param>
    /// <param name="n">an integer</param>
    /// <param name="l">an integer</param>
    /// <param name="d">an integer</param>
    public static int Reduce(int m, int n, int l, int d)
    {
        while (n > l && n - m <= d)
            n--;

        return n;
    }

    /// <summary>
    /// Divide n until n is a divisor of m and greater or equal to l
    /// </summary>
    /// <param name="m">an integer</param>
    /// <param name="n">an integer</param>
    /// <param name="l">an integer</param>
    public static int Divide(int m, int n, int l)
    {
        while (n > l && m % n > 0)
            n--;

        return n;
    }

    /// <summary>
    /// Check if a gameobject o2 is in front of gameobject o1
    /// </summary>
    /// <param name="o1">a GameObject</param>
    /// <param name="o2">a GameObject</param>
    public static bool InFront(GameObject o1, GameObject o2)
    {
        Vector3 v1 = o1.transform.TransformDirection(Vector3.forward);
        Vector3 v2 = o2.transform.position - o1.transform.position;
        float f = Vector3.Dot(v1, v2);

        Debug.Log(o1.name + " " + v1 + " <-> " + o2.name + " " + v2 + " => " + f + " < 0f");

        return f < 0;
    }

    /// <summary>
    /// Rotates given point around a pivot by a given angle.
    /// </summary>
    /// <param name="point">Vector3 to rotate</param>
    /// <param name="pivot">the center of rotation</param>
    /// <param name="angles">Vector3 of Euler angles</param>
    /// <returns>rotated point</returns>
    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    /// <summary>
    /// Calculates the duration of a path.
    /// </summary>
    /// <param name="points">path as a list</param>
    /// <param name="speed">walkspeed</param>
    /// <returns>duration of the path</returns>
    public static float CalcPathDuration(Vector3[] points, float speed)
    {
        float duration = 0f;
        Vector3 p0 = points[0];

        for (int i = 1; i < points.Length; i++)
        {
            Vector3 p1 = points[i];
            duration += Vector3.Distance(p1, p0) / speed;
            p0 = p1;
        }

        return duration;
    }

    /// <summary>
    /// Get the nearest point on the ground.
    /// </summary>
    /// <param name="origin">Vector3 to find position on ground</param>
    /// <returns>position on ground</returns>
    public static RaycastHit GetPointOnGround(Vector3 origin)
    {
        Vector3 offUp = Vector3.up * 0.1f;
        Vector3 normal = Vector3.up;

        if (Physics.Raycast(origin + offUp, Vector3.down, out RaycastHit hit))
        {
            Vector3 point = new Vector3(origin.x, hit.point.y, origin.z);
            origin = point;
            normal = hit.normal;
        }

        return new RaycastHit
        {
            point = origin,
            normal = normal
        };
    }

    /// <summary>
    /// Get the corresponding position on screen for a given point in 3D world
    /// to display a 2D Object, e. g. a text label.
    /// </summary>
    /// <param name="pos">position in 3D world</param>
    /// <param name="pivot">center of the 2D object on screen</param>
    /// <param name="offset">offset to keep from object on screen</param>
    /// <param name="size">size of the 2D element</param>
    /// <returns>the bounds for the 2D elements to display on screen</returns>
    public static float[] CalculatePositionOnScreen(Vector3 pos, Vector2 pivot,
        Vector3 offset, Vector2 size)
    {
        float maxY = Screen.height - 10f;
        float maxX = Screen.width - 10f;
        float rotZ = 0f;
        float rotY = 0f;
        pos = new Vector3(pos.x, pos.y, 0f) + offset;
        Vector3 pos2 = new Vector3(pos.x + size.x, pos.y + size.y, 0f);

        //Debug.Log("Position " + pos + " " + pos2 + " " + maxX + " " + maxY);

        if (pos2.x >= maxX)
        {
            pivot.x = 1f;
            rotY = 180f;
            offset.x *= -1f;
        }

        if (pos2.y >= maxY)
        {
            pivot.y = 1f;
            rotZ = 180f;
            rotY += 180f;
            offset.y *= -1f;
        }

        pos = new Vector3(pos.x, pos.y, 0f) + offset;
        return new float[]
        {
            pos.x,
            pos.y,
            pivot.x,
            pivot.y,
            rotY,
            rotZ
        };
    }
}
