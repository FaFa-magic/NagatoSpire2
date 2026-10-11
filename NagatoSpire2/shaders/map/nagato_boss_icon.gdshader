shader_type canvas_item;

uniform vec4 paper_color : source_color = vec4(0.655, 0.541, 0.404, 1.0);

void fragment() {
    vec4 tint = COLOR;
    vec4 tex = texture(TEXTURE, UV);
    float luminance = dot(tex.rgb, vec3(0.299, 0.587, 0.114));
    COLOR = vec4(mix(tint.rgb, paper_color.rgb, luminance), tex.a * tint.a);
}
