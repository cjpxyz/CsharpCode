// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GaussianBlur"
{
	//-----------------------------------������ || Properties��------------------------------------------  
	Properties
	{
		//������
		_MaskTex("Mask (R)", 2D) = "white" {}
	 
	_DownSampleValue("_DownSampleValue1",float) = 0.75
		_DownSampleValue2("_DownSampleValue2",float) = 0.751
		_DownSampleValue3("_DownSampleValue3",float) = 0.752
		
	}

		//----------------------------------������ɫ�� || SubShader��---------------------------------------  
		SubShader
	{
		ZWrite Off
		Blend Off
	
	
		
		GrabPass{}
		//---------------------------------------��ͨ��0 || Pass 0��------------------------------------
		//ͨ��0��������ͨ�� ||Pass 0: Down Sample Pass
		Pass
	{
		ZTest Off
		Cull Off

		CGPROGRAM

		//ָ����ͨ���Ķ�����ɫ��Ϊvert_DownSmpl
#pragma vertex vert_DownSmpl
		//ָ����ͨ����������ɫ��Ϊfrag_DownSmpl
#pragma fragment frag_DownSmpl

		ENDCG

	}
		GrabPass{}
		//---------------------------------------��ͨ��1 || Pass 1��------------------------------------
		//ͨ��1����ֱ����ģ������ͨ�� ||Pass 1: Vertical Pass
		Pass
	{
		ZTest Always
		Cull Off

		CGPROGRAM

		//ָ����ͨ���Ķ�����ɫ��Ϊvert_BlurVertical
#pragma vertex vert_BlurVertical_1
		//ָ����ͨ����������ɫ��Ϊfrag_Blur
#pragma fragment frag_Blur

		ENDCG
	}
		GrabPass{  }
		//---------------------------------------��ͨ��2 || Pass 2��------------------------------------
		//ͨ��2��ˮƽ����ģ������ͨ�� ||Pass 2: Horizontal Pass
		Pass
	{
		ZTest Always
		Cull Off

		CGPROGRAM

		//ָ����ͨ���Ķ�����ɫ��Ϊvert_BlurHorizontal
#pragma vertex vert_BlurHorizontal_1
		//ָ����ͨ����������ɫ��Ϊfrag_Blur
#pragma fragment frag_Blur

		ENDCG
	}



	 
	 
		 GrabPass{ }
		//---------------------------------------��ͨ��1 || Pass 1_2��------------------------------------
		//ͨ��1����ֱ����ģ������ͨ�� ||Pass 1: Vertical Pass
		Pass
	{
		ZTest Always
		Cull Off

		CGPROGRAM

		//ָ����ͨ���Ķ�����ɫ��Ϊvert_BlurVertical
#pragma vertex vert_BlurVertical_2
		//ָ����ͨ����������ɫ��Ϊfrag_Blur
#pragma fragment frag_Blur

		ENDCG
	}
		GrabPass{  }
		//---------------------------------------��ͨ��2 || Pass 2_2��------------------------------------
		//ͨ��2��ˮƽ����ģ������ͨ�� ||Pass 2: Horizontal Pass
		Pass
	{
		ZTest Always
		Cull Off

		CGPROGRAM

		//ָ����ͨ���Ķ�����ɫ��Ϊvert_BlurHorizontal
#pragma vertex vert_BlurHorizontal_2
		//ָ����ͨ����������ɫ��Ϊfrag_Blur
#pragma fragment frag_Blur

		ENDCG
	}



		 GrabPass{  }
			//---------------------------------------��ͨ��1 || Pass 1_3��------------------------------------
			//ͨ��1����ֱ����ģ������ͨ�� ||Pass 1: Vertical Pass
			Pass
		{
			ZTest Always
			Cull Off

			CGPROGRAM

			//ָ����ͨ���Ķ�����ɫ��Ϊvert_BlurVertical
#pragma vertex vert_BlurVertical_3
			//ָ����ͨ��������ɫ��Ϊfrag_Blur
#pragma fragment frag_Blur

			ENDCG
		}
			GrabPass{  }
			//---------------------------------------��ͨ��2 || Pass 2_3��------------------------------------
			//ͨ��2��ˮƽ����ģ������ͨ�� ||Pass 2: Horizontal Pass
			Pass
		{
			ZTest Always
			Cull Off

			CGPROGRAM

			//ָ����ͨ���Ķ�����ɫ��Ϊvert_BlurHorizontal
#pragma vertex vert_BlurHorizontal_3
			//ָ����ͨ����������ɫ��Ϊfrag_Blur
#pragma fragment frag_Blur

			ENDCG
		}
//


	}


		//-------------------------CG��ɫ������������ || Begin CG Include Part----------------------  
		CGINCLUDE

		//��1��ͷ�ļ����� || include
#include "UnityCG.cginc"

		//��2���������� || Variable Declaration
		sampler2D _MaskTex;
		sampler2D _GrabTexture;
		 
	//UnityCG.cginc�����õı����������еĵ����سߴ�|| it is the size of a texel of the texture
	//uniform half4 _MainTex_TexelSize;
	uniform half4 _GrabTexture_ST;
	//C#�ű����Ƶı��� || Parameter
	  half _DownSampleValue;
	  half _DownSampleValue2;
	  half _DownSampleValue3;


	//��3����������ṹ�� || Vertex Input Struct
	struct VertexInput
	{
		//����λ������
		float4 vertex : POSITION;
		//һ����������
		half2 texcoord : TEXCOORD0;
	};

	//��4������������ṹ�� || Vertex Input Struct
	struct VertexOutput_DownSmpl
	{
		//����λ������
		float4 pos : SV_POSITION;
		//һ���������꣨���ϣ�
		half2 uv20 : TEXCOORD0;
		//�����������꣨���£�
		half2 uv21 : TEXCOORD1;
		//�����������꣨���£�
		half2 uv22 : TEXCOORD2;
		//�ļ��������꣨���ϣ�
		half2 uv23 : TEXCOORD3;
	};


	//��5��׼����˹ģ��Ȩ�ؾ������7x4�ľ��� ||  Gauss Weight
	static const half4 GaussWeight[7] =
	{
		half4(0.0205,0.0205,0.0205,0),
		half4(0.0855,0.0855,0.0855,0),
		half4(0.232,0.232,0.232,0),
		half4(0.324,0.324,0.324,1),
		half4(0.232,0.232,0.232,0),
		half4(0.0855,0.0855,0.0855,0),
		half4(0.0205,0.0205,0.0205,0)
	};


	//��6��������ɫ���� || Vertex Shader Function
	VertexOutput_DownSmpl vert_DownSmpl(VertexInput v)
	{
		//��6.1��ʵ����һ������������ṹ
		VertexOutput_DownSmpl o;

		//��6.2���������ṹ
		//����ά�ռ��е�����ͶӰ����ά����  
		o.pos = UnityObjectToClipPos(v.vertex);
		//��ͼ��Ľ�������ȡ��������������Χ�ĵ㣬�ֱ�����ļ�����������
		//v.texcoord.y = 1- v.texcoord.y;
		float w = 1080/5;
		o.uv20 = v.texcoord + _GrabTexture_ST.xy* half2(0.5h+0.5h, 0.5h + 0.5h)/ w;
		o.uv21 = v.texcoord + _GrabTexture_ST.xy * half2(-0.5h + 0.5h, -0.5h + 0.5h) / w;
		o.uv22 = v.texcoord + _GrabTexture_ST.xy * half2(0.5h + 0.5h, -0.5h + 0.5h) / w;
		o.uv23 = v.texcoord + _GrabTexture_ST.xy * half2(-0.5h + 0.5h, 0.5h + 0.5h) / w;
		 
		//��6.3���������յ�������
		return o;
	}

	//��7��Ƭ����ɫ���� || Fragment Shader Function
	fixed4 frag_DownSmpl(VertexOutput_DownSmpl i) : SV_Target
	{
		clip(tex2D(_MaskTex, i.uv20).r - 0.5);
		//��7.1������һ����ʱ����ɫֵ
		fixed4 color = (0,0,0,0);

	//��7.2���ĸ��������ص㴦������ֵ���
	color += tex2D(_GrabTexture, i.uv20);
	color += tex2D(_GrabTexture, i.uv21);
	color += tex2D(_GrabTexture, i.uv22);
	color += tex2D(_GrabTexture, i.uv23);
	 
	//��7.3���������յ�ƽ��ֵ
	return color / 4;
	}

		//��8����������ṹ�� || Vertex Input Struct
		struct VertexOutput_Blur
	{
		//��������
		float4 pos : SV_POSITION;
		//һ�������������꣩
		half4 uv : TEXCOORD0;
		//��������ƫ������
		half2 offset : TEXCOORD1;
	};


	VertexOutput_Blur vert_BlurHorizontal(VertexInput v,float it)
	{
		//��9.1��ʵ����һ������ṹ
		VertexOutput_Blur o;

		//��9.2���������ṹ
		//����ά�ռ��е�����ͶӰ����ά����  
		o.pos = UnityObjectToClipPos(v.vertex);
		//��������
		o.uv = half4(v.texcoord.xy, 1, 1);
		//����X�����ƫ����
		o.offset = _GrabTexture_ST.xy * half2(1.0, 0.0) * (it)/1080;

		//��9.3���������յ�������
		return o;
	}

	//��9��������ɫ���� || Vertex Shader Function
	VertexOutput_Blur vert_BlurHorizontal_1(VertexInput v) {
		return vert_BlurHorizontal(v, _DownSampleValue);
	}

	VertexOutput_Blur vert_BlurHorizontal_2(VertexInput v) {
		return vert_BlurHorizontal(v, _DownSampleValue2);
	}
	VertexOutput_Blur vert_BlurHorizontal_3(VertexInput v) {
		return vert_BlurHorizontal(v, _DownSampleValue3);
	}
	//��10��������ɫ���� || Vertex Shader Function
	VertexOutput_Blur vert_BlurVertical(VertexInput v, float it)
	{
		//��10.1��ʵ����һ������ṹ
		VertexOutput_Blur o;

		//��10.2���������ṹ
		//����ά�ռ��е�����ͶӰ����ά����  
		o.pos = UnityObjectToClipPos(v.vertex);
		//��������
		o.uv = half4(v.texcoord.xy, 1, 1);
		//����Y�����ƫ����
		o.offset = _GrabTexture_ST.xy * half2(0.0, 1.0)    * ( it)/1080;

		//��10.3���������յ�������
		return o;
	}
	VertexOutput_Blur vert_BlurVertical_1(VertexInput v) {
		return vert_BlurVertical(v, _DownSampleValue);
	}

	VertexOutput_Blur vert_BlurVertical_2(VertexInput v) {
		return vert_BlurVertical(v, _DownSampleValue2);
	}
	VertexOutput_Blur vert_BlurVertical_3(VertexInput v) {
		return vert_BlurVertical(v, _DownSampleValue3);
	}
	//��11��Ƭ����ɫ���� || Fragment Shader Function
	half4 frag_Blur(VertexOutput_Blur i) : SV_Target
	{
		//��11.1����ȡԭʼ��uv����
		half2 uv = i.uv.xy;
		
			clip(tex2D(_MaskTex, uv).r-0.5);
		//��11.2����ȡƫ����
		half2 OffsetWidth = i.offset;
		//�����ĵ�ƫ��3�����������������Ͽ�ʼ��Ȩ�ۼ�
		half2 uv_withOffset = uv - OffsetWidth * 3.0;

		//��11.3��ѭ����ȡ��Ȩ�����ɫֵ
		half4 color = 0;
		for (int j = 0; j< 7; j++)
		{
			//ƫ�ƺ����������ֵ
			half4 texCol = tex2D(_GrabTexture, uv_withOffset);
			//�������ɫֵ+=ƫ�ƺ����������ֵ x ��˹Ȩ��
			color += texCol * GaussWeight[j];
			//�Ƶ���һ�����ش���׼����һ��ѭ����Ȩ
			uv_withOffset += OffsetWidth;
		}

		//��11.4���������յ���ɫֵ
		return color;
	}
	/*half4 frag_Blur2(VertexOutput_Blur i) : SV_Target{ return frag_Blur(i,_GrabTexture2); }
	half4 frag_Blur3(VertexOutput_Blur i) : SV_Target{ return frag_Blur(i,_GrabTexture3); }
	half4 frag_Blur4(VertexOutput_Blur i) : SV_Target{ return frag_Blur(i,_GrabTexture4); }
	half4 frag_Blur5(VertexOutput_Blur i) : SV_Target{ return frag_Blur(i,_GrabTexture5); }
	half4 frag_Blur6(VertexOutput_Blur i) : SV_Target{ return frag_Blur(i,_GrabTexture6); }
	half4 frag_Blur7(VertexOutput_Blur i) : SV_Target{ return frag_Blur(i,_GrabTexture7); }*/

		//-------------------����CG��ɫ������������  || End CG Programming Part------------------  			
		ENDCG

		FallBack Off
}

