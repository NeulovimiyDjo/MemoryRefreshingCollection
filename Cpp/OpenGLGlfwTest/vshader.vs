#version 330 core
layout(location = 0) in vec3 a_pos;
layout(location = 1) in vec3 a_color;
layout(location = 2) in vec2 a_texture_coords;

out vec4 v_color;
out vec2 v_texture_coords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {
  gl_Position = projection * view * model * vec4(a_pos, 1.0);
  v_color = vec4(a_color, 1.0);
  v_texture_coords = a_texture_coords;
}