shader_type spatial;
render_mode unshaded;

uniform sampler2D mask_tex : source_color, filter_nearest_mipmap_anisotropic;
uniform vec3 color : source_color = vec3(1.0);
uniform vec2 scroll = vec2(0.0);

void vertex() {}

void fragment() {
	vec2 uv = UV + scroll * TIME;
	
	vec4 sample = texture(mask_tex, uv);
	
	if (sample.a < 0.5) discard;
	
	ALBEDO = sample.rgb * color * COLOR.rgb;
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
