#version 300 es
#ifdef GLSL

// <Semantic Name='POSITION' Attribute='a_position' />

uniform mat4 u_worldViewProjectionMatrix;

in vec3 a_position;

void main()
{
    gl_Position = u_worldViewProjectionMatrix * vec4(a_position.xyz, 1.0);
    OPENGL_POSITION_FIX;
}

#endif
