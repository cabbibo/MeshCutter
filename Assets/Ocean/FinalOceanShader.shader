// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/FinalOceanShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthTextureture ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
GrabPass { "_WaterBackground" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture, _WaterBackground;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float ripple : TEXCOORD1;
                float3 normal: TEXCOORD3;
                float3 world : TEXCOORD4;
			float4 screenPos:TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _DepthTexture;

            float _Size;


            float3 offsetPos( float3 p ){
                float4 d = tex2Dlod( _MainTex, float4((p.xz/_Size) + .5,0,0) );

                return p + float3(0,d.r * .1, 0);
            }

            v2f vert (appdata v)
            {
                v2f o;

                
                float4 rippleTex = tex2Dlod( _MainTex, float4(v.uv.xy,0,0) );

                o.ripple = rippleTex.r;

                float3 fPos = offsetPos( v.vertex );//+ float3(0,rippleTex.x * .1,0);

                float eps = .05;

                float3 l = offsetPos( v.vertex + float3(eps,0,0));
                float3 r = offsetPos( v.vertex + float3(-eps,0,0));

                float3 u = offsetPos( v.vertex + float3(0,0,eps));
                float3 d = offsetPos( v.vertex + float3(0,0,-eps));

                o.normal = normalize( cross(l-r, u-d));//normalize(cross( float3(1,rippleTex.z,0),float3(1,rippleTex.w,0)));

                o.vertex = UnityObjectToClipPos (fPos);       
                o.screenPos = ComputeScreenPos(o.vertex );

                o.world = mul( UNITY_MATRIX_M , fPos );
             
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f v) : SV_Target
            {

float4 screenPos = v.screenPos;
float2 uv =(screenPos.xy) / screenPos.w;
                	float backgroundDepth =
		LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
	float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
	float depthDifference = backgroundDepth - surfaceDepth;

    float4 bgCol = tex2D(_WaterBackground, uv);





        float3 eye = normalize( _WorldSpaceCameraPos - v.world );

        float m = -dot( eye , v.normal);





                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col = tex2D(_DepthTexture, v.uv);
                col = lerp( bgCol , float4(0,0,1,0) , saturate(depthDifference));
                //col = bgCol;
                //col.r-= v.ripple * .3;//rippleTex;

              

                if( depthDifference < .2){
                    col = lerp( col , 1 , saturate((.2-depthDifference) * 10));
                }

                col = lerp( col , 1 , pow(1-m,3));

                //col.xyz = v.normal * .5 + .5;
                return col;
            }
            ENDCG
        }
    }
}
