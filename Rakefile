# frozen_string_literal: true

require 'rake/clean'
require 'shellwords'
require 'tmpdir'
require 'yaml'
require 'mustache'
require 'pathname'
require 'unitypackage'
CLEAN.include '**/.DS_Store'

TEAK_SDK_VERSION = `git describe --tags`.strip
NATIVE_CONFIG = YAML.load_file('native.config.yml')

PROJECT_PATH = Rake.application.original_dir
BUILD_TYPE = ENV.fetch('BUILD_TYPE', 'Release')

UPM_BUILD_TEMP = ENV.fetch('UPM_BUILD_TEMP', 'temp-upm-build')
UPM_PACKAGE_REPO = ENV.fetch('UPM_PACKAGE_REPO', '../upm-package-teak')

TEMPLATE_PARAMETERS = {
  teak_sdk_version: TEAK_SDK_VERSION
}
#
# Helper methods
#

def ci?
  ENV.fetch('CI', false).to_s == 'true'
end

def build_local?
  ENV.fetch('BUILD_LOCAL', false).to_s == 'true'
end

task default: ['build:android', 'build:ios', 'build:package']

task :format do
  sh 'astyle --project --recursive Assets/*.cs --exclude=Assets/Teak/Editor/iOS/Xcode'
end

task :version, [:v] do |_, args|
  Rake::Task['version:ios'].invoke(args.v)
  Rake::Task['version:android'].invoke(args.v)
  Rake::Task['version:unity'].invoke(args.v)
end

namespace :version do
  require 'aws-sdk-s3'

  task :ios, [:v] do |_, args|
    s3 = Aws::S3::Resource.new(
      region: 'us-east-1'
    )
    bucket = s3.bucket('teak-build-artifacts')

    fail "Teak iOS version #{args.v} does not exist" unless bucket.object("ios/Teak-#{args.v}.framework.zip").exists?

    NATIVE_CONFIG['version']['ios'] = args.v
    File.write('native.config.yml', NATIVE_CONFIG.to_yaml)
  end

  task :android, [:v] do |_, args|
    s3 = Aws::S3::Resource.new(
      region: 'us-east-1'
    )
    bucket = s3.bucket('teak-build-artifacts')

    fail "Teak Android version #{args.v} does not exist" unless bucket.object("android/teak-#{args.v}.aar").exists?

    NATIVE_CONFIG['version']['android'] = args.v
    File.write('native.config.yml', NATIVE_CONFIG.to_yaml)
  end

  task :unity, [:v] do |_, args|
    File.write('VERSION', "#{args.v}\n")
  end
end

namespace :build do
  task :android do
    # Write Unity SDK version information
    plugins_android = File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'Android')

    template = File.read(File.join(PROJECT_PATH, 'Templates', 'Version.java.template'))
    File.write(File.join(plugins_android, 'Version.java'), Mustache.render(template, TEMPLATE_PARAMETERS))

    Dir.chdir(plugins_android) do
      # Download or copy Teak SDK AAR
      if build_local?
        cp "#{PROJECT_PATH}/../teak-android/build/outputs/aar/teak-#{BUILD_TYPE.downcase}.aar", 'teak.aar'
      else
        sh "curl -o teak.aar https://sdks.teakcdn.com/android/teak-#{NATIVE_CONFIG['version']['android']}.aar"
      end
    end
  end

  task :ios do
    # Download or copy Teak SDK
    if build_local?
      cp "#{PROJECT_PATH}/../teak-ios/build/#{BUILD_TYPE}-iphoneos/libTeak.a", File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'iOS', 'libTeak.a')
    else
      Dir.mktmpdir do |dir|
        Dir.chdir(dir) do
          sh "curl -o Teak.framework.zip https://sdks.teakcdn.com/ios/Teak-#{NATIVE_CONFIG['version']['ios']}.framework.zip"
          sh 'unzip Teak.framework.zip'
          cp 'Teak.framework/Teak', File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'iOS', 'libTeak.a')
        end
      end
    end

    # Download or copy Teak SDK Resources bundle
    Dir.mktmpdir do |dir|
      Dir.chdir(dir) do
        if build_local?
          cp "#{PROJECT_PATH}/../teak-ios/build/#{BUILD_TYPE}-iphoneos/TeakResources.bundle.zip", 'TeakResources.bundle.zip'
        else
          sh "curl -o TeakResources.bundle.zip https://sdks.teakcdn.com/ios/TeakResources-#{NATIVE_CONFIG['version']['ios']}.bundle.zip"
        end

        sh "unzip -o TeakResources.bundle.zip -d #{File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'iOS')}"
      end
    end

    # Download the latest Info.plist for TeakNotificationContent
    content_plist = File.join('Assets', 'Teak', 'Editor', 'iOS', 'TeakNotificationContent', 'Info.plist')
    if build_local?
      cp "#{PROJECT_PATH}/../teak-ios/TeakExtensions/TeakNotificationContent/Info.plist", content_plist
    else
      sh "curl --fail -o #{content_plist} https://sdks.teakcdn.com/ios/Info-#{NATIVE_CONFIG['version']['ios']}.plist"
    end

    # Write Unity SDK version information to 'Assets/Teak/Plugins/iOS/teak_version.m'
    template = File.read(File.join(PROJECT_PATH, 'Templates', 'teak_version.m.template'))
    File.write(File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'iOS', 'teak_version.m'), Mustache.render(template, TEMPLATE_PARAMETERS))
  end

  task :package do
    project_path = File.expand_path('./')
    package_path = File.expand_path('./Teak.unitypackage')

    FileUtils.rm_f(package_path)

    # TeakVersion.cs
    template = File.read(File.join(PROJECT_PATH, 'Templates', 'TeakVersion.cs.template'))
    File.write(File.join(PROJECT_PATH, 'Assets', 'Teak', 'TeakVersion.cs'), Mustache.render(template, TEMPLATE_PARAMETERS))

    package = UnityPackage::UnityPackage.new
    package << Dir['Assets/Teak/**/*']
    File.open(package_path, 'wb') do |file|
      package.write file
    end

    begin
      sh 'python extractunitypackage.py Teak.unitypackage _temp_pkg/', verbose: false
      FileUtils.rm_rf('_temp_pkg')
    rescue StandardError
      raise 'Unity build failed'
    end
  end
