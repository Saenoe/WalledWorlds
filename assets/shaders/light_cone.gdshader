shader_type spatial;
render_mode unshaded, cull_disabled;

uniform float alpha = 0.5;
uniform vec3 color : source_color = vec3(1.0);

void fragment() {
	float ass = max(0.0, dot(VIEW, NORMAL));
	
	float h = FRAGCOORD.z;
	
	ALBEDO = color;
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
	ALPHA = ass * alpha * FRAGCOORD.z * COLOR.r;
}
