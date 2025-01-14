#version 330 core
layout (location = 0) in vec3 Position;
layout (location = 1) in vec3 Color;

out vec3 color;

void main()
{
   gl_Position = vec4(Position + vec3(-0.5,-0.5,0.0), 1.0);
   color = Color;
}