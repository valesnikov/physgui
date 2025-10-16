cmake_minimum_required(VERSION 3.10)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

add_compile_options(-Wall -Wextra -Wpedantic -std=c99 -pedantic)
set(CMAKE_C_STANDARD 99)

project(
    flphys
    VERSION 1.0.0
    DESCRIPTION "A library for calculating multi-body motion"
    LANGUAGES C
)

add_subdirectory(phys)
add_subdirectory(physgl)

add_library(${CMAKE_PROJECT_NAME} SHARED)

target_link_libraries(${CMAKE_PROJECT_NAME} PUBLIC
    physgl
    phys
)

install(TARGETS ${CMAKE_PROJECT_NAME}
        LIBRARY DESTINATION .)