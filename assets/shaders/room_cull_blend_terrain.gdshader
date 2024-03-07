shader_type spatial;

#include "room_cull.gdshaderinc"

uniform sampler2D blend_tex0 : source_color, filter_nearest_mipmap_anisotropic;
uniform float blend_tex0_tile = 1.0;
uniform sampler2D blend_tex1 : source_color, filter_nearest_mipmap_anisotropic;
uniform float blend_tex1_tile = 1.0;


void fragment() {
	vec3 frag_dir = mat3(INV_VIEW_MATRIX) * -VIEW;

	if (enable_cull && !point_in_window(CAMERA_POSITION_WORLD, frag_dir)) discard;
	
	
	float blend_value = COLOR.r;
	
	vec4 blend_sample0 = texture(blend_tex0, UV * blend_tex0_tile);
	vec4 blend_sample1 = texture(blend_tex1, UV * blend_tex1_tile);
	
	vec4 blend_color = mix(blend_sample0, blend_sample1, blend_value);
	
	ALBEDO = blend_color.rgb;
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
