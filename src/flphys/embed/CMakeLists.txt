include(${CMAKE_CURRENT_SOURCE_DIR}/generate_all_embedded_files.cmake)

generate_all_embedded_files(SOURCES)

if(SOURCES)
    add_library(embedded_files OBJECT ${SOURCES})
else()
    add_library(embedded_files INTERFACE)
endif()