end

namespace :upm do
  task :build do
    FileUtils.rm_rf(UPM_BUILD_TEMP)
    FileUtils.mkdir_p(UPM_BUILD_TEMP)

    `git clone git@github.com:GoCarrot/upm-package-teak.git #{UPM_BUILD_TEMP}` if build_local?

    editor_glob = Dir.glob('Assets/Teak/Editor/**/*')

    runtime_exclude = Dir.glob('Assets/Teak/LICENSE*') + Dir.glob('Assets/Teak/Editor*') + editor_glob
    runtime_glob = Dir.glob('Assets/Teak/**/*') - runtime_exclude

    def copy_glob_to(glob, dest, hax_path)
      glob.each do |filename|
        abs_path = Pathname.new(File.expand_path(filename))
        package_root = Pathname.new(File.expand_path(hax_path))
        rel_path = abs_path.relative_path_from(package_root)

        dir = File.join(dest, File.dirname(rel_path))
        FileUtils.mkdir_p(dir)
        FileUtils.cp(filename, dir) if File.file?(filename)
      end
    end

    copy_glob_to(editor_glob, File.join(UPM_BUILD_TEMP, 'Editor'), 'Assets/Teak/Editor')
    copy_glob_to(runtime_glob, File.join(UPM_BUILD_TEMP, 'Runtime'), 'Assets/Teak')

    # package.json
    template = File.read(File.join(PROJECT_PATH, 'Templates', 'package.json.template'))
    File.write(File.join(UPM_BUILD_TEMP, 'package.json'), Mustache.render(template, TEMPLATE_PARAMETERS))
  end

  task :deploy_versioned do
    # Ensure repo exists
    unless Dir.exist? UPM_PACKAGE_REPO
      `git clone git@github.com:GoCarrot/upm-package-teak.git #{UPM_PACKAGE_REPO}`
    end

    # Construct our version.
    version_parts = TEAK_SDK_VERSION.split('-')
    version = version_parts[0]
    version_suffix = version_parts[1..-1].join('-')
    major, minor, patch = version.split('.').map(&:to_i)

    cd UPM_PACKAGE_REPO do
      sh "git config user.email \"team@teak.io\""
      sh "git config user.name \"Teak CI\""

      sh "git checkout build" # Start on the 'build' branch
      sh "git checkout #{major}.#{minor} || " +  # If the current minor version branch exists, check it out
         "(git checkout #{major}.#{minor - 1} && git checkout -b #{major}.#{minor}) || " + # Check out the previous minor revision and then create a new minor version branch off that
         "(git checkout #{major} && git checkout -b #{major}.#{minor}) || " + # If there is no previous minor version branch, check out the major version and create one
         "(git checkout #{major - 1} ; (git checkout -b #{major} && git checkout -b #{major}.#{minor}))" # New major version based on previous major version, or 'build'
      sh "rm -fr *" # Delete all files
      sh "git ls-tree --name-only -r build | xargs git checkout --" # Restore files which exist in the 'build' branch
    end

    sh "cp -RT #{UPM_BUILD_TEMP} #{UPM_PACKAGE_REPO}" # Copy in all the files

    cd UPM_PACKAGE_REPO do
      sh "git add -A" # Add all files present
      sh "git commit -am \"#{TEAK_SDK_VERSION}\"" # Commit with message set to full version
      sh "git tag #{TEAK_SDK_VERSION}" # Create a tag of the full version

      # Push versioned
      sh "git push origin #{major}.#{minor}"
      sh "git push --tags"

      # puts "git switch #{major} # Checkout branch '#{major}' or create it if it doesn't exist"
      # puts "git reset --hard #{TEAK_SDK_VERSION} # Reset the HEAD of the '#{major}' branch to the tag we just created"
      # puts "git push --all origin # Push all branches and commits"
      # puts "git push --tags # Push tags"
    end
  end

  task :deploy_latest do
    # Ensure repo exists
    unless Dir.exist? UPM_PACKAGE_REPO
      `git clone git@github.com:GoCarrot/upm-package-teak.git #{UPM_PACKAGE_REPO}`
    end

    version_parts = TEAK_SDK_VERSION.split('-')
    version = version_parts[0]
    version_suffix = version_parts[1..-1].join('-')
    major, minor, patch = version.split('.').map(&:to_i)

    cd UPM_PACKAGE_REPO do
      sh "git checkout #{major}.#{minor}"
      sh "git reset --hard #{TEAK_SDK_VERSION}" # Move the major.minor branch HEAD to the latest tag
      sh "git checkout #{major} || git checkout -b #{major}"
      sh "git reset --hard #{TEAK_SDK_VERSION}" # Move the major branch HEAD to the latest tag
      sh "git checkout main"
      sh "git reset --hard #{TEAK_SDK_VERSION}" # Move the main branch HEAD to the latest tag
      sh "git push --all"
    end
  end
end
