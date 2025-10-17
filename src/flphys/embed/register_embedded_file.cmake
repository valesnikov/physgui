# Function to register an embedded file and its corresponding variable name
function(register_embedded_file FILE_PATH VARIABLE_NAME)

    # Retrieve the current global lists of embedded files and variable names
    get_property(CURRENT_FILES GLOBAL PROPERTY EMBEDDED_FILES_LIST)
    get_property(CURRENT_VARS GLOBAL PROPERTY EMBEDDED_VARIABLES_LIST)

    # Append the new file path and variable name to the lists
    list(APPEND CURRENT_FILES "${FILE_PATH}")
    list(APPEND CURRENT_VARS "${VARIABLE_NAME}")
    
    # Store the updated lists back to global properties
    set_property(GLOBAL PROPERTY EMBEDDED_FILES_LIST "${CURRENT_FILES}")
    set_property(GLOBAL PROPERTY EMBEDDED_VARIABLES_LIST "${CURRENT_VARS}")
    
    # Print a status message indicating successful registration
    message(STATUS "Registered embedded file: ${FILE_PATH} as ${VARIABLE_NAME}")

endfunction()
