shader_type spatial;
render_mode unshaded;

uniform sampler2D screen_tex : hint_screen_texture, filter_nearest;
uniform sampler2D depth_tex : hint_depth_texture, filter_nearest;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 screen_sample = textureLod(screen_tex, SCREEN_UV, 0);
	float depth_sample = textureLod(depth_tex, SCREEN_UV, 0).r;
	depth_sample = depth_sample * 0.5 + 0.5;
	
	ALBEDO = vec3(depth_sample);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
