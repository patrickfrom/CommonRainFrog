#version 460 core

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;

const float rt_w = 1280.0;
const float rt_h = 720.0; 
const float pixel_w = 5.0; 
const float pixel_h = 5.0; 

void main()
{
    vec3 tc = vec3(1.0, 0.0, 0.0);
    if (TexCoords.x < (2.0-0.005))
    {
        float dx = pixel_w*(1./rt_w);
        float dy = pixel_h*(1./rt_h);
        vec2 coord = vec2(dx*floor(TexCoords.x/dx),
        dy*floor(TexCoords.y/dy));
        tc = texture(screenTexture, coord).rgb;
    }
    else if (TexCoords.x>=(2.0+0.005))
    {
        tc = texture(screenTexture, TexCoords).rgb;
    }
    FragColor = vec4(tc, 1.0);
}  