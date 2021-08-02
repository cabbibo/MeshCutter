Shader "Ocean/RippleMat"
{
    Properties
    {
        _LastTex ("tex2D", 2D) = "white" {}
        _Height ("tex2D", 2D) = "white" {}
        _TexelSize ("TexelSize", Vector) = (1.0, 1.0, 0.0, 0.0) 
        _Resolution ("Resolution", Vector) = (1.0, 1.0, 0.0, 0.0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
                float3 worldPos : TEXCOORD1;
                float texel : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };


            float4x4 _Transform;

            sampler2D _LastTex;
            sampler2D _Height;
            sampler2D _DepthTexture;
            float4 _TexelSize;
            float4 _Resolution;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul( _Transform , float4(0, 0 , 0 , 1 )).xyz;
                float scale = mul( _Transform , float4(1, 0 , 0 , 0 )).x * 10;

                o.worldPos -= float3(v.uv.x -.5 , 0,v.uv.y-.5)* scale;
                o.texel = _TexelSize * scale;

                return o;
            }
            
        


            float2 _HitUV;
            int _Down;

            float4 newHeight(float2 uv , float3 worldPos ,float texel ) {

             // float dif = terrainWorldPos( worldPos ).y - worldPos.y;

                float2 c   = tex2D(_LastTex, uv  ).xy;

                float pressure = c.x;
                float pVel = c.y;


                

                float h_n = tex2D(_LastTex, uv + float2(0., 1.) * _TexelSize.xy ).x;
                float h_e = tex2D(_LastTex, uv + float2(1., 0.) * _TexelSize.xy ).x;
                float h_s = tex2D(_LastTex, uv + float2(0., -1.) * _TexelSize.xy ).x;
                float h_w = tex2D(_LastTex, uv + float2(-1., 0.) * _TexelSize.xy ).x;

                float  m_n = tex2D(_DepthTexture,uv + float2(0., 1.) * _TexelSize.xy );
                float  m_e = tex2D(_DepthTexture,uv + float2(1., 1.) * _TexelSize.xy );
                float  m_s = tex2D(_DepthTexture,uv + float2(0., -1.) * _TexelSize.xy );
                float  m_w = tex2D(_DepthTexture,uv + float2(-1., 0.) * _TexelSize.xy );
                //float m_n = terrainWorldPos( worldPos  + float3( 0., 0,  1.) * texel* .1).y - worldPos.y;
                //float m_e = terrainWorldPos( worldPos  + float3( 1., 0,  0.) * texel* .1).y - worldPos.y;
                //float m_s = terrainWorldPos( worldPos  + float3( 0., 0, -1.) * texel* .1).y - worldPos.y;
                //float m_w = terrainWorldPos( worldPos  + float3(-1., 0,  0.) * texel* .1).y - worldPos.y;

               // if( )

               const float delta = .1;
                float edge = 0;
                 if(m_n > .99 ){   pVel *=1;edge = 1;}
                 if(m_e > .99 ){   pVel *=1;edge = 1;}
                 if(m_s > .99 ){   pVel *=1;edge = 1;}
                 if(m_w > .99 ){   pVel *=1;edge = 1;}

                 if( edge ){
                     pressure = pressure  * .9;
                     pVel *= .9;
                 }
               // if(m_e > .9 ){  h_e = 0 ;edge = 

                // Apply horizontal wave function
                pVel += delta * (-2.0 * pressure + h_e + h_w) / 4.0;
                // Apply vertical wave function (these could just as easily have been one line)
                pVel += delta * (-2.0 * pressure + h_n + h_s) / 4.0;


// Change pressure by pressure velocity
    pressure += delta * pVel;
    
    // "Spring" motion. This makes the waves look more like water waves and less like sound waves.
    pVel -= 0.01 * delta * pressure;
    
    // Velocity damping so things eventually calm down
    pVel *= 1.0 - 0.0005 * delta;
    
    // Pressure damping to prevent it from building up forever.
    pressure *= 0.999999;
          

                return float4(pressure, pVel , (h_e - h_w) / 2.0, (h_n- h_s) / 2.0);

            }

            fixed4 frag (v2f v) : SV_Target
            {
        
                    float4 result = newHeight(v.uv , v.worldPos - float3(.5,0,.5) , v.texel);


                    if( length(v.uv-_HitUV) < .01 && _Down == 1 ){
                        result.x += (.01- length(v.uv-_HitUV)) * 5;
                    }


                   // float tPos = terrainWorldPos( v.worldPos ).y- v.worldPos.y;

                    return result;//fixed4(result.x , result.y  ,1, 1.0);
                
            }
            ENDCG
        }
    }
}
