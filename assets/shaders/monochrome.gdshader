shader_type spatial;
render_mode unshaded;

uniform sampler2D screen_tex : hint_screen_texture, repeat_disable, filter_nearest;
uniform sampler2D depth_tex : hint_depth_texture, repeat_disable, filter_nearest;

uniform float fade_distance = 0.0;
uniform float fade_smoothing = 0.4;

uniform bool ignore_sky = false;

void vertex() {
	POSITION = vec4(VERTEX.xy * 2.0, 0.0, 1.0);
}

void fragment() {
	vec4 sample = textureLod(screen_tex, SCREEN_UV, 0.0);
	float depth_sample = textureLod(depth_tex, SCREEN_UV, 0.0).r;
	
	vec4 pixel_pos = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, depth_sample, 1.0);
	pixel_pos.xyz /= pixel_pos.w;
	
	float pixel_dist = length(pixel_pos);
	
	float lum = dot(sample.rgb, vec3(0.299, 0.587, 0.114));
	
	float blend_factor = smoothstep(fade_distance + fade_smoothing, max(0, fade_distance - fade_smoothing), pixel_dist);
	blend_factor = mix(blend_factor, 1.0, step(0.99999, depth_sample) * float(ignore_sky));
	

	ALBEDO = mix(vec3(lum), sample.rgb, blend_factor);
}



//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
