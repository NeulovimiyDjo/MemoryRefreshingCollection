#version 330 core
flat in highp vec3 v_light_pos;
in highp vec3 v_vert_pos;
in highp vec2 v_texcoord;
in highp vec3 v_normal;

out highp vec4 fragColor;

uniform bool is_green;
uniform sampler2D texture0;
uniform sampler2D texture1;

void main() {
  vec4 color0 = texture2D(texture0, v_texcoord);
  vec4 color1 = texture2D(texture1, v_texcoord);

  float factor = 0.5;
  if (color1.x > 0.5 && color1.y > 0.25 && color1.z > 0.25) // ignore white space of texture1
    factor = 0;

  vec3 color = mix(color0, color1, factor).xyz;

  vec3 L = normalize(v_light_pos - v_vert_pos);
  float NL = max(dot(normalize(v_normal), L), 0.0);
  color = clamp(color * 0.2 + color * 0.8 * NL, 0.0, 1.0);


  fragColor = vec4(color, 1.0);
  if (is_green)
    fragColor = mix(fragColor, vec4(0.0, 1.0, 0.0, 1.0), 0.2);
}
