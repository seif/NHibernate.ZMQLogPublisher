$: << './'
require 'albacore'

require 'build/utils'
require 'build/version'

desc 'generate common assembly info'
assemblyinfo :assemblyinfo => ["version:set_values"] do |asm|
  data = git_hash_and_date()
  asm.version = ASSEMBLY_VERSION
  asm.file_version = SEM_VER
  asm.custom_attributes :AssemblyInformationalVersion => "#{BUILD_VERSION}-#{data[0]}-#{data[1]}"
  asm.output_file = 'src/CommonAssemblyInfo.cs'
end

desc "Run a sample build using the MSBuildTask"
msbuild :msbuild do |msb|
  msb.properties = { :configuration => "Release" , :platform => "x86", :outdir => File.join(File.dirname(__FILE__), "output/") }
  msb.targets = [ :Clean, :Build ]
  msb.solution = "src/NHibernate.ZMQLogPublisher.sln"
  msb.log_level = :verbose
end

task :default  => ["assemblyinfo", "msbuild"]