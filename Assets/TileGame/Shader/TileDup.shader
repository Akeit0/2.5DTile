Shader "Unlit/TileDup"
{
	Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            #include "UnityCG.cginc"
             #include "TileCG.cginc"
            struct appdata
            {
                uint2 vertex : POSITION;
                uint uv_id : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
               half3 light : COLOR;
               
            };
            StructuredBuffer<float2> uv_buffer;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            v2f vert (appdata v)
            {
                v2f o;
               const float2 xy=v.vertex.xy;
                o.light = get_light(xy);
                o.vertex = UnityObjectToClipPos(float4(xy,0,0));
                o.uv = uv_buffer[v.uv_id];
                return o;
            }

            half4 frag (const v2f i) : SV_Target
            {

              half4 col= tex2D_bilinear_vert( _MainTex,i.uv,_MainTex_TexelSize,0.15);
                clip(col.a-0.05);
                col.rgb=apply_light(col.rgb,i.light*1.2);
                return col;
            }
            ENDCG
        }
    }
}
