#version 330 core
layout (location = 0) in vec3 Position;

void main()
{
   gl_Position = vec4(Position + vec3(1.0,1.0,0.0), 1.0);
}