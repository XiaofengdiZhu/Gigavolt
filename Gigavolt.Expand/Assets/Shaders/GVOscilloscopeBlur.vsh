#version 300 es
#ifdef GLSL

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

uniform mat4 u_worldViewProjectionMatrix;

in vec3 a_position;
in vec2 a_texcoord;
out vec2 v_texcoord;

void main()
{
    v_texcoord = a_texcoord;
    gl_Position = u_worldViewProjectionMatrix * vec4(a_position.xyz, 1.0);
    OPENGL_POSITION_FIX;
}

#endif
