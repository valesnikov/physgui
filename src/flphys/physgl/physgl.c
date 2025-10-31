#include "physgl.h"
#include <epoxy/gl.h>
#include <math.h>
#include <stdio.h>
#include <stdlib.h>

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

// from generated
extern const unsigned char physgl_vertex_shader_src[];
extern const unsigned char physgl_fragment_shader_src[];

static GLuint compile_shader(GLenum type, const char *source) {
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

static GLuint link_program(const char *vertex_shader_src, const char *fragment_shader_src) {
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

static void check_gl_error(const char *tag) {
    GLenum err;
    while ((err = glGetError()) != GL_NO_ERROR) {
        fprintf(stderr, "[GL ERROR] %s: 0x%x\n", tag, err);
    }
}

struct physgl {
    GLuint vao;
    GLuint shader_program;

    GLfloat window_aspect; // width / height
    struct {
        GLuint vbo;
        GLuint ebo;
    } base_fig;

    unsigned int count;
    struct {
        GLuint vbo;
        GLfloat (*data)[3];
    } colors;
    struct {
        GLuint vbo;
        GLfloat (*data)[2];
    } pos;
    struct {
        GLuint vbo;
        GLfloat *data;
    } radii;

    struct { // for uniform variables
        GLuint center;
        GLuint scale;
        GLuint aspect;
    } locs;
};

static void setup_buffers(struct physgl *phgl) {
    glGenVertexArrays(1, &phgl->vao);
    glBindVertexArray(phgl->vao);

    glGenBuffers(1, &phgl->base_fig.vbo);
    glBindBuffer(GL_ARRAY_BUFFER, phgl->base_fig.vbo);
    const GLfloat vertices[] = {-1.0f, -1.0f, 1.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f}; //square
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, 0, (void *)0);
    glEnableVertexAttribArray(0);
    glVertexAttribDivisor(0, 0);

    glGenBuffers(1, &phgl->colors.vbo);
    glBindBuffer(GL_ARRAY_BUFFER, phgl->colors.vbo);
    glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 0, (void *)0);
    glEnableVertexAttribArray(1);
    glVertexAttribDivisor(1, 1);

    glGenBuffers(1, &phgl->pos.vbo);
    glBindBuffer(GL_ARRAY_BUFFER, phgl->pos.vbo);
    glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, 0, (void *)0);
    glEnableVertexAttribArray(2);
    glVertexAttribDivisor(2, 1);

    glGenBuffers(1, &phgl->radii.vbo);
    glBindBuffer(GL_ARRAY_BUFFER, phgl->radii.vbo);
    glVertexAttribPointer(3, 1, GL_FLOAT, GL_FALSE, 0, (void *)0);
    glEnableVertexAttribArray(3);
    glVertexAttribDivisor(3, 1);

    glGenBuffers(1, &phgl->base_fig.ebo);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, phgl->base_fig.ebo);
    const GLuint indices[] = {0, 1, 2, 2, 3, 0};
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);
    glBindVertexArray(0);
}

static int physgl_init(struct physgl *phgl) {
    printf("OpenGL version: %s\n", glGetString(GL_VERSION));
    printf("GLSL version: %s\n", glGetString(GL_SHADING_LANGUAGE_VERSION));
    printf("Vendor: %s\n", glGetString(GL_VENDOR));
    printf("Renderer: %s\n", glGetString(GL_RENDERER));
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

    setup_buffers(phgl);

    phgl->shader_program =
        link_program((const char *)physgl_vertex_shader_src, (const char *)physgl_fragment_shader_src);
    if (!phgl->shader_program) {
        return -1;
    }

    phgl->locs.aspect = glGetUniformLocation(phgl->shader_program, "aspect");
    phgl->locs.center = glGetUniformLocation(phgl->shader_program, "center");
    phgl->locs.scale = glGetUniformLocation(phgl->shader_program, "scale");

    phgl->count = 0;
    phgl->window_aspect = (float)16 / 9;
    return 0;
}

struct physgl *physgl_create(void) {
    struct physgl *phgl = calloc(1, sizeof(*phgl));
    if (!phgl)
        return NULL;

    if (physgl_init(phgl) < 0) {
        free(phgl);
        return NULL;
    }
    return phgl;
}

void physgl_preview_render(struct physgl *phgl, double center_x, double center_y, double scale) {
    glBindVertexArray(phgl->vao);

    glBindBuffer(GL_ARRAY_BUFFER, phgl->colors.vbo);
    GLfloat color[6] =
        {0.9, 0.9, 0.9, (float)rand() / RAND_MAX, (float)rand() / RAND_MAX, (float)rand() / RAND_MAX};
    glBufferData(GL_ARRAY_BUFFER, sizeof(color), color, GL_DYNAMIC_DRAW);

    glBindBuffer(GL_ARRAY_BUFFER, phgl->pos.vbo);
    GLfloat pos[4] = {0, 0, 0.3, 0.3};
    glBufferData(GL_ARRAY_BUFFER, sizeof(pos), pos, GL_DYNAMIC_DRAW);

    glBindBuffer(GL_ARRAY_BUFFER, phgl->radii.vbo);
    GLfloat radii[2] = {0.3, 0.05};
    glBufferData(GL_ARRAY_BUFFER, sizeof(radii), radii, GL_DYNAMIC_DRAW);

    glClearColor(0.1, 0.1, 0.1, 1.0);

    glClear(GL_COLOR_BUFFER_BIT);

    glUseProgram(phgl->shader_program);

    glUniform2f(phgl->locs.center, center_x, center_y);
    glUniform1f(phgl->locs.scale, scale);
    glUniform1f(phgl->locs.aspect, phgl->window_aspect);

    glDrawElementsInstanced(GL_TRIANGLES, 3 * 2, GL_UNSIGNED_INT, 0, 2);
    check_gl_error("physgl_preview_render");
}

void physgl_on_resize(struct physgl *phgl, double aspect_ratio) {
    phgl->window_aspect = aspect_ratio;
}

void physgl_destroy(struct physgl *phgl) {
    if (phgl->vao) {
        glDeleteVertexArrays(1, &phgl->vao);
    }
    if (phgl->base_fig.vbo) {
        glDeleteBuffers(1, &phgl->base_fig.vbo);
    }
    if (phgl->base_fig.ebo) {
        glDeleteBuffers(1, &phgl->base_fig.ebo);
    }
    if (phgl->colors.vbo) {
        glDeleteBuffers(1, &phgl->colors.vbo);
    }
    if (phgl->pos.vbo) {
        glDeleteBuffers(1, &phgl->pos.vbo);
    }
    if (phgl->radii.vbo) {
        glDeleteBuffers(1, &phgl->radii.vbo);
    }
    if (phgl->shader_program) {
        glDeleteProgram(phgl->shader_program);
    }
    free(phgl->pos.data);
    free(phgl->radii.data);
    free(phgl->colors.data);
    free(phgl);
}
