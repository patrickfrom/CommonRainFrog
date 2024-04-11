#version 460 core

out vec4 FragColor;

in vec2 TexCoords;


void main() {
    // Get the fragment's normalized coordinates
    vec2 st = gl_PointCoord;

    // Interpolate between red and blue based on the fragment's y-coordinate
    vec3 color = mix(vec3(1.0, 0.0, 0.0), vec3(0.0, 0.0, 1.0), TexCoords.y);

    FragColor = vec4(color, 1.0);
}