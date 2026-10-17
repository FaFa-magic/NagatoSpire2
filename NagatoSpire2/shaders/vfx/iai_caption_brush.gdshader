shader_type canvas_item;
render_mode unshaded;

uniform float reveal : hint_range(0.0, 1.0) = 0.0;
uniform float glow : hint_range(0.0, 1.0) = 0.0;
uniform float dissolve : hint_range(0.0, 1.0) = 0.0;
varying vec4 brush_tint;

void vertex() {
    brush_tint = COLOR;
}

float ink_grain(vec2 cell) {
    return fract(sin(dot(cell, vec2(127.1, 311.7))) * 43758.5453);
}

float ink_noise(vec2 point) {
    vec2 cell = floor(point);
    vec2 blend = fract(point);
    blend = blend * blend * (3.0 - 2.0 * blend);
    return mix(mix(ink_grain(cell), ink_grain(cell + vec2(1.0, 0.0)), blend.x),
        mix(ink_grain(cell + vec2(0.0, 1.0)), ink_grain(cell + vec2(1.0)), blend.x), blend.y);
}

void fragment() {
    vec4 ink = texture(TEXTURE, UV);
    float front = mix(-0.06, 1.06, reveal);
    float bristles = (sin(UV.x * 51.0) + sin(UV.x * 97.0 + 1.7)) * 0.009;
    float mask = 1.0 - smoothstep(front + bristles - 0.012, front + bristles + 0.012, UV.y);
    // One shared brush front for the complete two-column artwork, not two timed wipes.
    // Dissolve into fine irregular ink fragments only after the complete verse has glowed.
    float grain = mix(ink_noise(UV * vec2(24.0, 55.0)),
        ink_noise(UV * vec2(73.0, 168.0)), 0.25);
    float threshold = mix(-0.08, 1.08, dissolve);
    float remaining = smoothstep(threshold - 0.06, threshold + 0.06, grain);
    float edge = remaining * (1.0 - smoothstep(threshold + 0.06, threshold + 0.17, grain));
    edge *= smoothstep(0.0, 0.12, dissolve) * (1.0 - smoothstep(0.85, 1.0, dissolve));

    vec2 radius = TEXTURE_PIXEL_SIZE * 7.0;
    float halo = 0.0;
    halo += texture(TEXTURE, UV + vec2(radius.x, 0.0)).a;
    halo += texture(TEXTURE, UV - vec2(radius.x, 0.0)).a;
    halo += texture(TEXTURE, UV + vec2(0.0, radius.y)).a;
    halo += texture(TEXTURE, UV - vec2(0.0, radius.y)).a;
    halo += texture(TEXTURE, UV + radius * 0.707).a;
    halo += texture(TEXTURE, UV - radius * 0.707).a;
    halo += texture(TEXTURE, UV + radius * vec2(-0.707, 0.707)).a;
    halo += texture(TEXTURE, UV + radius * vec2(0.707, -0.707)).a;
    halo = max(0.0, halo * 0.125 - ink.a) * glow * 0.32;
    vec3 color = mix(ink.rgb, vec3(1.0, 0.94, 0.97), glow * 0.20 + edge * 0.48);
    color = mix(color, vec3(1.0, 0.60, 0.78), halo / max(ink.a + halo, 0.001));
    COLOR = vec4(color * brush_tint.rgb,
        (ink.a + halo) * brush_tint.a * mask * remaining * pow(1.0 - dissolve, 0.30));
}
