# Check if input file exists
if(NOT EXISTS "${INPUT_FILE}")
    message(FATAL_ERROR "Input file ${INPUT_FILE} does not exist")
endif()

# Read file content as hexadecimal and get file size
file(READ "${INPUT_FILE}" CONTENT HEX)
file(SIZE "${INPUT_FILE}" CONTENT_SIZE)

# Add null terminator to the content (useful for text files)
set(CONTENT "${CONTENT}00")

# Write C header and variable declarations
file(WRITE ${OUTPUT_C_FILE} "// AUTO-GENERATED FILE - DO NOT EDIT DIRECTLY\n")
file(APPEND ${OUTPUT_C_FILE} "\n")
file(APPEND ${OUTPUT_C_FILE} "#include <stddef.h>\n")
file(APPEND ${OUTPUT_C_FILE} "\n")
file(APPEND ${OUTPUT_C_FILE} "extern const unsigned char ${VARIABLE_NAME}[];\n")
file(APPEND ${OUTPUT_C_FILE} "extern const size_t ${VARIABLE_NAME}_len;\n")
file(APPEND ${OUTPUT_C_FILE} "\n")

# Start array definition
file(APPEND ${OUTPUT_C_FILE} "const unsigned char ${VARIABLE_NAME}[] = {\n")
file(APPEND ${OUTPUT_C_FILE} "    ")

# Process each byte in hexadecimal content
string(LENGTH ${CONTENT} LEN)
set(INDEX 0)
set(BYTE_COUNT 0)

while(INDEX LESS LEN)
    # Extract two characters representing one byte in hex
    string(SUBSTRING ${CONTENT} ${INDEX} 2 BYTE_HEX)

    # Write byte to output file
    file(APPEND ${OUTPUT_C_FILE} "0x${BYTE_HEX}")

    # Move to next byte position
    math(EXPR INDEX "${INDEX} + 2")
    math(EXPR BYTE_COUNT "${BYTE_COUNT} + 1")

    # Add comma separator if not the last byte
    if(INDEX LESS LEN)
        file(APPEND ${OUTPUT_C_FILE} ", ")
    endif()

    # Line break after every 16 bytes for readability
    if(BYTE_COUNT GREATER 0 AND BYTE_COUNT LESS CONTENT_SIZE)
        math(EXPR REMAINDER "${BYTE_COUNT} % 16")
        if(REMAINDER EQUAL 0 AND INDEX LESS LEN)
            file(APPEND ${OUTPUT_C_FILE} "\n    ")
        endif()
    endif()
endwhile()

# Finish array definition and add length variable
file(APPEND ${OUTPUT_C_FILE} "\n")
file(APPEND ${OUTPUT_C_FILE} "};\n")
file(APPEND ${OUTPUT_C_FILE} "\n")
file(APPEND ${OUTPUT_C_FILE} "const size_t ${VARIABLE_NAME}_len = ${CONTENT_SIZE};\n")