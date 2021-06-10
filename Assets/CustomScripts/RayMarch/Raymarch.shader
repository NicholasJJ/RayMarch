Shader "Unlit/Raymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

#define MAX_STEPS 100
#define MAX_DIST 100
#define SURF_DIST 1e-3
#define MAX_OBJS 10

            uniform float4 _objPos[MAX_OBJS];
            uniform float4 _objcolor[MAX_OBJS];

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1));
                //o.hitPos = v.vertex;
                o.ro = _WorldSpaceCameraPos;
                o.hitPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float pMod(float p, float size)
            {
                float halfsize = size * 0.5;
                float c = floor((p + halfsize) / size);
                p = fmod(p + halfsize, size) - halfsize;
                p = fmod(p - halfsize, size) + halfsize;
                //p = fmod(p, size);
                return p;
            }


            float opSmoothUnion(float d1, float d2, float k) {
                float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
                return lerp(d2, d1, h) - k * h * (1.0 - h);
            }

            float SphereDist(float3 p, float3 c, float r) {
                //p = fmod(p, 10);
                return length(p - c) - r;
            }

            float GetDist(float3 p) {
                //p = float3(p.x % 2, p.y % 2, p.z % 2);
                //float d = SphereDist(p,float3(0,0,0),.5); //sphere at center
                float3 mp;
                //mp.x = pMod(p.x, 5);
                //mp.y = pMod(p.y, 5);
                //mp.z = pMod(p.z, 5);
                mp.yxz = p.yxz;
                float d = length(mp) - .5;
                for (int i = 0; i < MAX_OBJS; i++) {
                    if (_objPos[i].w == 0) break;
                    if (_objPos[i].w == 1) {
                        float nd = SphereDist(p, _objPos[i], .5);
                        d = min(d, nd);
                    }
                }
                
                d = min(d, p.y);
                return d;
            }

            float Raymarch(float3 ro, float3 rd) {
                float dO = 0;
                float dS;
                for (int i = 0; i < MAX_STEPS; i++) {
                    float3 p = ro + (dO * rd);
                    dS = GetDist(p);
                    dO += dS;
                    if (dS<SURF_DIST || dO>MAX_DIST) break;
                }

                return dO;
            }

            float3 GetNormal(float3 p) {
                float2 e = float2(1e-2, 0);
                float3 n = GetDist(p) - float3(
                    GetDist(p - e.xyy),
                    GetDist(p - e.yxy),
                    GetDist(p - e.yyx)
                    );
                return normalize(n);
            }

            float GetLight(float3 p) {
                float3 lightPos = float3(1, 3, 0);
                float3 l = normalize(lightPos - p);
                float3 n = GetNormal(p);
                float3 dif = clamp(dot(n, l),0,1);

                //shadows
                float d = Raymarch(p+n*SURF_DIST*2, l);
                if (d < length(lightPos - p)) dif *= .1;
                return dif;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv -0.5;
                float3 ro = i.ro;
                float3 rd = normalize(i.hitPos - ro);

                float d = Raymarch(ro, rd);
                fixed4 col = 0;

                if (d >= MAX_DIST) {
                    discard;
                }
                else {
                    float3 p = ro + (rd * d);
                    float dif = GetLight(p);
                    //float3 n = GetNormal(p);
                    //col.rgb = float3(dif*n.x,dif*n.y,dif*n.z);
                    col.rgb = float3(dif, dif, dif);
                }

                return col;
            }
            ENDCG
        }
    }
}
