Shader "Unlit/TransparentTile"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TransparentPos ("TransparentPos", Vector) = (0,0,0,0)
        _Radius ("Radius", Range(0,10)) = 0.5
        _Transparency ("Transparency", Range(0,1)) = 0.1
        _Filter ("Filter", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Trasparent" }
        LOD 100
        Blend One OneMinusSrcAlpha
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
                float2 xy : TEXCOORD1;
                half3 light : COLOR;
              
            };
            StructuredBuffer<float2> uv_buffer;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _TransparentPos;
            float _Transparency;
            float _Radius;
            float _Filter;
            v2f vert (appdata v)
            {
                v2f o;
                const float2 xy=v.vertex.xy;
                o.light =get_light(xy);
                o.xy=xy;
                o.vertex = UnityObjectToClipPos(float4(xy,0,0));
                 o.uv = uv_buffer[v.uv_id];
                return o;
            }
            inline float sqr_len(float2 v)
            {
                return v.x*v.x+v.y*v.y;
            }
            half4 frag (const v2f i) : SV_Target
            {
                half4 col= tex2D_bilinear( _MainTex,i.uv,_MainTex_TexelSize,0.15);
                col.a=clamp(col.a-_Radius*_Radius/sqr_len(_TransparentPos.xy-i.xy),min(_Transparency, col.a),1);
                 col.rgb=apply_light(col.rgb,i.light)*col.a;
                return col;
            }
            ENDCG
        }
    }
}
