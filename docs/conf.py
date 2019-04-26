# -*- coding: utf-8 -*-
#
# Teak: Unity documentation build configuration file, created by
# sphinx-quickstart on Tue Aug 29 19:04:36 2017.
#
# This file is execfile()d with the current directory set to its
# containing dir.
#
# Note that not all possible configuration values are present in this
# autogenerated file.
#
# All configuration values have a default; values that are commented out
# serve to show the default.

# If extensions (or modules to document with autodoc) are in another directory,
# add these directories to sys.path here. If the directory is relative to the
# documentation root, use os.path.abspath to make it absolute, like shown here.
#
# import sys
# sys.path.insert(0, os.path.abspath('.'))

import os, subprocess, re, sys, errno
from functools import cmp_to_key

read_the_docs_build = os.environ.get('READTHEDOCS', None) == 'True'
if read_the_docs_build:
    subprocess.call('cd .. ; doxygen', shell=True)

# -- General configuration ------------------------------------------------

# If your documentation needs a minimal Sphinx version, state it here.
#
# needs_sphinx = '1.0'

# Add any Sphinx extension module names here, as strings. They can be
# extensions coming with Sphinx (named 'sphinx.ext.*') or your custom
# ones.

# sys.path.append(os.path.abspath('./ext'))
extensions = [ "breathe" ] #, "teak-versions" ], "teak-sdk" ]

breathe_projects = {
    "teak":"_doxygen/xml/",
}

breathe_default_project = "teak"

# Add any paths that contain templates here, relative to this directory.
templates_path = ['_templates']

# The suffix(es) of source filenames.
# You can specify multiple suffix as a list of string:
#
# source_suffix = ['.rst', '.md']
source_suffix = '.rst'

# The master toctree document.
master_doc = 'index'

# General information about the project.
project = u'Teak for Unity'
copyright = u'2017-2018, GoCarrot Inc'
author = u'Teak'

# The version info for the project you're documenting, acts as replacement for
# |version| and |release|, also used in various other places throughout the
# built documents.
#
# x.y.z[-short-sha]
git_version = subprocess.check_output(['git', 'describe', '--tags', '--always'])
# The short X.Y version.
version = str(git_version.split(b'-')[0], 'utf-8')
# The full version, including alpha/beta/rc tags.
release = str(git_version, 'utf-8')

# The language for content autogenerated by Sphinx. Refer to documentation
# for a list of supported languages.
#
# This is also used if you do content translation via gettext catalogs.
# Usually you set "language" from the command line for these cases.
language = None

# List of patterns, relative to source directory, that match files and
# directories to ignore when looking for source files.
# This patterns also effect to html_static_path and html_extra_path
exclude_patterns = ['_build', 'Thumbs.db', '.DS_Store', 'versions',
    'ios/index.rst', 'ios/versions', 'android/index.rst', 'android/versions']

# The name of the Pygments (syntax highlighting) style to use.
pygments_style = 'sphinx'

# If true, `todo` and `todoList` produce output, else they produce nothing.
todo_include_todos = False


# -- Options for HTML output ----------------------------------------------

# The theme to use for HTML and HTML Help pages.  See the documentation for
# a list of builtin themes.
#
import sphinx_rtd_theme
html_theme = "sphinx_rtd_theme"
html_theme_path = [sphinx_rtd_theme.get_html_theme_path()]

# Theme options are theme-specific and customize the look and feel of a theme
# further.  For a list of options available for each theme, see the
# documentation.
#
# html_theme_options = {}

# Add any paths that contain custom static files (such as style sheets) here,
# relative to this directory. They are copied after the builtin static files,
# so a file named "default.css" will overwrite the builtin "default.css".
html_static_path = ['_static']

# Custom sidebar templates, must be a dictionary that maps document names
# to template names.
#
# This is required for the alabaster theme
# refs: http://alabaster.readthedocs.io/en/latest/installation.html#sidebars
html_sidebars = {
    '**': [
        'about.html',
        'navigation.html',
        'relations.html',  # needs 'show_related': True theme option to display
        'searchbox.html',
        'donate.html',
    ]
}


# -- Options for HTMLHelp output ------------------------------------------

# Output file base name for HTML help builder.
htmlhelp_basename = 'TeakUnitydoc'


# -- Options for LaTeX output ---------------------------------------------

