#ifdef GLSL

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='COLOR' Attribute='a_color' />

uniform mat4 u_worldViewProjectionMatrix;

attribute vec3 a_position;
attribute vec4 a_color;
varying vec4 v_color;

void main()
{
    v_color = a_color;
    gl_PointSize = 2.0;
    gl_Position = u_worldViewProjectionMatrix * vec4(a_position.xyz, 1.0);
    OPENGL_POSITION_FIX;
}

#endif
