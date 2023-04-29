Shader "Unlit/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("NoiseTexture", 2D) = "white" {}
           _LightMap ("LightMap", 2D) = "white" {}
        _Offset ("Offset", Range(0,1)) =0
        _Scale ("Scale", Range(0,1)) =0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"
             "Queue"="Transparent+1001" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                 // float light : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
             sampler2D _LightMap;
            float _Offset;
            float _Scale;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                //   o.light = tex2Dlod(_LightMap, float4(v.uv,0,0)).a;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                i.uv.y+=cos(64*(i.uv.x+_Time))/400;
               const  half4 base_col = tex2D(_MainTex, i.uv);
                half4 col = base_col*(_Offset+_Scale*tex2D(_NoiseTex, i.uv+_Time/5).r);
                col.rgb*=tex2D(_LightMap, i.uv).a;
                clip(col.a-0.1);
                return col;
            }
            ENDCG
        }
    }
}
