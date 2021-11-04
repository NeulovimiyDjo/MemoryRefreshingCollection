#version 330 core
in vec4 v_color;
in vec2 v_texture_coords;

uniform ivec2 u_window_size;
uniform float u_shift;
uniform sampler2D texture0;
uniform sampler2D texture1;

out vec4 frag_color;

void main()
{
  const float Pi = 3.14;
  float pos_x = (gl_FragCoord.x-200)/(u_window_size.x-200-200)*3;
  vec4 color1 = vec4((cos(Pi*pos_x + u_shift*8)+1)/2, (cos(Pi*pos_x + u_shift*4)+1)/2, (cos(Pi*pos_x + u_shift * 2)+1)/2, 1.0); // from window coords
  vec4 color2 = vec4((cos(Pi+Pi*v_color.r + u_shift * 8)+1)/2, (cos(Pi+Pi*v_color.g + u_shift * 4)+1)/2, (cos(Pi+Pi*v_color.b + u_shift * 2)+1)/2, 1.0); // from world coords
  frag_color = mix(texture(texture0, v_texture_coords), texture(texture1, v_texture_coords), 0.2);
  frag_color = mix(frag_color, color2, 0.3);
}