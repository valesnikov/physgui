#version 330 core

layout(location = 0) in vec2 aPos;
layout(location = 1) in vec3 aColor;

out vec3 vColor;

uniform vec2 center;
uniform float scale;
uniform float aspect;

void main() {
    vColor = aColor;
    vec2 pos = (aPos - center) * scale;
    pos.x /= aspect;
    gl_Position = vec4(pos, 0.0, 1.0);
}