using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UILineRenderer : Graphic
{
    public float LineThikness = 2;
    public bool UseMargins;
    public Vector2 Margin;
    public Vector2[] Points;

    protected override void OnPopulateMesh(Mesh m)
    {
        if (Points == null || Points.Length < 2)
            Points = new[] { new Vector2(0, 0), new Vector2(1, 1) };

        //var size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

        var sizeX = rectTransform.rect.width;
        var sizeY = rectTransform.rect.height;
        var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
        var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

        if (UseMargins)
        {
            sizeX -= Margin.x;
            sizeY -= Margin.y;
            offsetX += Margin.x / 2f;
            offsetY += Margin.y / 2f;
        }


        var vh = new VertexHelper();
        //{
        //    vh.AddVert(vertices[0], color, new Vector2(0f, 0f));
        //    vh.AddVert(vertices[1], color, new Vector2(0f, 1f));
        //    vh.AddVert(vertices[2], color, new Vector2(1f, 1f));
        //    vh.AddVert(vertices[3], color, new Vector2(1f, 0f));

        //    vh.AddTriangle(0, 1, 2);
        //    vh.AddTriangle(2, 3, 0);
        //    vh.FillMesh(m);
        //}

        for (int i = 0; i < Points.Length-1; i++)
        {
            var prev = Points[i];
            var cur = Points[i+1];
            prev = new Vector2(prev.x * sizeX + offsetX, prev.y * sizeY + offsetY);
            cur = new Vector2(cur.x * sizeX + offsetX, cur.y * sizeY + offsetY);

            var normal = new Vector3(cur.x - prev.x, cur.y - prev.y);
            var perp_vector = Vector3.Cross(normal, Vector3.forward).normalized;

            var halfThikness = LineThikness/2;

            var v1 = prev + new Vector2(perp_vector.x * -halfThikness, perp_vector.y * -halfThikness);
            var v2 = prev + new Vector2(perp_vector.x * halfThikness, perp_vector.y * halfThikness);
            var v3 = cur + new Vector2(perp_vector.x * halfThikness, perp_vector.y * halfThikness);
            var v4 = cur + new Vector2(perp_vector.x * -halfThikness, perp_vector.y * -halfThikness);

            vh.AddVert(v1, color, new Vector2(0f, 0f));
            vh.AddVert(v2, color, new Vector2(0f, 1f));
            vh.AddVert(v3, color, new Vector2(1f, 1f));
            vh.AddVert(v4, color, new Vector2(1f, 0f));

            vh.AddTriangle(0 + i*4, 1 + i * 4, 2 + i * 4);
            vh.AddTriangle(2 + i * 4, 3 + i * 4, 0 + i * 4);
            
        }

        vh.FillMesh(m);
    }

    Vector2 get_perp_vector(Vector2 a, Vector2 b)
    {
        Vector2 v;
        v.x = a.x - b.x;
        v.y = a.y - b.y;

        float angle = Mathf.Atan2(-v.y, v.x);
        return new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)).normalized;

    }

    //public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    //{
    //    Vector3 dir = point - pivot; // get point direction relative to pivot
    //    dir = Quaternion.Euler(angles) * dir; // rotate it
    //    point = dir + pivot; // calculate rotated point
    //    return point; // return it
    //}
}