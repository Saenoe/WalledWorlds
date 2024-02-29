shader_type spatial;
render_mode unshaded, shadows_disabled;

uniform float inner_dot_radius = 0.5;
uniform vec3 inner_dot_color : source_color = vec3(1.0);

uniform float outer_ring_width;
uniform float outer_ring_radius;

void vertex() {
	float aspect = VIEWPORT_SIZE.x / VIEWPORT_SIZE.y;
	POSITION = vec4(vec2(VERTEX.x / aspect, VERTEX.y), 0.0, 1.0);
}

void fragment() {
	float smoothing = 1.0 / VIEWPORT_SIZE.y * 5.0;
	
	float d = length(UV * 2.0 - 1.0);
	//if (d > inner_dot_radius) discard;
	
	float ring_inside = outer_ring_radius - outer_ring_width;
	float ring_outside = outer_ring_radius + outer_ring_width;
	
	float outer_ring =
		smoothstep(ring_inside - smoothing, ring_inside, d) *
		smoothstep(ring_outside, ring_outside - smoothing, d);
	
	float inner_dot = smoothstep(inner_dot_radius, inner_dot_radius - smoothing, d);

	float alpha = min(outer_ring + inner_dot, 1.0);

	if (alpha < 0.0001) discard;
	
	ALBEDO = vec3(1.0);
	ALPHA = alpha;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
