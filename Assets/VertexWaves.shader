Shader "Unlit/VertexWaves"
{
    Properties
    {
        _Amplitude("Amplitude", float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }
        Pass
        {
            
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            
            #include "ShaderFuncs.cginc"

            float _Amplitude;
            
            struct FragmentData {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            FragmentData vert(
                float4 vertex : POSITION, // vertex position input
                float2 uv : TEXCOORD0 // first texture coordinate input
                )
            {
                FragmentData o;

                vertex.y = GetWaveRadial(uv)*_Amplitude;                
                
                o.pos = UnityObjectToClipPos(vertex);

                // passing through uvs
                o.uv = uv;
                return o;
            }

            float4 frag(FragmentData i) : SV_Target
            {
                return GetWaveRadial(i.uv);
            }
            ENDCG
        }
    }
}