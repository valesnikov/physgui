#include "backgl.h"

#include <epoxy/gl.h>
#include <stdlib.h>

#include "commongl.h"

#define INIT_CAP 16

struct backgl {
    GLuint vao;
    GLuint vbo;
    GLuint program;
    GLsizei count;
    struct {
        GLuint center;
        GLuint scale;
        GLuint aspect;
    } locs;
    GLfloat background_color[3];
};

struct vert {
    GLfloat pos[2];
    GLfloat color[3];
};

struct backgl_builder {
    unsigned int cap;
    unsigned int len;
    struct vert *verts;
    GLfloat background_color[3];
};

struct backgl_builder *backgl_builder_create(void) {
    struct backgl_builder *bglb = calloc(1, sizeof(struct backgl_builder));
    bglb->len = 0;
    bglb->cap = INIT_CAP;
    bglb->verts = malloc(sizeof(struct vert) * INIT_CAP);
    return bglb;
}

void backgl_builder_add(
    struct backgl_builder *bglb,
    float x1,
    float y1,
    float x2,
    float y2,
    float x3,
    float y3,
    unsigned char r,
    unsigned char g,
    unsigned char b
) {
    if (bglb->len + 3 > bglb->cap) {
        bglb->cap *= 2;
        bglb->verts = realloc(bglb->verts, sizeof(struct vert) * bglb->cap);
    }

    bglb->verts[bglb->len].pos[0] = x1;
    bglb->verts[bglb->len].pos[1] = y1;
    bglb->verts[bglb->len + 1].pos[0] = x2;
    bglb->verts[bglb->len + 1].pos[1] = y2;
    bglb->verts[bglb->len + 2].pos[0] = x3;
    bglb->verts[bglb->len + 2].pos[1] = y3;

    for (int i = 0; i < 3; i++) {
        bglb->verts[bglb->len + i].color[0] = r / 255.0;
        bglb->verts[bglb->len + i].color[1] = g / 255.0;
        bglb->verts[bglb->len + i].color[2] = b / 255.0;
    }

    bglb->len += 3;
}

// from generated
extern const unsigned char backgl_vertex_shader_src[];
extern const unsigned char backgl_fragment_shader_src[];

struct backgl *backgl_builder_build(struct backgl_builder *bglb) {
    struct backgl *bgl = calloc(1, sizeof(struct backgl));
    if (!bgl)
        return NULL;

    glGenVertexArrays(1, &bgl->vao);
    if (bgl->vao == 0) {
        free(bgl);
        return NULL;
    }

    glBindVertexArray(bgl->vao);

    glGenBuffers(1, &bgl->vbo);
    if (bgl->vbo == 0) {
        glDeleteVertexArrays(1, &bgl->vao);
        free(bgl);
        return NULL;
    }

    glBindBuffer(GL_ARRAY_BUFFER, bgl->vbo);
    glBufferData(GL_ARRAY_BUFFER, bglb->len * sizeof(struct vert), bglb->verts, GL_STATIC_DRAW);

    GLenum err = glGetError();
    if (err != GL_NO_ERROR) {
        glDeleteBuffers(1, &bgl->vbo);
        glDeleteVertexArrays(1, &bgl->vao);
        free(bgl);
        return NULL;
    }

    glEnableVertexAttribArray(0);
    glVertexAttribPointer(
        0,
        2,
        GL_FLOAT,
        GL_FALSE,
        sizeof(struct vert),
        (void *)offsetof(struct vert, pos)
    );

    glEnableVertexAttribArray(1);
    glVertexAttribPointer(
        1,
        3,
        GL_FLOAT,
        GL_FALSE,
        sizeof(struct vert),
        (void *)offsetof(struct vert, color)
    );

    err = glGetError();
    if (err != GL_NO_ERROR) {
        glDeleteBuffers(1, &bgl->vbo);
        glDeleteVertexArrays(1, &bgl->vao);
        free(bgl);
        return NULL;
    }

    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glBindVertexArray(0);

    bgl->program =
        link_program((const char *)backgl_vertex_shader_src, (const char *)backgl_fragment_shader_src);
    if (bgl->program == 0) {
        glDeleteBuffers(1, &bgl->vbo);
        glDeleteVertexArrays(1, &bgl->vao);
        free(bgl);
        return NULL;
    }

    bgl->locs.aspect = glGetUniformLocation(bgl->program, "aspect");
    bgl->locs.center = glGetUniformLocation(bgl->program, "center");
    bgl->locs.scale = glGetUniformLocation(bgl->program, "scale");

    bgl->count = bglb->len;

    check_gl_error("backgl_builder_build");

    for (int i = 0; i < 3; i++) {
        bgl->background_color[i] = bglb->background_color[i];
    }

    free(bglb->verts);
    free(bglb);
    return bgl;
}

void backgl_render(struct backgl *bgl, double center_x, double center_y, double scale, double aspect) {
    glClearColor(bgl->background_color[0], bgl->background_color[1], bgl->background_color[2], 1.0);

    glClear(GL_COLOR_BUFFER_BIT);

    glBindVertexArray(bgl->vao);
    glUseProgram(bgl->program);

    glUniform2f(bgl->locs.center, center_x, center_y);
    glUniform1f(bgl->locs.scale, scale);
    glUniform1f(bgl->locs.aspect, aspect);

    glDrawArrays(GL_TRIANGLES, 0, bgl->count);
    check_gl_error("backgl_render");
}

void backgl_destroy(struct backgl *bgl) {
    if (bgl->vao) {
        glDeleteVertexArrays(1, &bgl->vao);
    }
    if (bgl->vbo) {
        glDeleteBuffers(1, &bgl->vbo);
    }
    if (bgl->program) {
        glDeleteProgram(bgl->program);
    }
    free(bgl);
}

void backgl_builder_set_background_color(
    struct backgl_builder *bglb,
    unsigned char r,
    unsigned char g,
    unsigned char b
) {
    bglb->background_color[0] = r / 255.0;
    bglb->background_color[1] = g / 255.0;
    bglb->background_color[2] = b / 255.0;
}

void backgl_builder_cancel(struct backgl_builder *bglb) {
    free(bglb->verts);
    free(bglb);
}