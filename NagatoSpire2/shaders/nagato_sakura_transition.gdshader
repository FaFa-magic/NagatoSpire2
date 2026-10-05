shader_type canvas_item;
render_mode unshaded;

// The official NTransition owns this timeline, cancellation and scene swap.
// threshold=0 is transparent; threshold=1 must be opaque black.
uniform sampler2D background_tex : source_color, filter_linear, repeat_disable;
uniform float threshold : hint_range(0.0, 1.0) = 0.0;
uniform float petal_softness : hint_range(0.08, 0.45) = 0.23;

const float BACKGROUND_ASPECT = 1920.0 / 1080.0;
// Match NagatoTransitionPatch.MinimumDuration and the preview AnimationPlayer.
// 1.8s ink entrance + 0.3s portrait hold + 3.3s petal sweep/black fade.
const float INK_ENTRANCE_END = 1.8 / 5.4;
const float PORTRAIT_HOLD_END = 2.1 / 5.4;

float hash21(vec2 p) {
	p = fract(p * vec2(123.34, 456.21));
	p += dot(p, p + 45.32);
	return fract(p.x * p.y);
}

float ink_noise(vec2 p) {
	vec2 cell = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	return mix(mix(hash21(cell), hash21(cell + vec2(1.0, 0.0)), f.x),
		mix(hash21(cell + vec2(0.0, 1.0)), hash21(cell + vec2(1.0)), f.x), f.y);
}

vec2 ink_reveal(vec2 screen, float phase, float aspect) {
	// A stable multi-scale field makes ink diffuse outward rather than flicker.
	// Begin near Nagato, with irregular wet brush edges and fine paper fibers.
	vec2 p = (screen - vec2(0.66, 0.42)) * vec2(aspect, 1.0);
	float wash = ink_noise(p * 4.5 + vec2(7.1, 3.8));
	float fibers = ink_noise(p * 19.0 + vec2(2.8, 11.2));
	float grain = ink_noise(p * 75.0);
	vec2 distortion = vec2(wash - 0.5, ink_noise(p * 5.2 + vec2(15.3)) - 0.5);
	float distance = length((p + distortion * 0.18) * vec2(0.82, 1.08))
		/ max(aspect * 0.65, 0.82);
	float field = distance + wash * 0.22 + fibers * 0.075 + grain * 0.015;
	// Linear diffusion avoids the old mid-entrance acceleration/full-screen pop.
	float spread = mix(-0.10, 1.28, phase);
	float feather = mix(0.045, 0.085, wash);
	float reveal = 1.0 - smoothstep(spread - feather, spread + feather, field);
	// Exact transparent/full-image endpoints within the 1.8-second entrance.
	reveal *= smoothstep(0.0, 0.07, phase);
	reveal = mix(reveal, 1.0, smoothstep(0.82, 1.0, phase));
	float wet_edge = (1.0 - smoothstep(0.0, feather * 2.1, abs(field - spread)))
		* (1.0 - smoothstep(0.66, 1.0, phase));
	return vec2(reveal, wet_edge);
}

mat2 rotation(float angle) {
	float s = sin(angle);
	float c = cos(angle);
	return mat2(vec2(c, -s), vec2(s, c));
}

float petal_mask(vec2 p, float feather) {
	// Rounded, gently notched sakura petal, not a sharply focused flower.
	float width = max(0.30, 0.70 - p.y * 0.28);
	float body = length(vec2(p.x / width, p.y)) - 0.88;
	float notch = 0.18 - length(p - vec2(0.0, -0.78));
	float distance = max(body, notch);
	return 1.0 - smoothstep(-feather, feather, distance);
}

