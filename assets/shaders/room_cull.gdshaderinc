uniform vec3 window_quad[4];
uniform bool enable_cull = true;

vec3 triangle_intersect( in vec3 ro, in vec3 rd, in vec3 v0, in vec3 v1, in vec3 v2 ) {
    vec3 v1v0 = v1 - v0;
    vec3 v2v0 = v2 - v0;
    vec3 rov0 = ro - v0;
    vec3  n = cross( v1v0, v2v0 );
    vec3  q = cross( rov0, rd );
    float d = 1.0/dot( rd, n );
    float u = d*dot( -q, v2v0 );
    float v = d*dot(  q, v1v0 );
    float t = d*dot( -n, rov0 );
    if( u<0.0 || v<0.0 || (u+v)>1.0 ) t = -1.0;
    return vec3( t, u, v );
}

bool point_in_window(vec3 cam_pos, vec3 eye_dir) {
	return 
		triangle_intersect(cam_pos, eye_dir, window_quad[0], window_quad[1], window_quad[2]).x >= 0.0 ||
		triangle_intersect(cam_pos, eye_dir, window_quad[0], window_quad[2], window_quad[3]).x >= 0.0;
}