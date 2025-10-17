function(generate_all_embedded_files SOURCES)
    # Get global properties containing files and corresponding variable names
    get_property(EMBEDDED_FILES_LIST GLOBAL PROPERTY EMBEDDED_FILES_LIST)
    get_property(EMBEDDED_VARIABLES_LIST GLOBAL PROPERTY EMBEDDED_VARIABLES_LIST)

    # Exit early if there are no embedded files
    if(NOT EMBEDDED_FILES_LIST)
        return()
    endif()
    
    # Count how many files will be processed
    list(LENGTH EMBEDDED_FILES_LIST FILE_COUNT)
    message(STATUS "Generating ${FILE_COUNT} embedded files...")
    
    # Iterate over all registered files
    math(EXPR LAST_INDEX "${FILE_COUNT} - 1")
    foreach(INDEX RANGE ${LAST_INDEX})
        # Extract file path and variable name by index
        list(GET EMBEDDED_FILES_LIST ${INDEX} INPUT_FILE)
        list(GET EMBEDDED_VARIABLES_LIST ${INDEX} VARIABLE_NAME)
        
        # Define output .c file path
        set(OUTPUT_C_FILE "${CMAKE_CURRENT_BINARY_DIR}/embedded_${VARIABLE_NAME}.c")

        # Add custom build rule for each embedded file
        add_custom_command(
            OUTPUT ${OUTPUT_C_FILE}
            COMMAND ${CMAKE_COMMAND} 
                -DINPUT_FILE=${INPUT_FILE}
                -DOUTPUT_C_FILE=${OUTPUT_C_FILE} 
                -DVARIABLE_NAME=${VARIABLE_NAME}
                -P "${CMAKE_CURRENT_SOURCE_DIR}/embed_file.cmake"
            MAIN_DEPENDENCY "${INPUT_FILE}"          # Re-run if input changes
            COMMENT "Generating embedded file: ${VARIABLE_NAME}"
            VERBATIM                                # Prevent argument misinterpretation
        )

        # Append generated .c file to output source list
        list(APPEND ${SOURCES} "${OUTPUT_C_FILE}")
    endforeach()
    
    # Return resulting list to parent scope
    set(${SOURCES} ${${SOURCES}} PARENT_SCOPE)
    message(STATUS "Successfully generated ${FILE_COUNT} embedded files")
endfunction()
