## ======================================================================== ##
## Copyright 2009-2015 Intel Corporation                                    ##
##                                                                          ##
## Licensed under the Apache License, Version 2.0 (the "License");          ##
## you may not use this file except in compliance with the License.         ##
## You may obtain a copy of the License at                                  ##
##                                                                          ##
##     http://www.apache.org/licenses/LICENSE-2.0                           ##
##                                                                          ##
## Unless required by applicable law or agreed to in writing, software      ##
## distributed under the License is distributed on an "AS IS" BASIS,        ##
## WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. ##
## See the License for the specific language governing permissions and      ##
## limitations under the License.                                           ##
## ======================================================================== ##

SET(EMBREE_INCLUDE_DIRS ${CMAKE_CURRENT_LIST_DIR}/../../../@CMAKE_INSTALL_INCLUDEDIR@)
SET(EMBREE_LIBRARY ${CMAKE_CURRENT_LIST_DIR}/../../libembree.so.@EMBREE_CONFIG_VERSION@)
SET(EMBREE_LIBRARY_XEONPHI ${CMAKE_CURRENT_LIST_DIR}/../../libembree_xeonphi.so.@EMBREE_CONFIG_VERSION@)

MARK_AS_ADVANCED(embree_DIR)

INCLUDE(${CMAKE_CURRENT_LIST_DIR}/embree-config-default.cmake)
