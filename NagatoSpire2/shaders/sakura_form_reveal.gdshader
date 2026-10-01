shader_type canvas_item;

uniform float reveal : hint_range(0.0, 1.0) = 0.0;

float ink_noise(vec2 point) {
	vec2 cell = floor(point);
	vec2 blend = fract(point);
	blend = blend * blend * (3.0 - 2.0 * blend);
	float a = fract(sin(dot(cell, vec2(127.1, 311.7))) * 43758.5453);
	float b = fract(sin(dot(cell + vec2(1.0, 0.0), vec2(127.1, 311.7))) * 43758.5453);
	float c = fract(sin(dot(cell + vec2(0.0, 1.0), vec2(127.1, 311.7))) * 43758.5453);
	float d = fract(sin(dot(cell + vec2(1.0, 1.0), vec2(127.1, 311.7))) * 43758.5453);
	return mix(mix(a, b, blend.x), mix(c, d, blend.x), blend.y);
}

void fragment() {
	vec4 painted = COLOR;
	float broad = ink_noise(UV * vec2(7.0, 4.0));
	float fine = ink_noise(UV * vec2(31.0, 18.0));
	float radius = distance(UV, vec2(0.5));
	float ink = broad * 0.52 + fine * 0.18 + radius * 0.42;
	float threshold = mix(-0.15, 1.12, reveal);
	float wash = 1.0 - smoothstep(threshold - 0.13, threshold + 0.13, ink);
	painted.a *= wash * smoothstep(0.0, 0.16, reveal);
	COLOR = painted;
}
