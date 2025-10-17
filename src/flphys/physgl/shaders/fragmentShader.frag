#version 330 core

in vec3 vColor;
in vec2 vPos;

out vec4 FragColor;

void main() {
    float dist = length(vPos);
    float smoothing = fwidth(dist);
    float alpha = smoothstep(1.0, 1.0 - smoothing, dist);
    FragColor = vec4(vColor, alpha);
}