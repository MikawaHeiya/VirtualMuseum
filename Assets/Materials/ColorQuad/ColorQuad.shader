Shader "Custom/ColorQuad"
{
	properties
	{
		_N("����俪ʼ����",range(0,100)) = 0
		_R("������������",range(0,100)) = 20
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
			//������������ľ���
			float D = length(IN.wordPos.xyz - _WorldSpaceCameraPos.xyz);
			//��ת���룬Լ����С��1
			D = (_R - D + _N) / _R;
			//Լ����1~0��Χ��
			D = saturate(D);

			//�����ɫ����
			return _Color + float4(D, D, D, 1) * _Color;
		};

		ENDCG
		}
	}
}