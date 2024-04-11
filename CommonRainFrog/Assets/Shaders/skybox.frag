#version 460 core

out vec4 FragColor;

in vec3 Position;

uniform samplerCube skybox;

void main() {
    FragColor = texture(skybox, Position);
}