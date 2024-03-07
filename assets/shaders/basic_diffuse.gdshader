shader_type spatial;
render_mode unshaded;

uniform sampler2D albedo_tex : source_color, filter_nearest_mipmap_anisotropic;

void vertex() {}

void fragment() {
	vec4 sample = texture(albedo_tex, UV);
	
	ALBEDO = sample.rgb * COLOR.rgb;
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
