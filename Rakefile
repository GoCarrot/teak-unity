# frozen_string_literal: true

require 'rake/clean'
require 'shellwords'
require 'tmpdir'
require 'yaml'
require 'mustache'
require 'pathname'
require 'unitypackage'
CLEAN.include '**/.DS_Store'

KMS_KEY = `aws kms decrypt --ciphertext-blob fileb://kms/store_encryption_key.key --output text --query Plaintext | base64 --decode`.freeze
CIRCLE_TOKEN = ENV.fetch('CIRCLE_TOKEN') { `openssl enc -md MD5 -d -aes-256-cbc -in kms/encrypted_circle_ci_key.data -k #{KMS_KEY}` }

TEAK_SDK_VERSION = `git describe --tags`.strip
NATIVE_CONFIG = YAML.load_file('native.config.yml')

PROJECT_PATH = Rake.application.original_dir
BUILD_TYPE = ENV.fetch('BUILD_TYPE', 'Release')

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
    # Write Unity SDK version information to 'res/values/teak_unity_version.xml'
    template = File.read(File.join(PROJECT_PATH, 'Templates', 'teak_unity_version.xml.template'))
    path = File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'Android', 'teak-version-information.androidlib', 'res', 'values')
    mkdir_p path
    File.write(File.join(path, 'teak_unity_version.xml'), Mustache.render(template, TEMPLATE_PARAMETERS))

    Dir.chdir(File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'Android')) do
      # Download or copy Teak SDK AAR
      if build_local?
        cp "#{PROJECT_PATH}/../teak-android/build/outputs/aar/teak-debug.aar", 'teak.aar'
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
      sh "curl --fail -o #{content_plist} https://sdks.teakcdn.com/ios/Info.plist"
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
    # Ensure repo exists
    unless Dir.exist? UPM_PACKAGE_REPO
      `git clone git@github.com:GoCarrot/upm-package-teak.git #{UPM_PACKAGE_REPO}`
    end

    # package.json
    template = File.read(File.join(PROJECT_PATH, 'Templates', 'package.json.template'))
    File.write(File.join(PROJECT_PATH, UPM_PACKAGE_REPO, 'package.json'), Mustache.render(template, TEMPLATE_PARAMETERS))

    # Changelog
    # cd 'docs' do
    #   `make html`
    #   `pandoc -f rst -t gfm -o ../#{UPM_PACKAGE_REPO}/CHANGELOG.md changelog.rst`
    # end

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

    copy_glob_to(editor_glob, File.join(UPM_PACKAGE_REPO, 'Editor'), 'Assets/Teak/Editor')
    copy_glob_to(runtime_glob, File.join(UPM_PACKAGE_REPO, 'Runtime'), 'Assets/Teak')
  end

  task :deploy do
    # package.json
    template = File.read(File.join(PROJECT_PATH, 'Templates', 'package.json.template'))
    File.write(File.join(PROJECT_PATH, UPM_PACKAGE_REPO, 'package.json'), Mustache.render(template, TEMPLATE_PARAMETERS))

    cd 'upm-package-teak' do
      `git config user.email "team@teak.io"`
      `git config user.name "Teak CI"`
      `git checkout -b $PVERSION`
      `git add -A ; git commit -am "$PVERSION"`
      `GIT_SSH_COMMAND='ssh -i ~/.ssh/id_rsa_37d2d909bc0dc341f4685879809cf578' git push --set-upstream origin $PVERSION`
    end
  end
end
