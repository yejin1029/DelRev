// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SuburbanHouse/MaskedColorChange"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Normal("Normal", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Basecolor("Basecolor", 2D) = "white" {}
		_MetallicSmoothness("Metallic-Smoothness", 2D) = "white" {}
		_AO("AO", 2D) = "white" {}
		_Normal_Strength("Normal_Strength", Range( 0 , 5)) = 1
		_Color("Color", Color) = (1,1,1,0)
		[Toggle(_ACTIVATEMASKING_ON)] _ActivateMasking("ActivateMasking?", Float) = 0
		[Toggle(_MASKED_ON)] _Masked("Masked?", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma shader_feature _ACTIVATEMASKING_ON
		#pragma shader_feature _MASKED_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _Normal_Strength;
		uniform sampler2D _Normal;
		uniform sampler2D _Basecolor;
		uniform float4 _Color;
		uniform sampler2D _Mask;
		uniform sampler2D _MetallicSmoothness;
		uniform sampler2D _AO;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (1.0).xx;
			float2 uv_TexCoord8 = i.uv_texcoord * temp_cast_0 + float2( 0,0 );
			o.Normal = UnpackScaleNormal( tex2D( _Normal, uv_TexCoord8 ) ,_Normal_Strength );
			float4 tex2DNode1 = tex2D( _Basecolor, uv_TexCoord8 );
			float4 lerpResult6 = lerp( tex2DNode1 , _Color , tex2D( _Mask, uv_TexCoord8 ).r);
			#ifdef _MASKED_ON
				float4 staticSwitch11 = lerpResult6;
			#else
				float4 staticSwitch11 = _Color;
			#endif
			#ifdef _ACTIVATEMASKING_ON
				float4 staticSwitch13 = staticSwitch11;
			#else
				float4 staticSwitch13 = tex2DNode1;
			#endif
			o.Albedo = staticSwitch13.rgb;
			float4 tex2DNode2 = tex2D( _MetallicSmoothness, uv_TexCoord8 );
			o.Metallic = tex2DNode2.r;
			o.Smoothness = tex2DNode2.a;
			o.Occlusion = tex2D( _AO, uv_TexCoord8 ).r;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15001
7;29;1522;788;715.2103;804.7647;1.3;True;True
Node;AmplifyShaderEditor.RangedFloatNode;9;-1321.854,-382.6935;Float;False;Constant;_Tiling;Tiling;6;0;Create;True;0;0;False;0;1;0;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-994.3943,-405.4923;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-516,-639;Float;True;Property;_Basecolor;Basecolor;3;0;Create;True;0;0;False;0;None;bbd6d84d3fe83b84897559f260e4a75c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;5;-461.3943,-859.4923;Float;False;Property;_Color;Color;7;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-512.3943,-436.4923;Float;True;Property;_Mask;Mask;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;-112.5944,-610.692;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-811.0079,-166.26;Float;False;Property;_Normal_Strength;Normal_Strength;6;0;Create;True;0;0;False;0;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;11;140.3907,-685.5815;Float;False;Property;_Masked;Masked?;9;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-509.608,-212.56;Float;True;Property;_Normal;Normal;1;0;Create;True;0;0;False;0;None;16165b6052798ee47a70f89b448fb85f;True;0;True;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-492.5515,221.2385;Float;True;Property;_AO;AO;5;0;Create;True;0;0;False;0;None;a6691e46740cff54f98906fd673143b4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;13;412.9119,-523.6711;Float;False;Property;_ActivateMasking;ActivateMasking?;8;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-512.008,10.93998;Float;True;Property;_MetallicSmoothness;Metallic-Smoothness;4;0;Create;True;0;0;False;0;None;28e1922607944fc45a71496639debea2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;694.4825,-188.6902;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;SuburbanHouse/MaskedColorChange;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;0;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;0;0;False;0;0;0;False;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;8;0;9;0
WireConnection;1;1;8;0
WireConnection;7;1;8;0
WireConnection;6;0;1;0
WireConnection;6;1;5;0
WireConnection;6;2;7;1
WireConnection;11;1;5;0
WireConnection;11;0;6;0
WireConnection;3;1;8;0
WireConnection;3;5;4;0
WireConnection;10;1;8;0
WireConnection;13;1;1;0
WireConnection;13;0;11;0
WireConnection;2;1;8;0
WireConnection;0;0;13;0
WireConnection;0;1;3;0
WireConnection;0;3;2;1
WireConnection;0;4;2;4
WireConnection;0;5;10;1
WireConnection;0;10;1;4
ASEEND*/
//CHKSM=9F6EA0093B88437357201159CDA2DA2D2B0CC0B4