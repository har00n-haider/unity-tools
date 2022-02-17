Shader "Unlit/Textured"
{
    Properties
    {
        // flat normal map color is applied by default.
        // can choose from white, black, grey, and bump
        _MainTex ("Texture", 2D) = "white" {}
        _Pattern ("Pattern", 2D) = "white" {}
        _SubTex ("Sub Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"
            #include "ShaderFuncs.cginc"
            
            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentData
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos: TEXCOORD1;
            };

            sampler2D _Pattern;
            sampler2D _MainTex;
            sampler2D _SubTex;
            // optional, provided by Unity and is affected by the Tiling (scale/offset) values
            float4 _MainTex_ST;

            FragmentData vert (MeshData meshData)
            {
                FragmentData o;
                o.worldPos = mul(unity_ObjectToWorld, meshData.vertex);
                
                o.vertex = UnityObjectToClipPos(meshData.vertex);

                // can the _ST value to apply scale/offset values using TRANSFORM_TEX
                o.uv = meshData.uv;

                return o;
            }

            float GetWave(float2 coord)
            {
                float wave = NCos((coord + _Time.y*0.1)*UNITY_TWO_PI*5);
                wave *= coord; // fade out at extremities
                return wave;
            }

            float4 frag (FragmentData fragData) : SV_Target
            {

                float2 topDownProj = fragData.worldPos.xz;
                // sample the texture
                float4 textureColor = tex2D(_MainTex, topDownProj);
                float4 subTextureColor = tex2D(_SubTex, topDownProj);
                
                float patternColor = tex2D(_Pattern, fragData.uv);

                float4 finalColor = lerp(subTextureColor, textureColor, patternColor);
                    
                return finalColor;
            }
            ENDCG
        }
    }
}
