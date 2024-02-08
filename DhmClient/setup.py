import numpy
import setuptools

setuptools.setup(
    ext_modules = [
        setuptools.Extension(
            name="_scalardiffract",
            sources=["scalardiffract/diffract.c"],
            include_dirs=[numpy.get_include()],
            extra_compile_args=["/openmp"],
            ),
        ],
)
