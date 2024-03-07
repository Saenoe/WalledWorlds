shader_type spatial;
render_mode unshaded;

uniform sampler2D albedo_tex : source_color, filter_nearest_mipmap_anisotropic;

#include "room_cull.gdshaderinc"

void vertex() {}


void fragment() {
	vec3 frag_dir = mat3(INV_VIEW_MATRIX) * -VIEW;
	
	vec4 sample = texture(albedo_tex, UV);

	if ((enable_cull && !point_in_window(CAMERA_POSITION_WORLD, frag_dir)) || sample.a < ALPHA_SCISSOR_THRESHOLD) discard;
	
	
	ALBEDO = sample.rgb * COLOR.rgb;
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
	
	
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
