Shader "Hidden/Nodemon/PointShader" 
{
SubShader 
{
	Blend SrcAlpha OneMinusSrcAlpha 
	ZWrite Off 
	Cull Off 
	Fog { Mode Off }
	
	Pass 
	{  
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		uniform float4 _PointColor;
		uniform sampler2D _PointTexture;

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
			return _PointColor * tex2D(_PointTexture, IN.texcoord);
		}
		ENDCG
	}
}
}