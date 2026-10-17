shader_type canvas_item;
render_mode unshaded;

uniform sampler2D next_pose : source_color, filter_linear;
uniform float pose_blend : hint_range(0.0, 1.0) = 0.0;
varying vec4 pose_tint;

void vertex() {
    pose_tint = COLOR;
}

void fragment() {
    vec4 a = texture(TEXTURE, UV);
    vec4 b = texture(next_pose, UV);
    float alpha = mix(a.a, b.a, pose_blend);
    vec3 premultiplied = mix(a.rgb * a.a, b.rgb * b.a, pose_blend);
    COLOR = vec4(premultiplied / max(alpha, 0.00001) * pose_tint.rgb, alpha * pose_tint.a);
}
