cmake_minimum_required(VERSION 3.10)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

add_compile_options(-Wall -Wextra -Wpedantic -std=c99 -pedantic)

project(
    flphys
    VERSION 1.0.0
    DESCRIPTION "A library for calculating multi-body motion"
    LANGUAGES C
)

add_library(${PROJECT_NAME} SHARED)

if(POLICY CMP0072)
    cmake_policy(SET CMP0072 NEW)
endif()

set(OpenGL_GL_PREFERENCE GLVND)

find_package(OpenGL REQUIRED)
find_package(GLEW REQUIRED)

target_link_libraries(${PROJECT_NAME} PRIVATE
    OpenGL::GL
    GLEW::GLEW
)

find_library(M_LIB m)

if(M_LIB)
    message(STATUS "Found libm: ${M_LIB}")
    target_link_libraries(${PROJECT_NAME} PRIVATE ${M_LIB})
else()
    message(STATUS "libm not found — skipping")
endif()

target_sources(${PROJECT_NAME} PRIVATE
    flphys.c
    flphysgl.c
)

target_include_directories(${PROJECT_NAME} PUBLIC 
    ${CMAKE_CURRENT_SOURCE_DIR}
)


install(TARGETS ${PROJECT_NAME}
    LIBRARY DESTINATION .
)