// A moving seeded lattice avoids both hundreds of sprites and popping respawns.
// Coordinates are in viewport-height units so petals stay round at any aspect.
vec4 petal_layer(vec2 screen, float t, float layer, float aspect) {
	float spacing = mix(0.22, 0.34, layer / 2.0);
	float motion = max(t - 0.12, 0.0);
	vec2 drift = vec2(1.10, 0.92) * motion * (0.85 + layer * 0.38);
	vec2 field = screen * vec2(aspect, 1.0) - drift;
	vec2 cell = floor(field / spacing);
	float density = mix(0.06, 1.0, smoothstep(0.17, 0.67, t));
	float front = mix(-0.16, 1.22, smoothstep(0.13, 0.78, t));
	float diagonal = (screen.x + screen.y) * 0.5;
	float arrival = smoothstep(diagonal - 0.16, diagonal + 0.12, front);
	float appear = smoothstep(0.13, 0.24, t) * arrival;
	float cluster = smoothstep(0.54, 0.80, t);
	vec4 result = vec4(0.0);
	for (int y = -1; y <= 1; y++) {
		for (int x = -1; x <= 1; x++) {
			vec2 id = cell + vec2(float(x), float(y));
			float seed = hash21(id + vec2(layer * 27.1, 9.2));
			float variation = hash21(id + vec2(41.7, layer * 13.3));
			vec2 jitter = vec2(seed, variation) * 0.48 + vec2(0.26);
			vec2 center = (id + jitter) * spacing;
			center += vec2(sin(t * 9.0 + seed * 20.0), cos(t * 7.0 + variation * 23.0)) * 0.013;
			float radius = spacing * mix(0.21, 0.36, variation) * mix(1.0, 2.15, cluster);
			vec2 local = rotation(seed * 6.28318 + t * (2.0 + variation * 2.0)) * (field - center) / radius;
			float active = smoothstep(seed - 0.07, seed + 0.07, density);
			float alpha = petal_mask(local, petal_softness + layer * 0.04) * active * appear * 0.94;
			vec3 pink = mix(vec3(1.0, 0.76, 0.84), vec3(1.0, 0.91, 0.93), variation);
			pink *= 1.0 - 0.055 * clamp(local.y, -1.0, 1.0);
			result.rgb = pink * alpha + result.rgb * (1.0 - alpha);
			result.a = alpha + result.a * (1.0 - alpha);
		}
	}
	return result;
}

void fragment() {
	float progress = clamp(threshold, 0.0, 1.0);
	// Remap only the opening/hold; retain the established petal animation and
	// fade sequence instead of slowing every stage equally.
	float t = progress < INK_ENTRANCE_END
		? progress * (0.15 / INK_ENTRANCE_END)
		: (progress < PORTRAIT_HOLD_END ? 0.15
			: mix(0.15, 1.0, (progress - PORTRAIT_HOLD_END) / (1.0 - PORTRAIT_HOLD_END)));
	float aspect = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
	// Official transition rectangle extends beyond the viewport. Never use UV
	// for framing: SCREEN_UV keeps this image fitted to the actual screen.
	vec2 image_uv = SCREEN_UV;
	if (aspect > BACKGROUND_ASPECT) {
		image_uv.y = (image_uv.y - 0.5) * BACKGROUND_ASPECT / aspect + 0.5;
	} else {
		image_uv.x = (image_uv.x - 0.5) * aspect / BACKGROUND_ASPECT + 0.5;
	}
	vec3 scene = texture(background_tex, image_uv).rgb;
	float appear = 1.0;
	if (t < 0.15) {
		float phase = t / 0.15;
		vec2 ink = ink_reveal(SCREEN_UV, phase, aspect);
		appear = ink.x;
		// A restrained monochrome wet rim evokes sumi-e; the original colors
		// return completely before petals take over. Do not restyle the image.
		float grey = dot(scene, vec3(0.299, 0.587, 0.114));
		scene = mix(scene, vec3(grey), ink.y * 0.42);
		scene *= 1.0 - ink.y * 0.30;
	}
	// The out-of-focus rear petals become a continuous blanket underneath the
	// moving foreground petals, guaranteeing full coverage without flattening
	// the final frame into a plain pink rectangle.
	float cover = smoothstep(0.72, 0.81, t);
	float cloud = sin(SCREEN_UV.x * 13.0 + SCREEN_UV.y * 11.0 - t * 7.0) * 0.5 + 0.5;
	vec3 blanket = mix(vec3(0.98, 0.75, 0.83), vec3(1.0, 0.91, 0.94), cloud);
	scene = mix(scene, blanket, cover);
	for (int i = 0; i < 3; i++) {
		vec4 petals = petal_layer(SCREEN_UV, t, float(i), aspect);
		scene = petals.rgb + scene * (1.0 - petals.a);
	}
	float fade_to_black = smoothstep(0.85, 1.0, t);
	COLOR = vec4(scene * (1.0 - fade_to_black), appear);
}
