shader_type spatial;
render_mode blend_mix, cull_disabled, depth_draw_never;

uniform bool is_underwater = false;

// --- Controles del modo submarino ---
uniform float under_alpha      : hint_range(0.0, 1.0) = 1.0;        // opacidad (1 = sólido)
uniform vec3  under_color      : source_color = vec3(0.02, 0.11, 0.16); // color base
uniform float under_darkness   : hint_range(0.0, 2.0) = 0.1;        // multiplica el color (más bajo = más “denso”)
uniform float under_roughness  : hint_range(0.0, 1.0) = 1.0;        // 1 = sin brillos
uniform float under_fog_density: hint_range(0.0, 5.0) = 5.0;        // niebla interna (0 = desactivada)


uniform vec3 albedo : source_color;
uniform vec3 albedo2 : source_color;
uniform float metallic : hint_range(0.0, 1.0) = 0.0;
uniform float roughness : hint_range (0.0, 1.0) = 0.02;

uniform sampler2D wave;
uniform sampler2D texture_normal;
uniform sampler2D texture_normal2;
uniform sampler2D depth_tex : hint_depth_texture, filter_linear;
uniform sampler2D screen_tex : hint_screen_texture, filter_linear_mipmap;

uniform vec2 wave_direction  = vec2(2.0, 0.0);
uniform vec2 wave_direction2 = vec2(0.0, 1.0);
uniform float time_scale : hint_range(0.0, 0.01, 0.005) = 0.015;
uniform float noise_scale = 2.0;
uniform float height_scale = 1.1;

uniform vec4 color_deep : source_color;
uniform vec4 color_shallow : source_color;
uniform float beers_law = 2.0;
uniform float depth_offset = -75.0;

uniform float edge_scale = 0.1;
uniform float near = 1.0;
uniform float far  = 100.0;
uniform vec3 edge_color : source_color;

// waving variables
varying float height;
varying vec3 world_pos;

// Fresnel (renombrado para no chocar con variable)
float fresnel_fn(float amount, vec3 normal, vec3 view_dir) {
    return pow(1.0 - clamp(dot(normalize(normal), normalize(view_dir)), 0.0, 1.0), amount);
}

// Tu helper de depth para edges
float edge(float depth) {
    depth = 1.0 - 2.0 * depth;
    return near * far / (far + depth * (near - far));
}

void vertex() {
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    height = texture(wave, world_pos.xz / noise_scale + TIME * time_scale).r;
    VERTEX.y += height * height_scale;
}

void fragment() {
    // --- 1) Normales animadas: calcular ANTES del if, para usar en ambos modos ---
    vec2 t1 = (TIME * wave_direction)  * time_scale;
    vec2 t2 = (TIME * wave_direction2) * time_scale;
    vec3 n1 = texture(texture_normal,  world_pos.xz + t1).rgb;
    vec3 n2 = texture(texture_normal2, world_pos.xz + t2).rgb;
    vec3 normal_blend = mix(n1, n2, 0.5);            // [0..1]
    vec3 normal_inside = vec3(1.0 - normal_blend.xy, // invertir XY para ver bien desde dentro
                              normal_blend.z);

    if (is_underwater) {
        // ===== MODO SUBMARINO =====
        NORMAL_MAP = normal_inside;   // <- ya existe porque lo calculamos arriba
        // Color opaco bajo el agua (sin screen texture)
        vec3 under_col = mix(color_deep.rgb, color_shallow.rgb, 0.2);
        ALBEDO    = under_col;
        METALLIC  = 0.0;
        ROUGHNESS = 1.0;
        ALPHA     = 1.0;              // totalmente opaco por dentro
    } else {
        // ===== MODO SUPERFICIE =====
        NORMAL_MAP = normal_blend;

        // (aquí va tu código original de profundidad, screen_tex, edges, etc.)
        float depth_texture = texture(depth_tex, SCREEN_UV).r * 2.0 - 1.0;
        float depth = PROJECTION_MATRIX[3][2] / (depth_texture + PROJECTION_MATRIX[2][2]);
        float depth_blend = exp((depth + VERTEX.z + depth_offset) * beers_law);
        depth_blend = clamp(1.0 - depth_blend, 0.0, 1.0);
        float depth_blend_power = clamp(pow(depth_blend, 2.5), 0.0, 1.0);

        vec3 screen_color = textureLod(screen_tex, SCREEN_UV, depth_blend_power * 2.5).rgb;
        vec3 depth_color  = mix(color_shallow.rgb, color_deep.rgb, depth_blend_power);
        vec3 color_mix    = mix(screen_color * depth_color, depth_color * 0.25, depth_blend_power * 0.5);

        float z_depth = edge(texture(depth_tex, SCREEN_UV).x);
        float z_pos   = edge(FRAGCOORD.z);
        float z_dif   = z_depth - z_pos;

        float f = pow(1.0 - clamp(dot(normalize(NORMAL), normalize(VIEW)), 0.0, 1.0), 5.0);
        vec3 surface_color = mix(albedo, albedo2, f);

        vec3 depth_color_adj = mix(edge_color, color_mix, step(edge_scale, z_dif));
        ALBEDO     = clamp(surface_color + depth_color_adj, vec3(0.0), vec3(1.0));
        METALLIC   = metallic;
        ROUGHNESS  = roughness;
        ALPHA = 1.0; // si tu superficie no necesita transparencia
    }
}
