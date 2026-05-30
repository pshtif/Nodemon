Shader "Hidden/Nodemon/PointShader"
{
SubShader
{
	// Overlay queue so dots draw after every other Geometry/Transparent
	// material in the frame — debug overlay should sit on top regardless
	// of when its draw call was issued from C#.
	Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Overlay" }
	Blend SrcAlpha OneMinusSrcAlpha
    // Always over Off — same effect on most platforms but some treat Off
    // as "don't change state" rather than "always pass," and the editor
    // SceneView depth state can leak in those cases.
    ZTest Always
	ZWrite Off
	Cull Off
	Fog { Mode Off }

	Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_instancing
		#include "UnityCG.cginc"

		uniform sampler2D _PointTexture;

		// _PointColor is per-instance so callers can pass an array via
		// MaterialPropertyBlock.SetVectorArray and tint each point individually
		// (e.g. from a per-point Cd attribute). When the property block has a
		// single value instead of an array, Unity broadcasts it to all instances
		// — old single-color usage still works unchanged.
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _PointColor)
		UNITY_INSTANCING_BUFFER_END(Props)

		struct VertexInput
		{
			float2 texcoord : TEXCOORD0;
			float4 vertex : POSITION;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct VertexOutput
		{
			float2 texcoord : TEXCOORD0;
			float4 vertex : POSITION;
			float4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		VertexOutput vert (VertexInput IN)
		{
			UNITY_SETUP_INSTANCE_ID(IN);
			VertexOutput o;
			UNITY_TRANSFER_INSTANCE_ID(IN, o);
			o.vertex = UnityObjectToClipPos(IN.vertex);
			o.texcoord = IN.texcoord;
			o.color = UNITY_ACCESS_INSTANCED_PROP(Props, _PointColor);
			return o;
		}

		float4 frag (VertexOutput IN) : COLOR
		{
			UNITY_SETUP_INSTANCE_ID(IN);
			return IN.color * tex2D(_PointTexture, IN.texcoord);
		}
		ENDCG
	}
}
}
