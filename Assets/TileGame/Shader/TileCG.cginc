

inline float4 tex2D_bilinear(const sampler2D tex,const float2 uv,const float4 texel_size,const float filter)
 {
    const float2 t = filter * texel_size.xy;
     const float4 c1 = tex2D(tex, uv +t);
     const float4 c2 = tex2D(tex, uv + float2(t.x, -t.y));
     const float4 c3 = tex2D(tex, uv + float2(-t.x, t.y));
     const float4 c4 = tex2D(tex, uv - t);
     const float2 d = frac(uv * texel_size.zw-0.498);
     return lerp( lerp(c4, c2, d.x), lerp(c3, c1, d.x), d.y);
 }
inline float4 tex2D_bilinear_vert(const sampler2D tex,const float2 uv,const float4 texel_size,const float filter)
 {
    const float2 t = float2(0,filter * texel_size.y);
     const float4 c1 = tex2D(tex, uv +t);
     const float4 c2 = tex2D(tex, uv - t);
     const float dy = frac(uv.y * texel_size.w-0.498);
     return lerp( c1,c2, dy);
 }
sampler2D _GlobalLightMap;
float4 _GlobalLightMap_TexelSize;
float _GlobalLightBloom;

inline half3 get_light(const float2 xy)
{
    return   tex2Dlod(_GlobalLightMap, float4(xy*_GlobalLightMap_TexelSize.xy,0,0)).rgb;
}


inline half3 apply_light(const half3 rgb,const float2 xy)
{
    const half3 light=get_light(xy);
    return    (rgb+light*_GlobalLightBloom)*light;
}
inline half3 apply_light(const half3 rgb,const float3 lit)
{
    return    (rgb+lit*_GlobalLightBloom)*lit;
}

inline half4 tex2D_lit(const sampler2D tex,const float2 uv,const float2 xy)
{
    half4 col = tex2D(tex, uv);
    const half3 lit=tex2D(_GlobalLightMap, xy*_GlobalLightMap_TexelSize.xy).rgb;
    col.rgb=apply_light(col.rgb,lit);
    return col;
}
