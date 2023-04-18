Shader "Hidden/Nodemon/LineShader" 
{
SubShader 
{
	Blend SrcAlpha OneMinusSrcAlpha
    ZTest Off 
	ZWrite Off 
	Cull Off 
	Fog { Mode Off }
	
	Pass 
	{  
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		uniform float4 _LineColor;
		uniform sampler2D _LineTexture;

        struct VertexInput
		{
			float2 texcoord : TEXCOORD0;
			float4 vertex : POSITION;
		};
		
		struct VertexOutput
		{
			float2 texcoord : TEXCOORD0;
			float4 vertex : POSITION;
		};

		VertexOutput vert (VertexInput IN)
		{
			VertexOutput o;
			o.vertex = UnityObjectToClipPos(IN.vertex);
			o.texcoord = IN.texcoord;
			return o;
		}

		float4 frag (VertexOutput IN) : COLOR
		{
			return _LineColor * tex2D(_LineTexture, IN.texcoord);
		}
		ENDCG
	}
}
}