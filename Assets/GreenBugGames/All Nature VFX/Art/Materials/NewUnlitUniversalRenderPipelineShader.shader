Shader "Custom/URP/CoolGoldOrbWithHaloUnlit"
{
    Properties
    {
        _BaseColor   ("Base Color", Color) = (0.85, 0.65, 0.25, 1)
        _GlowColor   ("Glow Color", Color) = (1.0, 0.85, 0.35, 1)
        _Intensity   ("Core Intensity", Range(0,15)) = 6

        _RimPower    ("Rim Power", Range(0.5,10)) = 2.5
        _PulseSpeed  ("Pulse Speed", Range(0,10)) = 1.5

        _SwirlSpeed  ("Swirl Speed", Range(0,10)) = 1.2
        _SwirlScale  ("Swirl Scale", Range(0.5,20)) = 7
        _NoiseScale  ("Noise Scale", Range(0.5,20)) = 5
        _NoiseAmount ("Noise Amount", Range(0,1)) = 0.4

        // Outside swirling frame/halo controls
        _HaloIntensity ("Halo Intensity", Range(0,20)) = 10
        _HaloWidth     ("Halo Width", Range(0.01,0.8)) = 0.18
        _HaloSoftness  ("Halo Softness", Range(0.001,0.5)) = 0.08
        _HaloSwirlFreq ("Halo Swirl Frequency", Range(1,20)) = 10
        _HaloSwirlSpeed("Halo Swirl Speed", Range(0,10)) = 2.2

        // NEW: 0..1 slider controlling overall effect brightness
        _Brightness ("Effect Brightness", Range(0,1)) = 1

        _Alpha       ("Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _GlowColor;
                float  _Intensity;

                float  _RimPower;
                float  _PulseSpeed;

                float  _SwirlSpeed;
                float  _SwirlScale;
                float  _NoiseScale;
                float  _NoiseAmount;

                float  _HaloIntensity;
                float  _HaloWidth;
                float  _HaloSoftness;
                float  _HaloSwirlFreq;
                float  _HaloSwirlSpeed;

                float  _Brightness; // 0..1

                float  _Alpha;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 viewDirWS   : TEXCOORD1;
                float2 uv          : TEXCOORD2;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash21(i);
                float b = hash21(i + float2(1,0));
                float c = hash21(i + float2(0,1));
                float d = hash21(i + float2(1,1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
            }

            float fbm(float2 p)
            {
                float sum = 0.0;
                float amp = 0.5;
                float freq = 1.0;
                [unroll] for (int i = 0; i < 4; i++)
                {
                    sum += amp * valueNoise(p * freq);
                    freq *= 2.0;
                    amp *= 0.5;
                }
                return sum;
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrm = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = pos.positionCS;
                OUT.normalWS    = normalize(nrm.normalWS);
                OUT.viewDirWS   = normalize(GetWorldSpaceViewDir(pos.positionWS));
                OUT.uv          = IN.uv;

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float t = _Time.y;

                // View-based rim (orb feel)
                float ndv = saturate(dot(IN.normalWS, IN.viewDirWS));
                float rim = pow(1.0 - ndv, _RimPower);

                // Centered UV for radial masks
                float2 cUV = (IN.uv - 0.5) * 2.0; // -1..1
                float dist = length(cUV);         // 0 center -> ~1 edge

                // Core brightness falloff
                float core = pow(saturate(1.0 - dist), 2.0);

                // Inner swirl/energy
                float angle = atan2(cUV.y, cUV.x);
                float swirl = sin(angle * 3.0 + t * _SwirlSpeed) * 0.5 + 0.5;

                // NOTE: _SwirlScale affects pattern frequency; keep it in the UV space.
                float2 uvScaled = IN.uv * _SwirlScale;

                float n = fbm(uvScaled * _NoiseScale + float2(t * 0.4, t * -0.3));
                float energy = saturate(lerp(swirl, n, _NoiseAmount));

                // Pulse
                float pulse = sin(t * _PulseSpeed) * 0.5 + 0.5;
                pulse = lerp(0.8, 1.4, pulse);

                // ---------- OUTER SWIRLING HALO ----------
                float ringCenter = 1.0 - _HaloWidth;
                float ring = smoothstep(ringCenter - _HaloSoftness, ringCenter, dist) *
                             (1.0 - smoothstep(1.0 - _HaloSoftness, 1.0, dist));

                float haloWave = sin(angle * _HaloSwirlFreq + t * _HaloSwirlSpeed) * 0.5 + 0.5;
                float haloNoise = fbm(float2(angle * 2.0, dist * 6.0) + float2(t * 0.9, t * 0.2));
                float haloStrands = saturate(lerp(haloWave, haloNoise, 0.55));

                float halo = ring * haloStrands * (0.4 + rim);

                // Combine masks
                float glowMask = saturate(core * 1.2 + rim * 1.2 + energy * 0.8);

                // Color build
                float3 baseCol = _BaseColor.rgb * (0.3 + core);
                float3 fxCol =
                    (_GlowColor.rgb * glowMask * _Intensity * pulse) +
                    (_GlowColor.rgb * halo * _HaloIntensity);

                // NEW: 0..1 slider controls the effect brightness (glow + halo),
                // while leaving the base visible.
                float3 col = baseCol + fxCol * _Brightness;

                float alpha = saturate(_Alpha * (0.35 + core + rim + halo));

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
