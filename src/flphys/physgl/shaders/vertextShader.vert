#version 330 core
layout(location = 0) in vec2 aVert;
layout(location = 1) in vec3 aColor;
layout(location = 2) in vec2 aPosition;
layout(location = 3) in float aRadius;

out vec3 vColor;
uniform vec2 center;
uniform float scale;
uniform float aspect;

void main() {
    vColor = aColor;
    vec2 pos = (aVert * aRadius + aPosition) * scale - center * scale;
    pos.x /= aspect;
    gl_Position = vec4(pos, 0.0, 1.0);
}