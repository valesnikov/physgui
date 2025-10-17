#include "physgl.h"
#include <GL/glew.h>
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

static GLuint link_program(GLuint vertex_shader, GLuint fragment_shader) {
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
        GLuint vbo, ebo;
        unsigned int verts_count;
        GLfloat (*vbo_data)[2];
        GLuint (*ebo_data)[3];
    } circle;
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
        GLfloat (*data)[2];
    } radii;
};

static void generate_circle(struct physgl *phgl, unsigned int circle_verts) {
    for (unsigned int i = 0; i < circle_verts; i++) {
        const float angle = 2.0f * M_PI * i / circle_verts;
        phgl->circle.vbo_data[i][0] = cosf(angle);
        phgl->circle.vbo_data[i][1] = sinf(angle);
    }
    for (unsigned int i = 0; i < circle_verts - 2; i++) {
        phgl->circle.ebo_data[i][0] = 0;
        phgl->circle.ebo_data[i][1] = i + 1;
        phgl->circle.ebo_data[i][2] = i + 2;
    }
}

static void setup_buffers(struct physgl *phgl) {
    glGenVertexArrays(1, &phgl->vao);
    glBindVertexArray(phgl->vao);

    glGenBuffers(1, &phgl->circle.vbo);
    glBindBuffer(GL_ARRAY_BUFFER, phgl->circle.vbo);
    glBufferData(
        GL_ARRAY_BUFFER,
        sizeof(GLfloat[2]) * phgl->circle.verts_count,
        phgl->circle.vbo_data,
        GL_STATIC_DRAW
    );
    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, sizeof(GLuint[2]), (void *)0);
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

    glGenBuffers(1, &phgl->circle.ebo);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, phgl->circle.ebo);
    glBufferData(
        GL_ELEMENT_ARRAY_BUFFER,
        sizeof(GLuint[3]) * (phgl->circle.verts_count - 2),
        phgl->circle.ebo_data,
        GL_STATIC_DRAW
    );
    glBindVertexArray(0);
}

struct physgl *physgl_init(unsigned int circle_verts) {
    if (circle_verts < 3) {
        circle_verts = 3;
    }

    GLenum err = glewInit();
    if (err != GLEW_OK) {
        fprintf(stderr, "GLEW error: %s\n", glewGetErrorString(err));
        return NULL;
    }

    printf("OpenGL version: %s\n", glGetString(GL_VERSION));
    printf("GLSL version: %s\n", glGetString(GL_SHADING_LANGUAGE_VERSION));
    printf("Vendor: %s\n", glGetString(GL_VENDOR));
    printf("Renderer: %s\n", glGetString(GL_RENDERER));

    struct physgl *phgl = calloc(1, sizeof(struct physgl));
    if (phgl != NULL) {
        phgl->circle.verts_count = circle_verts;
        phgl->circle.vbo_data = malloc(sizeof(GLfloat[2]) * circle_verts);
        if (phgl->circle.vbo_data == NULL) {
            physgl_destroy(phgl);
            return NULL;
        }
        phgl->circle.ebo_data = malloc(sizeof(GLuint[3]) * (circle_verts - 2));
        if (phgl->circle.ebo_data == NULL) {
            physgl_destroy(phgl);
            return NULL;
        }

        generate_circle(phgl, circle_verts);
        setup_buffers(phgl);

        GLuint vs = compile_shader(GL_VERTEX_SHADER, (const char *)physgl_vertex_shader_src);
        GLuint fs = compile_shader(GL_FRAGMENT_SHADER, (const char *)physgl_fragment_shader_src);
        if (!vs || !fs) {
            physgl_destroy(phgl);
            return NULL;
        }

        phgl->shader_program = link_program(vs, fs);
        if (!phgl->shader_program) {
            physgl_destroy(phgl);
            return NULL;
        }

        phgl->count = 0;
        phgl->window_aspect = (float)16 / 9;
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
    GLfloat radii[2] = {0.3, 0.06};
    glBufferData(GL_ARRAY_BUFFER, sizeof(radii), radii, GL_DYNAMIC_DRAW);

    glClearColor(0.1, 0.1, 0.1, 1.0);

    glClear(GL_COLOR_BUFFER_BIT);

    glUseProgram(phgl->shader_program);

    glUniform2f(
        glGetUniformLocation(phgl->shader_program, "center"),
        (GLfloat)center_x,
        (GLfloat)center_y
    );
    glUniform1f(glGetUniformLocation(phgl->shader_program, "scale"), (GLfloat)scale);
    glUniform1f(glGetUniformLocation(phgl->shader_program, "aspect"), (GLfloat)phgl->window_aspect);

    GLsizei index_count = 3 * (phgl->circle.verts_count - 2);
    glDrawElementsInstanced(GL_TRIANGLES, index_count, GL_UNSIGNED_INT, 0, 2);
    check_gl_error("physgl_preview_render");
}

void physgl_on_resize(struct physgl *phgl, int width, int height) {
    phgl->window_aspect = (float)width / height;
}

void physgl_destroy(struct physgl *phgl) {
    glUseProgram(0);
    glBindVertexArray(0);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    if (phgl->vao) {
        glDeleteVertexArrays(1, &phgl->vao);
    }
    if (phgl->circle.vbo) {
        glDeleteBuffers(1, &phgl->circle.vbo);
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
    free(phgl->circle.vbo_data);
    free(phgl->circle.ebo_data);
    free(phgl->pos.data);
    free(phgl->radii.data);
    free(phgl->colors.data);
    free(phgl);
}
