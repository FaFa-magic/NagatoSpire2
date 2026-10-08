shader_type canvas_item;
render_mode unshaded, blend_add;

// The official flipbook stores its intensity in red; recolor only this instance.
// Host textures, materials and every other card's slash remain unchanged.
uniform vec4 edge_tint : source_color = vec4(1.0, 0.63, 0.78, 1.0);
uniform float brightness = 1.65;
varying vec4 vertex_tint;

void vertex() {
    vertex_tint = COLOR;
}

void fragment() {
    vec4 sample_color = texture(TEXTURE, UV);
    vec3 moon_color = mix(edge_tint.rgb, vec3(1.0, 0.98, 0.92), smoothstep(0.18, 0.85, sample_color.r));
    COLOR = vec4(moon_color * brightness * vertex_tint.rgb, sample_color.a * vertex_tint.a);
}
