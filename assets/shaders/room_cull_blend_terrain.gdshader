shader_type spatial;
render_mode unshaded;

#include "room_cull.gdshaderinc"

uniform sampler2D blend_tex0 : source_color, filter_nearest_mipmap_anisotropic;
uniform sampler2D blend_tex1 : source_color, filter_nearest_mipmap_anisotropic;

uniform float normal_blend_angle = 0.9;
uniform float normal_blend_smooth = 0.1;

varying float normal_blend_factor;

void vertex() {
	normal_blend_factor = smoothstep(
		normal_blend_angle + normal_blend_smooth,
		normal_blend_angle - normal_blend_smooth,
		dot(NORMAL, vec3(0, 1, 0))
	);
}

void fragment() {
	
	vec3 frag_dir = mat3(INV_VIEW_MATRIX) * -VIEW;

	if (enable_cull && !point_in_window(CAMERA_POSITION_WORLD, frag_dir)) discard;
	
	float blend_value = COLOR.r;
	
	vec4 blend_sample0 = texture(blend_tex0, UV);
	vec4 blend_sample1 = texture(blend_tex1, UV);
	
	blend_value = clamp(blend_value + normal_blend_factor, 0, 1);
	
	vec4 blend_color = mix(blend_sample0, blend_sample1, blend_value);
	
	ALBEDO = blend_color.rgb * COLOR.g;
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
}