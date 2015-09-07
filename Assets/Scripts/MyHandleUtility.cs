using System.Collections.Generic;
using UnityEngine;

public class MyHandleUtility
{
    public static Color xAxisColor = new Color(0.8588235f, 0.2431373f, 0.1137255f, 0.93f);
    public static Color yAxisColor = new Color(0.6039216f, 0.9529412f, 0.282353f, 0.93f);
    public static Color zAxisColor = new Color(0.227451f, 0.4784314f, 0.972549f, 0.93f);
    public static Color centerColor = new Color(0.8f, 0.8f, 0.8f, 0.93f);
    public static Color selectedColor = new Color(0.9647059f, 0.9490196f, 0.1960784f, 0.89f);

    //********//

    private static Mesh _coneMesh;
    private static Mesh _cubeMesh;
    private static Mesh _sphereMesh;
    private static Mesh _cylinderMesh;
    private static Mesh _quadMesh;

    public static Mesh CubeMesh
    {
        get
        {
            if (_cubeMesh == null) _cubeMesh = Resources.Load("Meshes/Cube") as Mesh;
            return _cubeMesh;
        }
    }

    public static Mesh ConeMesh
    {
        get
        {
            if (_coneMesh == null) _coneMesh = Resources.Load("Meshes/Cone") as Mesh;
            return _coneMesh;
        }
    }

    public static Mesh CylinderMesh
    {
        get
        {
            if (_cylinderMesh == null) _cylinderMesh = Resources.Load("Meshes/Cylinder") as Mesh;
            return _cylinderMesh;
        }
    }

    public static Mesh SphereMesh
    {
        get
        {
            if (_sphereMesh == null) _sphereMesh = Resources.Load("Meshes/Sphere") as Mesh;
            return _sphereMesh;
        }
    }

    //*******//

    private static Material _handleMaterial;
    private static Material _handleWireMaterial;

    public static Material handleMaterial
    {
        get
        {
            if (!(bool)((Object)MyHandleUtility._handleMaterial))
                MyHandleUtility._handleMaterial = Resources.Load("Handles/HandleMat") as Material;
            return MyHandleUtility._handleMaterial;
        }
    }

    private static Material handleWireMaterial
    {
        get
        {
            if (!(bool)((Object)MyHandleUtility._handleMaterial))
                MyHandleUtility._handleMaterial = Resources.Load("Handles/HandleMat") as Material;
            return MyHandleUtility._handleMaterial;
        }
    }

    internal static void ApplyHandleMaterial()
    {
        handleMaterial.SetPass(0);
    }

    internal static void ApplyHandleWireMaterial()
    {
        handleWireMaterial.SetPass(0);
    }

    //*******//


    public static void DrawConeCap(Vector3 position, Quaternion rotation, float size, Color color)
    {
        Shader.SetGlobalColor("_HandleColor", color);
        Shader.SetGlobalInt("_EnableShading", 1);
        ApplyHandleMaterial();
        Graphics.DrawMeshNow(ConeMesh, Matrix4x4.TRS(position, rotation, new Vector3(size, size, size)));
    }

    public static void DrawCubeCap(Vector3 position, Quaternion rotation, float size, Color color)
    {
        Shader.SetGlobalColor("_HandleColor", color);
        Shader.SetGlobalInt("_EnableShading", 1);
        ApplyHandleMaterial();
        Graphics.DrawMeshNow(CubeMesh, Matrix4x4.TRS(position, rotation, new Vector3(size, size, size)));
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, Color color)
    {
        Shader.SetGlobalColor("_HandleColor", color);
        Shader.SetGlobalInt("_EnableShading", 0);

        ApplyHandleWireMaterial();

        GL.PushMatrix();
        GL.Begin(1);
        GL.Vertex(p1);
        GL.Vertex(p2);
        GL.End();
        GL.PopMatrix();
    }

    public static void DrawPolyLine(Color color, params Vector3[] points)
    {
        Shader.SetGlobalColor("_HandleColor", color);
        Shader.SetGlobalInt("_EnableShading", 0);
        ApplyHandleWireMaterial();

        GL.PushMatrix();
        GL.Begin(1);
        for (int index = 1; index < points.Length; ++index)
        {
            GL.Vertex(points[index]);
            GL.Vertex(points[index - 1]);
        }
        GL.End();
        GL.PopMatrix();
    }

    internal static void SetDiscSectionPoints(Vector3[] dest, int count, Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
    {
        from.Normalize();
        Quaternion quaternion = Quaternion.AngleAxis(angle / (float)(count - 1), normal);
        Vector3 vector3 = from * radius;
        for (int index = 0; index < count; ++index)
        {
            dest[index] = center + vector3;
            vector3 = quaternion * vector3;
        }
    }

    public static void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Color color)
    {
        Vector3[] dest = new Vector3[60];
        SetDiscSectionPoints(dest, 60, center, normal, from, angle, radius);
        DrawPolyLine(color, dest);
    }

