#ifdef GLSL

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='COLOR' Attribute='a_color' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

uniform vec2 u_origin;
uniform mat4 u_viewProjectionMatrix;
uniform vec3 u_viewPosition;
uniform mat4 u_subterrainTransform;

attribute vec3 a_position;
attribute vec4 a_color;
attribute vec2 a_texcoord;

varying vec4 v_color;
varying vec2 v_texcoord;

void main()
{
    v_texcoord = a_texcoord;
    v_color = a_color;

    a_position = u_subterrainTransform * vec4(a_position, 1.0);
    gl_Position = u_viewProjectionMatrix * vec4(a_position.x - u_origin.x, a_position.y, a_position.z - u_origin.y, 1.0);

    OPENGL_POSITION_FIX;
}

#endif
