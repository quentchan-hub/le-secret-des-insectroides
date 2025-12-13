shader_type spatial;

render_mode cull_disabled;

uniform sampler2D texture_albedo : source_color;
uniform float alpha_cutoff : hint_range(0.0, 1.0) = 0.1;

// Equivalent du Modulate du Sprite3D
uniform vec4 modulate_color : source_color = vec4(1.0);

void fragment() {
    vec4 tex = texture(texture_albedo, UV) * modulate_color;

    if (tex.a < alpha_cutoff) {
        discard;
    }

    ALBEDO = tex.rgb;
    ALPHA = tex.a;

    NORMAL = vec3(0.0, 0.0, 1.0);
}

