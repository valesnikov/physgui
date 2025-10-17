#version 330 core

in vec3 vColor;
in vec2 vPos;
in vec2 vCenter;
in float vRadius;

out vec4 FragColor;

void main() {
    if (length(vPos) <= 1) {
        FragColor = vec4(vColor, 1.0);
    } else {
        FragColor = vec4(0,0,0,0);
    }
}