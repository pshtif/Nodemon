Shader "Hidden/Nodemon/LineShaderZTest" 
{
SubShader 
{
	Blend SrcAlpha OneMinusSrcAlpha
    ZTest Less
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
        	float4 color : COLOR;
		};
		
		struct VertexOutput
		{
			float2 texcoord : TEXCOORD0;
			float4 vertex : POSITION;
			float4 color : COLOR;
		};

		VertexOutput vert (VertexInput IN)
		{
			VertexOutput o;
			o.vertex = UnityObjectToClipPos(IN.vertex);
			o.texcoord = IN.texcoord;
			o.color = IN.color;
			return o;
		}

		float4 frag (VertexOutput IN) : COLOR
		{
			return _LineColor * IN.color * tex2D(_LineTexture, IN.texcoord);
		}
		ENDCG
	}
}
}