using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VertexInput
{
    public Vector3 vertex;
    public Vector2 uv;
}
public struct FragmentInput
{
    public Vector2 uv;
}
public struct FragmentOutput
{
    public Color color;
}

public class ShaderBase 
{
    public virtual FragmentInput Vertex(VertexInput input)
    {
        return default(FragmentInput);
    }
    public virtual FragmentOutput Fragment(FragmentInput input)
    {
        return default(FragmentOutput);
    }
}
