Shader "Custom/StencilMask"
{
    SubShader
    {
        Tags { "Queue"="Geometry-10" "RenderType"="Opaque" }
        ColorMask 0 // Don't draw any color
        ZWrite Off  // Don't write depth (so it doesn't block)
        
        Stencil
        {
            Ref 1           // The stencil value we write
            Comp Always     // Always apply it
            Pass Replace    // Replace the value with 1
        }

        Pass { }
    }
}
