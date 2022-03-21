//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

Shader "EasyAR/CameraImage_YUV_NV21"
{
    Properties
    {
        _yTexture("Texture", 2D) = "white" {}
        _uvTexture("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _yTexture;
            float4 _yTexture_ST;
            sampler2D _uvTexture;
            float4x4 _DisplayTransform;

            v2f vert(appdata i)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.texcoord = TRANSFORM_TEX(i.texcoord, _yTexture);
                o.texcoord = float2(o.texcoord.x, 1.0 - o.texcoord.y);
                o.texcoord = mul(_DisplayTransform, float3(o.texcoord, 1.0f)).xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                const float4x4 ycbcrToRGBTransform = float4x4(
                    float4(1.0, +0.0000, +1.4020, -0.7010),
                    float4(1.0, -0.3441, -0.7141, +0.5291),
                    float4(1.0, +1.7720, +0.0000, -0.8860),
                    float4(0.0, +0.0000, +0.0000, +1.0000)
                    );
                float y = tex2D(_yTexture, i.texcoord).a;
                float4 rgb4444 = tex2D(_uvTexture, i.texcoord);
                float cr = rgb4444.b * 16 / 17 + rgb4444.a / 17;
                float cb = rgb4444.r * 16 / 17 + rgb4444.g / 17;
                float4 ycbcr = float4(y, cb, cr, 1.0);
                float4 col = mul(ycbcrToRGBTransform, ycbcr);
#ifndef UNITY_COLORSPACE_GAMMA
                col.xyz = GammaToLinearSpace(col.xyz);
#endif
                return col;
            }
            ENDCG
        }
    }
}
