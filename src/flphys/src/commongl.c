#include "commongl.h"


#include <stdio.h>

GLuint compile_shader(GLenum type, const char *source) {
    GLuint shader = glCreateShader(type);
    glShaderSource(shader, 1, &source, NULL);
    glCompileShader(shader);
    GLint status;
    glGetShaderiv(shader, GL_COMPILE_STATUS, &status);
    if (status != GL_TRUE) {
        char log[1024];
        glGetShaderInfoLog(shader, 1024, NULL, log);
        fprintf(stderr, "Shader compile error:\n%s\n", log);
        return 0;
    }
    return shader;
}

GLuint link_program(const char *vertex_shader_src, const char *fragment_shader_src) {
    GLuint vertex_shader = compile_shader(GL_VERTEX_SHADER, vertex_shader_src);
    if (vertex_shader == 0) {
        return 0;
    }
    GLuint fragment_shader = compile_shader(GL_FRAGMENT_SHADER, fragment_shader_src);
    if (fragment_shader == 0) {
        glDeleteShader(vertex_shader);
        return 0;
    }

    GLuint program = glCreateProgram();
    glAttachShader(program, vertex_shader);
    glAttachShader(program, fragment_shader);
    glLinkProgram(program);
    GLint status;
    glGetProgramiv(program, GL_LINK_STATUS, &status);
    if (status != GL_TRUE) {
        char log[1024];
        glGetProgramInfoLog(program, 1024, NULL, log);
        fprintf(stderr, "Program link error:\n%s\n", log);
        glDeleteProgram(program);
        glDeleteShader(vertex_shader);
        glDeleteShader(fragment_shader);
        return 0;
    }
    glDetachShader(program, vertex_shader);
    glDetachShader(program, fragment_shader);
    glDeleteShader(vertex_shader);
    glDeleteShader(fragment_shader);
    return program;
}

void check_gl_error(const char *tag) {
    GLenum err;
    while ((err = glGetError()) != GL_NO_ERROR) {
        fprintf(stderr, "[GL ERROR] %s: 0x%x\n", tag, err);
    }
}