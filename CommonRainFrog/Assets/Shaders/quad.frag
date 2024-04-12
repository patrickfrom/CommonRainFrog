#version 460 core

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;

const float width = 1280.0;
const float height = 720.0; 
const float pixelWidth = 5.0; 
const float pixelHeight = 5.0; 

void main()
{
    /*vec3 tc = vec3(1.0, 0.0, 0.0);
    if (TexCoords.x < (2.0-0.005))
    {
        float dx = pixelWidth*(1./width);
        float dy = pixelHeight*(1./height);
        vec2 coord = vec2(dx*floor(TexCoords.x/dx),
        dy*floor(TexCoords.y/dy));
        tc = texture(screenTexture, coord).rgb;
    }
    else if (TexCoords.x>=(2.0+0.005))
    {
        tc = texture(screenTexture, TexCoords).rgb;
    }
    FragColor = vec4(tc, 1.0);*/
    
    FragColor = texture(screenTexture, TexCoords);
}  