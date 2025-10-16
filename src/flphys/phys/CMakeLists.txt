add_library(phys OBJECT)

target_sources(phys PRIVATE
    phys.c
)

target_include_directories(phys PUBLIC 
    ${CMAKE_CURRENT_SOURCE_DIR}
)

find_library(M_LIB m)

if(M_LIB)
    message(STATUS "Found libm: ${M_LIB}")
    target_link_libraries(phys PUBLIC ${M_LIB})
else()
    message(STATUS "libm not found — skipping")
endif()