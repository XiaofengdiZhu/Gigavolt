#ifdef GLSL

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

uniform mat4 u_worldViewProjectionMatrix;

attribute vec3 a_position;
attribute vec2 a_texcoord;
varying vec2 v_texcoord;

void main()
{
    v_texcoord = a_texcoord;
    gl_Position = u_worldViewProjectionMatrix * vec4(a_position.xyz, 1.0);
    OPENGL_POSITION_FIX;
}

#endif
