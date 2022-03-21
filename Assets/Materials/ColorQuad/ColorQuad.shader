Shader "Custom/ColorQuad"
{
	properties
	{
		_N("景深渐变开始距离",range(0,100)) = 0
		_R("景深渐变结束距离",range(0,100)) = 20
		_Color("Main Tint", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Pass
		{

		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#include "unitycg.cginc"
		#pragma target 3.0

		float _N;
		float _R;
		float4 _Color;

		struct v2f
		{
			float4 pos:SV_POSITION;
			float4 wordPos:TEXCOORD0;
		};

		v2f vert(appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.wordPos = mul(unity_ObjectToWorld,v.vertex);
			return o;
		};

		fixed4 frag(v2f IN) : COLOR
		{
			//摄像机到物体点的距离
			float D = length(IN.wordPos.xyz - _WorldSpaceCameraPos.xyz);
			//反转距离，约束到小于1
			D = (_R - D + _N) / _R;
			//约束到1~0范围内
			D = saturate(D);

			//输出颜色数据
			return _Color + float4(D, D, D, 1) * _Color;
		};

		ENDCG
		}
	}
}