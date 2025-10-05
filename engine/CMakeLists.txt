cmake_minimum_required(VERSION 3.5)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

add_compile_options(-Wall -Wextra -Wpedantic -std=c99 -pedantic)

project(
    flphys
    VERSION 1.0.0
    DESCRIPTION "A library for calculating multi-body motion"
    LANGUAGES C
)

add_library(${PROJECT_NAME} SHARED)

if(WIN32)
    target_compile_definitions(${PROJECT_NAME} PRIVATE FLPHYS_SHARED)
endif()


target_sources(${PROJECT_NAME} PRIVATE
    flphys.c
)

target_include_directories(${PROJECT_NAME} PUBLIC 
    ${CMAKE_CURRENT_SOURCE_DIR}
)

# Set library version
set_target_properties(${PROJECT_NAME} PROPERTIES
    VERSION ${PROJECT_VERSION}
    SOVERSION 1
)

# Basic installation
install(TARGETS ${PROJECT_NAME}
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)

install(FILES flphys.h
    DESTINATION include
)