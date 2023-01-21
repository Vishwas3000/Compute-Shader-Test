Shader "Unlit/HelixCream"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset("Position Offset", Float) = (0,0,0,0)
        _Rate("Pouring Rate", Float) = 1.0
        _Color("Color", Color) = (1,1,1,1)
        _Amount("Amount", Float) = 0.05
        _Point("Point", Float) = 5.0
        _Omega("Angular Velocity", Float) = 20
        _Val("Height Infulance", Float) = 10
        _Check("Rest Distance", FLoat) = 10
        _Call("Change position", Range(0,1)) = 1
        _debugPoint("Debug Point", Float) = 1
        _RestTime("RestTime", Float) = 1
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
            #pragma target 5.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint vid : SV_VertexID;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float time : TEXTCOORD1;
            };

            struct vertexData
            {
                float restTime;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Offset;
            fixed4 _Color;
            float _Rate, _Amount, _Point, _Omega, _Val, _Check, _debugPoint;
            float _Call;

            RWStructuredBuffer<vertexData> buffer;
            
            float curveFun(float val)
            {
                return val * val;
            }
            v2f vert (appdata IN)
            {
                v2f OUT;
                float dist = _Point-IN.vertex.y;
                float HInf = dist * _Val;

                if (_Call)
                {
                    IN.vertex.x += sin(_Time * _Omega + HInf) * curveFun(dist * _Amount);
                    IN.vertex.z += cos(_Time * _Omega + HInf) * curveFun(dist * _Amount);
                    IN.vertex.y += _Rate * _Time + _Offset;
                    
                    buffer[IN.vid].restTime = _Time;
                }
                else
                {
                    float time = buffer[IN.vid].restTime;
                    IN.vertex.x += sin(time * _Omega + HInf) * curveFun(dist * _Amount);
                    IN.vertex.z += cos(time * _Omega + HInf) * curveFun(dist * _Amount);
                    IN.vertex.y += time * _Rate + _Offset;
                }

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                OUT.time = buffer[IN.vid].restTime - floor(buffer[IN.vid].restTime);
                
                return OUT;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;
            col *= i.time;

                return col;
            }
            ENDCG
        }
    }
}
