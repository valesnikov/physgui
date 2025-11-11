cmake_minimum_required(VERSION 3.12)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

add_compile_options(-Wall -Wextra -Wpedantic -std=c11 -pedantic)
set(CMAKE_C_STANDARD 11)

project(
    flphys
    VERSION 1.0.0
    DESCRIPTION "A library for calculating multi-body motion"
    LANGUAGES C
)

include(embed/register_embedded_file.cmake)
register_embedded_file("${CMAKE_CURRENT_SOURCE_DIR}/shaders/vertexShader.vert" physgl_vertex_shader_src)
register_embedded_file("${CMAKE_CURRENT_SOURCE_DIR}/shaders/fragmentShader.frag" physgl_fragment_shader_src)
add_subdirectory(embed)

add_library(${CMAKE_PROJECT_NAME} SHARED)

target_include_directories(${CMAKE_PROJECT_NAME} PUBLIC 
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_sources(${CMAKE_PROJECT_NAME} PRIVATE
    src/physgl.c
    src/phys.c
)

find_package(PkgConfig REQUIRED)
pkg_check_modules(EPOXY REQUIRED epoxy)

if(POLICY CMP0072)
    cmake_policy(SET CMP0072 NEW)
endif()

set(OpenGL_GL_PREFERENCE GLVND)

find_package(OpenGL REQUIRED)

target_include_directories(${CMAKE_PROJECT_NAME} PRIVATE
    ${EPOXY_INCLUDE_DIRS}
)

target_compile_options(${CMAKE_PROJECT_NAME} PRIVATE
    ${EPOXY_CFLAGS_OTHER}
)

target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE
    OpenGL::GL
    ${EPOXY_LIBRARIES}
)

target_link_libraries(${CMAKE_PROJECT_NAME} PUBLIC
    embedded_files
)

find_library(MATH_LIB m)
if(MATH_LIB)
    message(STATUS "Found libm: ${MATH_LIB}")
    target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE ${MATH_LIB})
else()
    message(STATUS "libm not found — skipping")
endif()

install(TARGETS ${CMAKE_PROJECT_NAME}
        LIBRARY DESTINATION .)