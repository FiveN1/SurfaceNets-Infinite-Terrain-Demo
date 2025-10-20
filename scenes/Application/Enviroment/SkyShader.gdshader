shader_type sky;
render_mode use_half_res_pass;

void sky() {
	// Called for every visible pixel in the sky background, as well as all pixels
	// in the radiance cubemap.

	COLOR = vec3(0.0f, 0.0f, 0.0f);
	ALPHA = 1.0f;
}
