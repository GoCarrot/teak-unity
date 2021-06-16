Unity Package Manager
=====================
Starting with SDK 4.0, Teak is available via UPM through a GitHub repo.

The repo uses a specific branch structure so that you can manage the Teak dependency via git in the same way you would use other package management, to automatically get minor and patch version updates.

Note
----

If you see an error: "Expected `resolvedPath` to have a value" then make sure it's "io.teak.unity.sdk" instead of a different value::

    {
      "dependencies": {
        "io.teak.unity.sdk": "https://github.com/GoCarrot/upm-package-teak.git#4.0.0"
      }
    }

How UPM Versions Work
---------------------
The Teak SDK uses semantic versioning, which consists of::

    <major>.<minor>.<patch>[-preview.<id>]

For example: ``4.0.0-preview.0``

Is the first preview release of major version 4, it has no minor version or patch version.

Major, breaking changes will happen only in major versions.

If new features are added, but no major breaking change has happened, it will be in a minor version.

If only bugs are fixed, or other changes occur in a non-breaking way, it will be a patch version.

For example: if you are using version 4.0.0 then you can update to version 4.0.1 and you should see no issues (except the bugs that got fixed, or performance improvements).

The version control structure for the Teak SDK, distributed via UPM, matches this and allows you to use the version that makes the most sense for you. 

For example if you use::

    "https://github.com/GoCarrot/upm-package-teak.git#4.0.0"

You will use only version 4.0.0, even though new versions come out

If you use::

    "https://github.com/GoCarrot/upm-package-teak.git#4.0"

You will use any version of 4.0.x, so if 4.0.1 comes out, your dependency will be updated.

If you use::

    "https://github.com/GoCarrot/upm-package-teak.git#4"

Then any version of the 4.x.x series that releases will be elegable.

This is accomplished via git branches

The 'main' branch contains only minimal framework for the UPM package. There is a branch per major version, then a branch per minor version, and a tag per full version. The HEAD of each major/minor branch is kept updated to match the latest corrisponding release.

