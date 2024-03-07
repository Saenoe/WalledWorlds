shader_type spatial;
render_mode unshaded;

#include "room_cull.gdshaderinc"

uniform sampler2D window_mask_tex : filter_nearest_mipmap_anisotropic;
uniform sampler2D noise_tex : filter_nearest_mipmap_anisotropic;

uniform vec2 scale = vec2(1.0);

uniform vec3 border_color : source_color = vec3(1.0);
uniform vec3 window_color : source_color = vec3(0.05);
uniform vec3 glow_color : source_color = vec3(1.0);
uniform vec3 sky_color : source_color= vec3(1.0);

varying vec3 local_vertex;

// https://www.shadertoy.com/view/WttXWX
uint ihash(uint x) {
    x ^= x >> 16u;
    x *= 0x7FEb352Du;
    x ^= x >> 15u;
    x *= 0x846CA68Bu;
    x ^= x >> 16u;
    return x;
}
float hash(uint x) {
	return float(ihash(x)) / float(0xFFFFFFFFu);
}
float hash3d(uvec3 x) {
	return hash(x.x + ihash(x.y + ihash(x.z)));
}
float hash2d(uvec2 x) {
	return hash(x.x + ihash(x.y));
}

void vertex() {
	local_vertex = VERTEX;
}

void fragment() {
	
	vec3 frag_dir = mat3(INV_VIEW_MATRIX) * -VIEW;

	if (enable_cull && !point_in_window(CAMERA_POSITION_WORLD, frag_dir)) discard;
	
	
	vec2 uv = UV * scale;
	
	float window_mask = texture(window_mask_tex, uv).r;
	float noise_value = texture(noise_tex, UV).r;
	//float noise_value = hash2d(uvec2(UV * scale));
	//float glow_value = smoothstep(glow_threshold - 0.01, glow_threshold, noise_value);
	
	float glow_value = noise_value;//step(glow_threshold, noise_value);
	
	ALBEDO = mix(
		mix(
			mix(border_color, window_color, window_mask),
			glow_color * glow_value,
			window_mask
		),
		sky_color,
		COLOR.r
	);
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
	
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
