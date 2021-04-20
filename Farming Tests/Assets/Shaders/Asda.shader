Shader "Hole" {
    Properties{
        _Color("Main Color", Color) = (1,1,1,0)
    }
        SubShader{
            Tags { "RenderType" = "Transparent" "Queue" = "Geometry+2"}

            ColorMask RGB
            Cull Front
            ZWrite On
            ZTest NotEqual
            Stencil {
                Ref 1
                Comp Always
            }

            CGPROGRAM
            #pragma surface surf Lambert
            float4 _Color;
            struct Input {
                float4 color : COLOR;
            };
            void surf(Input IN, inout SurfaceOutput o) {
                o.Albedo = _Color;
                o.Normal = half3(0,0,-1);
                o.Alpha = _Color.a;
            }
            ENDCG
    }
}