    public static void DrawWireDisc(Vector3 center, Vector3 normal, float radius, Color color)
    {
        Vector3 from = Vector3.Cross(normal, Vector3.up);
        if ((double)from.sqrMagnitude < 1.0 / 1000.0)
            from = Vector3.Cross(normal, Vector3.right);
        DrawWireArc(center, normal, from, 360f, radius, color);
    }

    //*******//

    public static float GetHandleSize(Vector3 position)
    {
        Camera current = Camera.current;
        if (!(bool)((Object)current))
            return 20f;
        Transform transform = current.transform;
        Vector3 position1 = transform.position;
        float z = Vector3.Dot(position - position1, transform.TransformDirection(new Vector3(0.0f, 0.0f, 1f)));
        return 80f / Mathf.Max((current.WorldToScreenPoint(position1 + transform.TransformDirection(new Vector3(0.0f, 0.0f, z))) - current.WorldToScreenPoint(position1 + transform.TransformDirection(new Vector3(1f, 0.0f, z)))).magnitude, 0.0001f);
    }

    public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector3 = lineEnd - lineStart;
        float magnitude = vector3.magnitude;
        Vector3 lhs = vector3;
        if ((double)magnitude > 9.99999997475243E-07)
            lhs /= magnitude;
        float num = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0.0f, magnitude);
        return lineStart + lhs * num;
    }

    public static Vector2 WorldToGUIPoint(Vector3 world)
    {
        var absolutePos = (Vector2)Camera.main.WorldToScreenPoint(world);
        absolutePos.y = (float)Screen.height - absolutePos.y;
        return (absolutePos);
        //return GUIClip.Clip(absolutePos);
    }

    public static void DebugD(Vector3 p1)
    {
        Debug.Log(MyHandleUtility.WorldToGUIPoint(p1));
        Debug.Log((Vector3)Event.current.mousePosition);
        Debug.Log("***");
    }

    public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return Vector3.Magnitude(MyHandleUtility.ProjectPointLine(point, lineStart, lineEnd) - point);
    }

    public static float DistanceToLine(Vector3 p1, Vector3 p2)
    {
        p1 = (Vector3)MyHandleUtility.WorldToGUIPoint(p1);
        p2 = (Vector3)MyHandleUtility.WorldToGUIPoint(p2);
        float num = MyHandleUtility.DistancePointLine((Vector3)Event.current.mousePosition, p1, p2);
        if ((double)num < 0.0)
            num = 0.0f;
        return num;
    }

    public static float DistanceToCircle(Vector3 position, float radius)
    {
        Vector2 vector2_1 = MyHandleUtility.WorldToGUIPoint(position);
        Camera current = Camera.current;
        Vector2 zero = Vector2.zero;
        if ((bool)((Object)current))
        {
            Vector2 vector2_2 = MyHandleUtility.WorldToGUIPoint(position + current.transform.right * radius);
            radius = (vector2_1 - vector2_2).magnitude;
        }
        float magnitude = (vector2_1 - Event.current.mousePosition).magnitude;
        if ((double)magnitude < (double)radius)
            return 0.0f;
        return magnitude - radius;
    }

    public static float DistanceToPolyLine(params Vector3[] points)
    {
        float num1 = DistanceToLine(points[0], points[1]);
        for (int index = 2; index < points.Length; ++index)
        {
            float num2 = DistanceToLine(points[index - 1], points[index]);
            if ((double)num2 < (double)num1)
                num1 = num2;
        }
        return num1;
    }

    public static float DistanceToArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
    {
        Vector3[] dest = new Vector3[60];
        MyHandleUtility.SetDiscSectionPoints(dest, 60, center, normal, from, angle, radius);
        return DistanceToPolyLine(dest);
    }

    public static float DistanceToDisc(Vector3 center, Vector3 normal, float radius)
    {
        Vector3 from = Vector3.Cross(normal, Vector3.up);
        if ((double)from.sqrMagnitude < 1.0 / 1000.0)
            from = Vector3.Cross(normal, Vector3.right);
        return DistanceToArc(center, normal, from, 360f, radius);
    }

    public static void DrawWireMesh(Mesh mesh, Transform transform, Color color)
    {
        Shader.SetGlobalColor("_HandleColor", color);
        Shader.SetGlobalInt("_EnableShading", 0);

        ApplyHandleWireMaterial();

        GL.wireframe = true;
        Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
        GL.wireframe = false;
    }
}