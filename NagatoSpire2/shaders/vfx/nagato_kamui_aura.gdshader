shader_type canvas_item;
render_mode unshaded;

uniform bool flames = false;

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash(i), hash(i + vec2(1.0, 0.0)), u.x),
               mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0)), u.x), u.y);
}

float turbulence(vec2 p) {
    return noise(p) * 0.57 + noise(p * 2.1 + 5.3) * 0.28 + noise(p * 4.3 - 8.1) * 0.15;
}

void fragment() {
    vec2 p = vec2((UV.x - 0.5) * 2.0, 1.0 - UV.y);
    float t = TIME;
    float n = turbulence(vec2(p.x * 4.2, p.y * 5.2 - t * 0.7));
    float edge = smoothstep(1.0, 0.77, abs(p.x)) * smoothstep(0.0, 0.08, p.y);
    if (flames) {
        float side = smoothstep(0.18, 0.56, abs(p.x));
        float flicker = turbulence(vec2(p.x * 11.0, p.y * 7.0 - t * 1.6));
        float height = 0.08 + 0.28 * side + 0.19 * flicker;
        float flame = smoothstep(height + 0.06, height - 0.02, p.y);
        flame *= smoothstep(0.42, 0.84, flicker + (0.58 - p.y) * 0.38);
        flame *= mix(0.38, 1.0, side) * smoothstep(0.86, 0.5, abs(p.x));
        float heat = clamp((height - p.y) * 2.8 + flicker * 0.24, 0.0, 1.0);
        vec3 color = mix(vec3(0.32, 0.015, 0.008), vec3(1.0, 0.24, 0.025), heat);
        color = mix(color, vec3(1.0, 0.72, 0.24), heat * heat * 0.65);
        COLOR = vec4(color, flame * edge * 0.83);
    } else {
        float silhouette = smoothstep(1.1, 0.58, length(vec2(p.x, (p.y - 0.36) * 1.35)));
        float hollow = mix(0.32, 1.0, smoothstep(0.12, 0.6, abs(p.x)));
        float smoke = smoothstep(0.27, 0.75, n) * silhouette * hollow * edge;
        vec3 color = mix(vec3(0.014, 0.009, 0.019), vec3(0.085, 0.023, 0.036), n);
        COLOR = vec4(color, smoke * 0.78);
    }
}