latex_elements = {
    # The paper size ('letterpaper' or 'a4paper').
    #
    # 'papersize': 'letterpaper',

    # The font size ('10pt', '11pt' or '12pt').
    #
    # 'pointsize': '10pt',

    # Additional stuff for the LaTeX preamble.
    #
    # 'preamble': '',

    # Latex figure (float) alignment
    #
    # 'figure_align': 'htbp',
}

# Grouping the document tree into LaTeX files. List of tuples
# (source start file, target name, title,
#  author, documentclass [howto, manual, or own class]).
latex_documents = [
    (master_doc, 'TeakUnity.tex', u'Teak for Unity Documentation',
     u'Teak', 'manual'),
]


# -- Options for manual page output ---------------------------------------

# One entry per manual page. List of tuples
# (source start file, name, description, authors, manual section).
man_pages = [
    (master_doc, 'teakunity', u'Teak for Unity Documentation',
     [author], 1)
]


# -- Options for Texinfo output -------------------------------------------

# Grouping the document tree into Texinfo files. List of tuples
# (source start file, target name, title, author,
#  dir menu entry, description, category)
texinfo_documents = [
    (master_doc, 'TeakUnity', u'Teak for Unity Documentation',
     author, 'TeakUnity', 'One line description of project.',
     'Miscellaneous'),
]

####
# Global include
with open('global.irst', 'r') as f:
    rst_prolog = f.read()

#####
# Setup
def cmp_versions(a, b):
    v = re.compile('^(\d+)\.(\d+)\.(\d+)')
    av = list(list(map(lambda x: map(int, x), v.findall(a)))[0])
    bv = list(list(map(lambda x: map(int, x), v.findall(b)))[0])

    if av[0] > bv[0]: return 1
    elif av[0] < bv[0]: return -1
    elif av[1] > bv[1]: return 1
    elif av[1] < bv[1]: return -1
    elif av[2] > bv[2]: return 1
    elif av[2] < bv[2]: return -1
    return 0

def ensure_versions():
    ios = set(os.listdir('ios/versions'))
    android = set(os.listdir('android/versions'))
    sdk = set(os.listdir('versions'))

    if len(ios.difference(android)) != 0:
        raise Exception('Android versions missing: %s' % ios.difference(android))
    if len(android.difference(ios)) != 0:
        raise Exception('iOS versions missing: %s' % android.difference(ios))
    if len(android.difference(sdk)):
        raise Exception('Versions missing: %s' % android.difference(sdk))

def mkdir_p(path):
    try:
        os.makedirs(path)
    except OSError as exc:  # Python >2.5
        if exc.errno == errno.EEXIST and os.path.isdir(path):
            pass
        else:
            raise

def gen_version(v):
    version = os.path.splitext(v)[0]
    mkdir_p('_versions')

    with open('versions/%s.rst' % version, 'r') as f:
        contents = f.read()

    contents += '\nAndroid\n^^^^^^^\n\n'
    with open('android/versions/%s.rst' % version, 'r') as f:
        data = f.read().splitlines(True)
        contents += '\n'.join(map(lambda ln: '    %s' % ln, data[2:]))

    contents += '\niOS\n^^^\n\n'
    with open('ios/versions/%s.rst' % version, 'r') as f:
        data = f.read().splitlines(True)
        contents += '\n'.join(map(lambda ln: '    %s' % ln, data[2:]))

    if os.path.isfile('_versions/%s.rst' % version):
        with open('_versions/%s.rst' % version, 'r') as f:
            prev = f.read()
    else:
        prev = ''

    if contents != prev:
        with open('_versions/%s.rst' % version, 'w') as f:
            f.write(contents)


def setup(app):
    # Ensure all versions of iOS/Android/X have entries
    ensure_versions()

    # Make array of includes
    versions = []
    for version in sorted(os.listdir('versions'), key = cmp_to_key(cmp_versions), reverse=True):
        gen_version(version)
        versions.append('.. include:: _versions/{0}'.format(version))

    changelog = """
Changelog
=========
%s
""" % '\n'.join(versions)

    if os.path.isfile('changelog.rst'):
        with open('changelog.rst', 'r') as f:
            data = f.read()
    else:
        data = ''

    if data != changelog:
        with open('changelog.rst', 'w') as f:
            f.write(changelog)
