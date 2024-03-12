shader_type spatial;
render_mode unshaded;

uniform float alpha = 0.5;

void vertex() {
	
}

void fragment() {
	ALBEDO = COLOR.rgb;
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
	ALPHA = alpha;
	
}