//	Properties
//	{
//		_BumpAmt("Distortion", range(0, 2)) = 1
//		_TintAmt("Tint Amount", Range(0,1)) = 0.1
//		_TintColor("Tint Color", Color) = (1, 1, 1, 1)
//		_MainTex("Tint Texture (RGB)", 2D) = "white" {}
//	_BumpMap("Normalmap", 2D) = "bump" {}
//	_BlurAmt("Blur", Range(0, 10)) = 1
//	}
//
//
//	
//		CGINCLUDE
//#include "UnityCG.cginc"
//
//		struct appData {
//		float4 vertex : POSITION;
//		float2 texcoord : TEXCOORD0;
//	};
//
//	struct v2f {
//		float4 vertex : SV_POSITION;
//		float2 texcoord : TEXCOORD0;
//		float4 uvgrab : TEXCOORD1;
//	};
//
//	float _BumpAmt;
//	float _TintAmt;
//	float _BlurAmt;
//	float4 _TintColor;
//	sampler2D _MainTex;
//	sampler2D _BumpMap;
//	sampler2D _GrabTexture;
//	float4 _GrabTexture_TexelSize;
// 
//	//https://en.wikipedia.org/wiki/Gaussian_blur
//	float blurWeight[49];
//
//	half4 blur(half4 col, sampler2D tex, float4 uvrgab,float bluramt) {
//		float2 offset = 1.0 / _ScreenParams.xy;
//		for (int i = -3; i <= 3; ++i) {
//			for (int j = -3; j <= 3; ++j) {
//				//col += tex2Dproj(tex, uvrgab + float4(_GrabTexture_TexelSize.x * i * _BlurAmt, _GrabTexture_TexelSize.y *j * _BlurAmt, 0.0f, 0.0f)) * blurWeight[j * 7 + i + 24];
//				col += tex2Dproj(tex, uvrgab + float4(offset.x * i * bluramt, offset.y * j * bluramt, 0.0f, 0.0f)) * blurWeight[j * 7 + i + 24];
//			}
//		}
//		return col;
//	}
//
//	v2f vert(appData v) {
//		v2f o;
//		o.vertex = UnityObjectToClipPos(v.vertex);
//		o.texcoord = v.texcoord;
//		o.uvgrab = ComputeGrabScreenPos(o.vertex);
//		return o;
//	}
//
//	half4 frag(v2f i) : COLOR{
//		half4 mainColor = tex2D(_MainTex, i.texcoord);
//		half2 distortion = (UnpackNormal(tex2D(_BumpMap, i.texcoord)).rg/5 ) * _BumpAmt;
//		half4 col = half4(0, 0, 0, 0);
//		float4 uvgrab = float4(i.uvgrab.x + distortion.x, i.uvgrab.y + distortion.y, i.uvgrab.z, i.uvgrab.w);
//		col = blur(col, _GrabTexture, uvgrab, _BlurAmt);
//		return lerp(col, col * mainColor, _TintAmt) * _TintColor*10;
//	}
//		half4 frag2(v2f i) : COLOR{
//		half4 mainColor = tex2D(_MainTex, i.texcoord);
//		half2 distortion = (UnpackNormal(tex2D(_BumpMap, i.texcoord)).rg / 5) * _BumpAmt;
//		half4 col = half4(0, 0, 0, 0);
//		distortion /= 2;
//		float4 uvgrab = float4(i.uvgrab.x + distortion.x, i.uvgrab.y + distortion.y, i.uvgrab.z, i.uvgrab.w);
//		col = blur(col, _GrabTexture, uvgrab, _BlurAmt/2);
//		return lerp(col, col * mainColor, _TintAmt) * _TintColor * 10;
//	}
//		ENDCG
//		SubShader
//	{
//		//Queue is Transparent so other objects will be rendered first
//		Tags{ "RenderType" = "Opaque" "Queue" = "Transparent" }
//			LOD 100
//
//		GrabPass {}
//
//	Pass
//	{
//
//		CGPROGRAM
//#pragma vertex vert
//#pragma fragment frag
//			ENDCG
//	}
//			GrabPass{}
//
//			Pass
//		{
//
//			CGPROGRAM
//#pragma vertex vert
//#pragma fragment frag2
//			ENDCG
//		}
//			GrabPass{}
//
//			Pass
//		{
//
//			CGPROGRAM
//#pragma vertex vert
//#pragma fragment frag
//			ENDCG
//		}
//			GrabPass{}
//
//			Pass
//		{
//
//			CGPROGRAM
//#pragma vertex vert
//#pragma fragment frag2
//			ENDCG
//		}
//	 
//	}
//
//}