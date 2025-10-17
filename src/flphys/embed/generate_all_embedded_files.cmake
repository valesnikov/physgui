function(generate_all_embedded_files SOURCES)
    # Check if there are any files to process

    get_property(EMBEDDED_FILES_LIST GLOBAL PROPERTY EMBEDDED_FILES_LIST)
    get_property(EMBEDDED_VARIABLES_LIST GLOBAL PROPERTY EMBEDDED_VARIABLES_LIST)

    if(NOT EMBEDDED_FILES_LIST)
        return()
    endif()
    
    # Get the number of files to process
    list(LENGTH EMBEDDED_FILES_LIST FILE_COUNT)
    message(STATUS "Generating ${FILE_COUNT} embedded files...")
    
    # Process each registered file
    math(EXPR LAST_INDEX "${FILE_COUNT} - 1")
    foreach(INDEX RANGE ${LAST_INDEX})
        # Get file details from global lists
        list(GET EMBEDDED_FILES_LIST ${INDEX} INPUT_FILE)
        list(GET EMBEDDED_VARIABLES_LIST ${INDEX} VARIABLE_NAME)
        set(OUTPUT_C_FILE "${CMAKE_CURRENT_BINARY_DIR}/embedded_${VARIABLE_NAME}.c")

        add_custom_command(
            OUTPUT ${OUTPUT_C_FILE}
            COMMAND ${CMAKE_COMMAND} 
                -DINPUT_FILE=${INPUT_FILE}
                -DOUTPUT_C_FILE=${OUTPUT_C_FILE} 
                -DVARIABLE_NAME=${VARIABLE_NAME}
                -P "${CMAKE_CURRENT_SOURCE_DIR}/embed_file.cmake"
            MAIN_DEPENDENCY "${INPUT_FILE}"
            COMMENT "Generating embedded file: ${VARIABLE_NAME}"
            VERBATIM
        )

        list(APPEND ${SOURCES} "${OUTPUT_C_FILE}")
    endforeach()
    
    set(${SOURCES} ${${SOURCES}} PARENT_SCOPE)
    message(STATUS "Successfully generated ${FILE_COUNT} embedded files")
endfunction()
