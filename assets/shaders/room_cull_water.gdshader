shader_type spatial;
render_mode unshaded;

#include "room_cull.gdshaderinc"

uniform sampler2D mask_tex : source_color, filter_nearest_mipmap_anisotropic;
uniform sampler3D wave_noise;
uniform vec4 wave_scale = vec4(0.0);
uniform float height_offset = 0.0f;

uniform vec3 color : source_color = vec3(1.0);
uniform vec2 scroll = vec2(0.0);

void vertex() {
	VERTEX += (texture(wave_noise, vec3(UV * wave_scale.xy, TIME * wave_scale.w)).xyz * 2.0 - 1.0) * wave_scale.z - height_offset;
}

void fragment() {
	vec2 uv = UV + scroll * TIME;
	
	vec3 frag_dir = mat3(INV_VIEW_MATRIX) * -VIEW;

	vec4 sample = texture(mask_tex, uv);

	if ((enable_cull && !point_in_window(CAMERA_POSITION_WORLD, frag_dir)) || sample.a < 0.5) discard;
	
	
	ALBEDO = sample.rgb * color * COLOR.rgb;
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
