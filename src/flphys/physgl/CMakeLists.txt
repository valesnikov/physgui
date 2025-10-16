add_library(physgl OBJECT)

target_sources(physgl PRIVATE
    physgl.c
)

target_include_directories(physgl PUBLIC 
    ${CMAKE_CURRENT_SOURCE_DIR}
)

if(POLICY CMP0072)
    cmake_policy(SET CMP0072 NEW)
endif()

set(OpenGL_GL_PREFERENCE GLVND)

find_package(OpenGL REQUIRED)
find_package(GLEW REQUIRED)

target_link_libraries(physgl PUBLIC
    OpenGL::GL
    GLEW::GLEW
    phys
)

find_library(M_LIB m)

if(M_LIB)
    message(STATUS "Found libm: ${M_LIB}")
    target_link_libraries(physgl PUBLIC ${M_LIB})
else()
    message(STATUS "libm not found — skipping")
endif()