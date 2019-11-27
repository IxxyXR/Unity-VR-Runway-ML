Shader "Unlit/Camera Effect Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BarrelPower("Barrel Power", Float) = 1.0

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
            uniform float _BarrelPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float2 distort(float2 pos)
            {
                float theta = atan2(pos.y, pos.x);
                float radius = length(pos);
                radius = pow(radius, _BarrelPower);
                pos.x = radius * cos(theta);
                pos.y = radius * sin(theta);

                return 0.5 * (pos + 1.0);
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float2 xy = 2.0 * i.uv - 1.0;

                float d = length(xy);

                if (d >= 1.0)
                {
                    discard;
                }

                float2 uv = distort(xy);

                // sample the texture
                fixed4 col = tex2D(_MainTex, uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
