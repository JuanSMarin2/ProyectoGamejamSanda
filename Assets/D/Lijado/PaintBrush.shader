Shader "Custom/PaintBrush"
{
    Properties
    {
        _MainTex ("Previous Paint", 2D) = "black" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Position ("Position", Vector) = (0.5,0.5,0,0)
        _Size ("Size", Float) = 0.05
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Overlay"
        }

        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float4 _Color;
            float4 _Position;
            float _Size;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex =
                    UnityObjectToClipPos(v.vertex);

                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 previous =
                    tex2D(_MainTex, i.uv);

                float distance =
                    distance(
                        i.uv,
                        _Position.xy
                    );

                float mask =
                    1.0 -
                    smoothstep(
                        _Size * 0.5,
                        _Size,
                        distance
                    );

                return lerp(
                    previous,
                    _Color,
                    mask
                );
            }

            ENDHLSL
        }
    }
}