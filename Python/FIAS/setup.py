import sys
from setuptools import setup
from Cython.Build import cythonize

setup(
    ext_modules = cythonize("filter_and_split_fias_xml.pyx", compiler_directives={'language_level' : sys.version_info[0]})
)