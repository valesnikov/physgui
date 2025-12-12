#pragma once

#include <epoxy/gl.h>

GLuint compile_shader(GLenum type, const char *source);

GLuint link_program(const char *vertex_shader_src, const char *fragment_shader_src);

void check_gl_error(const char *tag);