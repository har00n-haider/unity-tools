
#include "UnityCG.cginc"

float InverseLerp(float a, float b, float v)
{
    return (v - a) / (b - a);
}

float SquareWave(float a)
{
    return (floor(a) - floor(a + 0.5)) + 1;
}

float NZigZag(float a)
{
    return acos(cos(a * UNITY_TWO_PI))/UNITY_PI;
}

float NCos(float a)
{
    return cos(a * UNITY_TWO_PI) * 0.5 + 0.5;
}

float GetWaveRadial(float2 uv)
{
    float2 uvsCentered = uv*2-1;
    float radialDist = length(uvsCentered);
    float wave = cos((radialDist - _Time.y*0.1)*UNITY_TWO_PI*5);
    wave *= 1 -radialDist; // fade out at extremities
    return wave;
}

#define RED float4(1,0,0,0)
#define BLUE float4(0,0,1,0)
#define GREEN float4(0,1,0,0)
