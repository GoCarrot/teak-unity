require "rake/clean"
require "httparty"
CLEAN.include "**/.DS_Store"

desc "Build Unity package"
task :default

CIRCLE_TOKEN = ENV.fetch('CIRCLE_TOKEN') { `aws kms decrypt --ciphertext-blob fileb://kms/encrypted_circle_ci_key.data --output text --query Plaintext | base64 --decode` }
UNITY_HOME="#{ENV['UNITY_HOME'] || '/Applications/Unity'}"

PROJECT_PATH = Rake.application.original_dir

#
# Helper methods
#
def unity(*args)
  # Run Unity.
  sh "#{UNITY_HOME}/Unity.app/Contents/MacOS/Unity -logFile #{PROJECT_PATH}/unity.log #{args.join(' ')}"
end

def unity?
  # Return true if we can run Unity.
  File.exist? "#{UNITY_HOME}/Unity.app/Contents/MacOS/Unity"
end

# Docs task
DOXYGEN_BINARY = "/Applications/Doxygen.app/Contents/Resources/doxygen"

def doxygen?
  return if not File.exist?(DOXYGEN_BINARY)
  return true
end

if doxygen?
  task :docs do
    sh DOXYGEN_BINARY
  end
end

namespace :build do
  task :cleanroom do
    HTTParty.post("https://circleci.com/api/v1.1/project/github/GoCarrot/teak-unity-cleanroom/tree/master?circle-token=#{CIRCLE_TOKEN}",
                  body: {
                    build_parameters: {
                      FL_TEAK_SDK_VERSION: `git describe --tags --always`.strip
                    }
                  }.to_json,
                  headers: {
                    'Content-Type' => 'application/json',
                    'Accept' => 'application/json'
                  })
  end
end

#
# Unity build tasks
#

task :default => "unity:package"

desc "Build Unity Package"
task :unity => "unity:package"
namespace :unity do
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

    begin
      unity "-quit -batchmode -nographics -projectPath #{project_path} -executeMethod TeakPackageBuilder.BuildUnityPackage"
    rescue
      # Unity tends to crash on exit for some reason, so just ignore it
    end

    begin
      sh "python extractunitypackage.py Teak.unitypackage _temp_pkg/"
      FileUtils.rm_rf("_temp_pkg")
    rescue
      sh ">&2 cat unity.log"
      raise "Unity build failed"
    end
  end
end
