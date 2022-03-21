Shader "Custom/AlphaBlendBothSidedShader"
{
    Properties
    {
        _Color ("Main Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _AlphaScale ("Alpha Scale", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        pass
        {
            Tags {"LightMode"="ForwardBase"}
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Lighting.cginc"

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _AlphaScale;

            struct a2v
            {
                float4 vertex :  POSITION; //vertex pos object
                float3 normal : NORMAL; //normal direction object
                float4 texcoord : TEXCOORD0; //textrue cooedinate
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert(a2v i)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(i.vertex);
                o.worldNormal = UnityObjectToWorldNormal(i.normal);
                o.worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
                o.uv = TRANSFORM_TEX(i.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 worldNormalDir = normalize(i.worldNormal);
                fixed3 worldLightDir = UnityWorldSpaceLightDir(i.worldPos);
                fixed4 texColor = tex2D(_MainTex, i.uv);

                fixed3 albedo = texColor * _Color.rgb;
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo;
                fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(worldNormalDir, worldLightDir));

                return fixed4(ambient + diffuse, texColor.a * _AlphaScale);
            }
            ENDCG
        }

        pass
        {
            Tags {"LightMode"="ForwardBase"}
            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Lighting.cginc"

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _AlphaScale;

            struct a2v
            {
                float4 vertex :  POSITION; //vertex pos object
                float3 normal : NORMAL; //normal direction object
                float4 texcoord : TEXCOORD0; //textrue cooedinate
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert(a2v i)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(i.vertex);
                o.worldNormal = UnityObjectToWorldNormal(i.normal);
                o.worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
                o.uv = TRANSFORM_TEX(i.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 worldNormalDir = normalize(i.worldNormal);
                fixed3 worldLightDir = UnityWorldSpaceLightDir(i.worldPos);
                fixed4 texColor = tex2D(_MainTex, i.uv);

                fixed3 albedo = texColor * _Color.rgb;
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo;
                fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(worldNormalDir, worldLightDir));

                return fixed4(ambient + diffuse, texColor.a * _AlphaScale);
            }
            ENDCG
        }
    }

    Fallback "Transparent/VertexLit"
}
