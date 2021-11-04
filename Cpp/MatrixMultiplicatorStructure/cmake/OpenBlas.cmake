set(OPENBLAS_DLL_MANIFEST_PATH "${CMAKE_SOURCE_DIR}/../../libs/OpenBLAS-0.2.20/package/lib/libopenblas.dll.a")
set(OPENBLAS_LIB_PATH "${CMAKE_SOURCE_DIR}/../../libs/OpenBLAS-0.2.20/package/lib/libopenblas.a")
set(OPENBLAS_DLL_DIR "${CMAKE_SOURCE_DIR}/../../libs/OpenBLAS-0.2.20/package/bin")

list(APPEND OPENBLAS_DLL_FILES "${OPENBLAS_DLL_DIR}/libopenblas.dll")
list(APPEND OPENBLAS_DLL_FILES "${OPENBLAS_DLL_DIR}/libgcc_s_sjlj-1.dll")
list(APPEND OPENBLAS_DLL_FILES "${OPENBLAS_DLL_DIR}/libgfortran-4.dll")
list(APPEND OPENBLAS_DLL_FILES "${OPENBLAS_DLL_DIR}/libquadmath-0.dll")
list(APPEND OPENBLAS_DLL_FILES "${OPENBLAS_DLL_DIR}/libwinpthread-1.dll")

function(link_target_to_openblas target)
  if(MSVC)
    add_library(openblas SHARED IMPORTED)
    set_target_properties(openblas PROPERTIES IMPORTED_IMPLIB "${OPENBLAS_DLL_MANIFEST_PATH}")
    target_link_libraries(${target} openblas)
  elseif(MINGW)
    add_library(openblas STATIC IMPORTED)
    set_target_properties(openblas PROPERTIES IMPORTED_LOCATION "${OPENBLAS_LIB_PATH}")
    target_link_libraries(${target} openblas)
  endif()

  if(MSVC)
	foreach (dll_file ${OPENBLAS_DLL_FILES})
		add_custom_command(TARGET ${target}
						  POST_BUILD
						  COMMAND ${CMAKE_COMMAND} -E copy_if_different
						  "${dll_file}"
						  $<TARGET_FILE_DIR:${target}>)
	endforeach()
  endif()
endfunction()