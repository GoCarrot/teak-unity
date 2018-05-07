require "rake/clean"
require "httparty"
require "shellwords"
CLEAN.include "**/.DS_Store"

desc "Build Unity package"
task :default

CIRCLE_TOKEN = ENV.fetch('CIRCLE_TOKEN') { `aws kms decrypt --ciphertext-blob fileb://kms/encrypted_circle_ci_key.data --output text --query Plaintext | base64 --decode` }
UNITY_HOME="#{ENV['UNITY_HOME'] || '/Applications/Unity'}"

PROJECT_PATH = Rake.application.original_dir

#
# Play a sound after finished
#
at_exit do
  sh "afplay /System/Library/Sounds/Submarine.aiff" unless ci?
  if ci?
    add_unity_log_to_artifacts
    sh "#{UNITY_HOME}/Unity.app/Contents/MacOS/Unity -batchmode -quit -returnlicense", verbose: false rescue nil
    puts "Released Unity license..."
  end
end

#
# Helper methods
#

def ci?
  ENV.fetch('CI', false).to_s == 'true'
end

def add_unity_log_to_artifacts
  cp('unity.log', "#{Rake.application.current_task.name.sub(':', '-')}.unity.log") unless $!.nil?
end

def unity(*args, quit: true, nographics: true)
  args.push("-serial", ENV["UNITY_SERIAL"], "-username", ENV["UNITY_EMAIL"], "-password", ENV["UNITY_PASSWORD"]) if ci?

  escaped_args = args.map { |arg| Shellwords.escape(arg) }.join(' ')
  sh "#{UNITY_HOME}/Unity.app/Contents/MacOS/Unity -logFile #{PROJECT_PATH}/unity.log#{quit ? ' -quit' : ''}#{nographics ? ' -nographics' : ''} -batchmode -projectPath #{PROJECT_PATH} #{escaped_args}", verbose: false
  ensure
    return unless ci?
    add_unity_log_to_artifacts
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

    unity "-executeMethod", "TeakPackageBuilder.BuildUnityPackage"

    begin
      sh "python extractunitypackage.py Teak.unitypackage _temp_pkg/", verbose: false
      FileUtils.rm_rf("_temp_pkg")
    rescue
      raise "Unity build failed"
    end
  end
end
