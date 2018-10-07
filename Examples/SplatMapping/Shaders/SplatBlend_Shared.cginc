


struct Input 
{
   float2 uv_Tex1;
   float4 color : COLOR;
   float3 viewDir;
   #if (_FLOW1 || _FLOW2 || _FLOW3 || _FLOW4 || _FLOW5)
   float4 flowDir;
   #endif
   #if _DISTBLEND
   float3 worldPos;
   #endif
};

// macro for one layer of texture data      
#define LAYER(__N) sampler2D _Tex##__N; fixed4 _Tint##__N; sampler2D _Normal##__N; sampler2D _GlossinessTex##__N; half _Glossiness##__N; half _Metallic##__N; half _Parallax##__N; half _TexScale##__N; half _Contrast##__N; sampler2D _Emissive##__N; half _EmissiveMult##__N; fixed4 _SpecColor##__N; sampler2D _SpecGlossMap##__N; float _DistUVScale##__N;

LAYER(1)
LAYER(2)
LAYER(3)
LAYER(4)
LAYER(5)

half  _FlowSpeed;
half  _FlowIntensity;
fixed _FlowAlpha;
half  _FlowRefraction;
float _DistBlendMin;
float _DistBlendMax;

#if _DISTBLEND
#define COMPUTEDISTBLEND float dist = saturate((distance(_WorldSpaceCameraPos, IN.worldPos) / _DistBlendMax) - _DistBlendMin);
#else
#define COMPUTEDISTBLEND  
#endif


// macros to make dealing with the flow map option not a nightmare
#if (_FLOW1 || _FLOW2 || _FLOW3 || _FLOW4 || _FLOW5)
#define INIT_FLOW half flowInterp; float2 fuv1; float2 fuv2; Flow(IN.flowDir.xy, IN.flowDir.zw, _FlowSpeed, _FlowIntensity, fuv1, fuv2, flowInterp);
#else
#define INIT_FLOW  
#endif

// we define the function based on what channel is actively compiled for flow data - other combinations else into a standard tex2D
#if _FLOW1
#define FETCH_TEX1(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX1(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale1), dist)
#else
#define FETCH_TEX1(_T, _UV) tex2D(_T, _UV)
#endif

#if _FLOW2
#define FETCH_TEX2(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX2(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale2), dist)
#else
#define FETCH_TEX2(_T, _UV) tex2D(_T, _UV)
#endif

#if _FLOW3
#define FETCH_TEX3(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX3(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale3), dist)
#else
#define FETCH_TEX3(_T, _UV) tex2D(_T, _UV)
#endif

#if _FLOW4
#define FETCH_TEX4(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX4(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale4), dist)
#else
#define FETCH_TEX4(_T, _UV) tex2D(_T, _UV)
#endif

#if _FLOW5
#define FETCH_TEX5(_T, _UV) lerp(tex2D(_T, fuv1), tex2D(_T, fuv2), flowInterp)
#elif _DISTBLEND
#define FETCH_TEX5(_T, _UV) lerp(tex2D(_T, _UV), tex2D(_T, _UV*_DistUVScale5), dist)
#else
#define FETCH_TEX5(_T, _UV) tex2D(_T, _UV)
#endif  

// given two height values (from textures) and a height value for the current pixel (from vertex)
// compute the blend factor between the two with a small blending area between them.
half HeightBlend(half h1, half h2, half slope, half contrast)
{
   h2 = 1-h2;
   half tween = saturate( ( slope - min( h1, h2 ) ) / max(abs( h1 - h2 ), 0.001)); 
   half threshold = contrast;
   half width = 1.0 - contrast;
   return saturate( ( tween - threshold ) / max(width, 0.001) );
}

void Flow(float2 uv, half2 flow, half speed, float intensity, out float2 uv1, out float2 uv2, out half interp)
{
   float2 flowVector = (flow * 2.0 - 1.0) * intensity;
   
   float timeScale = _Time.y * speed;
   float2 phase = frac(float2(timeScale, timeScale + .5));

   uv1 = (uv - flowVector * half2(phase.x, phase.x));
   uv2 = (uv - flowVector * half2(phase.y, phase.y));
   
   interp = abs(0.5 - phase.x) / 0.5;
   
}

void SharedVert (inout appdata_full v, out Input o) 
{
    UNITY_INITIALIZE_OUTPUT(Input,o);
    #if (_FLOW1 || _FLOW2 || _FLOW3 || _FLOW4 || _FLOW5)
    o.flowDir.xy = v.texcoord.xy;
    o.flowDir.zw = v.texcoord2.xy;
    #endif
    
    #if (_FLOW1)
    o.flowDir.xy *= _TexScale1;
    #endif
    #if (_FLOW2)
    o.flowDir.xy *= _TexScale2;
    #endif
    #if (_FLOW3)
    o.flowDir.xy *= _TexScale3; 
    #endif
    #if (_FLOW4)
    o.flowDir.xy *= _TexScale4;
    #endif
    #if (_FLOW5)
    o.flowDir.xy *= _TexScale5;
    #endif

    o.uv_Tex1 = v.texcoord.xy;
    o.color = v.color;
    o.viewDir = float3(0,0,1);

    #if _DISTBLEND
    o.worldPos = float3(0,0,0);
    #endif
    // vertex shader version, sad- can't do it here because of surface shader limitations/texcoord stuffing issues
    //#if (_FLOW1 || _FLOW2 || _FLOW3 || _FLOW4 || _FLOW5)
    //half flowInterp; 
    //float2 fuv1; 
    //float2 fuv2; 
    //Flow(o.flowDir.xy + _Time.y * _FlowMove, o.flowDir.zw, _FlowSpeed, _FlowIntensity, fuv1, fuv2, flowInterp);
    //o.flowDir.xy = fuv1;
    //o.flowDir.zw = fuv2;
    //o.interp = float4(saturate(flowInterp));
    //#endif
}
