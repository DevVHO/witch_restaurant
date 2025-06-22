shader_type spatial;

render_mode blend_mix, depth_draw_opaque, cull_back, unshaded;
uniform float alpha_value : hint_range(0.0, 1.0) = 0.5;


uniform vec4 base_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform vec4 border_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float border_thickness = 0.05;
uniform vec3 tile_size = vec3(1.0, 0.165, 1.0); // X = largura, Y = altura, Z = profundidade

varying vec3 local_pos;

void vertex() {
    local_pos = VERTEX;
}

void fragment() {
    // Centraliza coordenadas em (0,0) no plano XZ
    vec3 centered = local_pos + tile_size / 2.0;

    bool is_border = centered.x < border_thickness || centered.x > tile_size.x - border_thickness ||
                     centered.z < border_thickness || centered.z > tile_size.z - border_thickness;

    ALBEDO = is_border ? border_color.rgb : base_color.rgb;
    ALPHA = alpha_value;
}
