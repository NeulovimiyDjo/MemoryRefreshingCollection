#version 330 core
layout (location = 0) in vec4 a_position;
layout (location = 1) in vec2 a_texcoord;
layout (location = 2) in vec3 a_normal;

flat out highp vec3 v_light_pos;
out highp vec3 v_vert_pos;
out highp vec2 v_texcoord;
out highp vec3 v_normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 view_inverted;
uniform mat4 projection;

void main() {
  gl_Position = projection * view * model * a_position;

  vec4 light_pos = vec4(0.0, 0.0, 0.0, 1.0);
  vec4 light_world_pos = view_inverted * light_pos; // place light source on camera position

  vec4 vert_world_pos = model * a_position;

  v_light_pos = light_world_pos.xyz;
  v_vert_pos = vert_world_pos.xyz;
  v_texcoord = a_texcoord;
  v_normal = a_normal;
}
