shader_type canvas_item;

uniform vec2  center = vec2(0.5, 0.5); // centro en UV
uniform float radius = 0.48;           // radio del círculo (0..0.5 aprox)
uniform float feather = 0.01;          // borde suave

void fragment() {
    vec4 col = texture(TEXTURE, UV);
    float d = distance(UV, center);
    // 1 dentro del círculo, 0 fuera (con suavizado)
    float mask = 1.0 - smoothstep(radius, radius + feather, d);
    COLOR = col;
    COLOR.a *= mask;
}
