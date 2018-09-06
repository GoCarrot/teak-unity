require 'rake/clean'
require 'httparty'
require 'shellwords'
require 'tmpdir'
require 'yaml'
require 'awesome_print'
CLEAN.include '**/.DS_Store'

#
# Extend Rake to have current_task
#
require 'rake'
module Rake
  class Application
    attr_accessor :current_task
  end
  class Task
    alias :old_execute :execute
    def execute(args=nil)
      Rake.application.current_task = self
      old_execute(args)
    end
  end #class Task
end #module Rake

KMS_KEY = `aws kms decrypt --ciphertext-blob fileb://kms/store_encryption_key.key --output text --query Plaintext | base64 --decode`
CIRCLE_TOKEN = ENV.fetch('CIRCLE_TOKEN') { `openssl enc -md MD5 -d -aes-256-cbc -in kms/encrypted_circle_ci_key.data -k #{KMS_KEY}` }

UNITY_HOME = ENV.fetch('UNITY_HOME', '/Applications/Unity-2017.1.0f3')
TEAK_SDK_VERSION=`git describe --tags`.strip
NATIVE_CONFIG=YAML.load_file('native.config.yml')

PROJECT_PATH = Rake.application.original_dir

#
# Play a sound after finished
#
at_exit do
  sh "afplay /System/Library/Sounds/Submarine.aiff" unless ci?
  if ci?
    add_unity_log_to_artifacts
    Rake::Task["unity:returnlicense"].invoke
  end
end

#
# Helper methods
#

def ci?
  ENV.fetch('CI', false).to_s == 'true'
end

def build_local?
  ENV.fetch('BUILD_LOCAL', false).to_s == 'true'
end

def add_unity_log_to_artifacts
  cp('unity.log', "#{Rake.application.current_task.name.sub(':', '-')}.unity.log") unless $!.nil?
end

def unity(*args, quit: true, nographics: true)
  args.push("-serial", ENV["UNITY_SERIAL"], "-username", ENV["UNITY_EMAIL"], "-password", ENV["UNITY_PASSWORD"]) if ci?

  unity_cmd = UNITY_HOME.start_with?("/Applications") ? "#{UNITY_HOME}/Unity.app/Contents/MacOS/Unity" : "#{UNITY_HOME}/Unity"
  escaped_args = args.map { |arg| Shellwords.escape(arg) }.join(' ')
  sh "#{unity_cmd} -logFile #{PROJECT_PATH}/unity.log#{quit ? ' -quit' : ''}#{nographics ? ' -nographics' : ''} -batchmode -projectPath #{PROJECT_PATH} #{escaped_args}", verbose: false
  ensure
    return unless ci?
    add_unity_log_to_artifacts
end

namespace :unity do
  task :returnlicense do
    sh "#{UNITY_HOME}/Unity.app/Contents/MacOS/Unity -batchmode -quit -returnlicense", verbose: false rescue nil
    puts "Released Unity license..."
  end
end

namespace :build do
  task :cleanroom do
    json = HTTParty.post("https://circleci.com/api/v1.1/project/github/GoCarrot/teak-unity-cleanroom/build?circle-token=#{CIRCLE_TOKEN}",
                  body: {
                    build_parameters: {
                      FL_TEAK_SDK_VERSION: `git describe --tags --always`.strip
                    }
                  }.to_json,
                  headers: {
                    'Content-Type' => 'application/json',
                    'Accept' => 'application/json'
                  }).body
    ap(JSON.parse(json))
  end

  task :android do
    Dir.mktmpdir do |dir|
      Dir.chdir(dir) do
        # Download or copy Teak SDK AAR
        if build_local? then
          cp "#{PROJECT_PATH}/../teak-android/build/outputs/aar/teak-release.aar", "teak.aar"
        else
          sh "curl -o teak.aar https://s3.amazonaws.com/teak-build-artifacts/android/teak-#{NATIVE_CONFIG['version']['android']}.aar"
        end

        # Unzip AAR, delete original AAR
        sh 'unzip teak.aar'
        rm 'teak.aar'

        # Write Unity SDK version information to 'res/values/teak_unity_version.xml'
versionfile = <<END
<?xml version="1.0" encoding="utf-8"?>
<resources>
  <string name="io_teak_wrapper_sdk_name">unity</string>
  <string name="io_teak_wrapper_sdk_version">#{TEAK_SDK_VERSION}</string>
</resources>
END
        File.open(File.join('res', 'values', 'teak_unity_version.xml'), 'w') do |file|
          file.write(versionfile)
        end

        # Re-package AAR
        sh "jar cf #{File.join(PROJECT_PATH, 'Assets', 'Plugins', 'Android', 'teak.aar')} ."
      end
    end
  end

  task :ios do
    # Download or copy Teak SDK
    if build_local? then
      cp "#{PROJECT_PATH}/../teak-ios/build/Release-iphoneos/libTeak.a", File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'iOS', 'libTeak.a')
    else
      Dir.mktmpdir do |dir|
        Dir.chdir(dir) do
          sh "curl -o Teak.framework.zip https://s3.amazonaws.com/teak-build-artifacts/ios/Teak-#{NATIVE_CONFIG['version']['ios']}.framework.zip"
          sh 'unzip Teak.framework.zip'
          cp 'Teak.framework/Teak', File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'iOS', 'libTeak.a')
        end
      end
    end

    # Download or copy Teak SDK Resources bundle
    Dir.mktmpdir do |dir|
      Dir.chdir(dir) do
        if build_local? then
          cp "#{PROJECT_PATH}/../teak-ios/build/Release-iphoneos/TeakResources.bundle.zip", 'TeakResources.bundle.zip'
        else
          sh "curl -o TeakResources.bundle.zip https://s3.amazonaws.com/teak-build-artifacts/ios/TeakResources-#{NATIVE_CONFIG['version']['ios']}.bundle.zip"
        end

        sh "unzip -o TeakResources.bundle.zip -d #{File.join(PROJECT_PATH, 'Assets', 'Teak', 'Plugins', 'iOS')}"
      end
    end

    # Write Unity SDK version information to 'Assets/Teak/Plugins/iOS/teak_version.m'
versionfile = <<END
NSString* TeakUnitySDKVersion = @"#{TEAK_SDK_VERSION}";
END
    File.open(File.join('Assets', 'Teak', 'Plugins', 'iOS', 'teak_version.m'), 'w') do |file|
      file.write(versionfile)
    end
  end

  task :package do
    project_path = File.expand_path("./")
    package_path = File.expand_path("./Teak.unitypackage")

    FileUtils.rm_f(package_path)

versionfile = <<END
/* THIS FILE IS AUTOMATICALLY GENERATED, DO NOT MODIFY IT. */
public class TeakVersion
{
    public static string Version
    {
        get
        {
            return "#{`git describe --tags`.strip}";
        }
    }
}
END

    File.open(File.join(project_path, "Assets", "Teak", "TeakVersion.cs"), 'w') do |file|
      file.write(versionfile)
    end

    unity "-executeMethod", "TeakPackageBuilder.BuildUnityPackage"

    begin
      sh "python extractunitypackage.py Teak.unitypackage _temp_pkg/", verbose: false
      FileUtils.rm_rf("_temp_pkg")
    rescue
      raise "Unity build failed"
    end
  end
end
