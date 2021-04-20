Shader "Unlit/EdgeDetectionUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold("Edge Threshold", Float) = 0.5
        _EdgeColor("Edge Color", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _MainTex_TexelSize;

            float _Threshold;
            float4 _EdgeColor;

            float getLuminanceValue(float2 uv, float2 uvOffset, float kernelMultiplier) {
                fixed4 col = tex2D(_MainTex, uv + uvOffset);
                return (col.r * 0.30 + col.g * 0.59 + col.b * 0.11) * kernelMultiplier;
            }

            float3 performSobelOperation(float2 uv) {
                float horizontal = 0;
                float vertical = 0;

                //Horizontal Matrix
                horizontal += getLuminanceValue(uv, float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y), -1);
                horizontal += getLuminanceValue(uv, float2(-_MainTex_TexelSize.x, 0), -2);
                horizontal += getLuminanceValue(uv, float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y), -1);
                horizontal += getLuminanceValue(uv, float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y), 1);
                horizontal += getLuminanceValue(uv, float2(_MainTex_TexelSize.x, 0), 2);
                horizontal += getLuminanceValue(uv, float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y), 1);

                //Vertical Matrix
                vertical += getLuminanceValue(uv, float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y), -1);
                vertical += getLuminanceValue(uv, float2(0, -_MainTex_TexelSize.y), -2);
                vertical += getLuminanceValue(uv, float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y), -1);
                vertical += getLuminanceValue(uv, float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y), 1);
                vertical += getLuminanceValue(uv, float2(0, _MainTex_TexelSize.y), 2);
                vertical += getLuminanceValue(uv, float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y), 1);

                return sqrt((horizontal * horizontal) + (vertical * vertical));
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 sobelResult = performSobelOperation(i.uv);
                if (sobelResult.r >= _Threshold && sobelResult.g >= _Threshold && sobelResult.b >= _Threshold)
                    return _EdgeColor;
                else
                    return col;
            }
            ENDCG
        }
    }
}
