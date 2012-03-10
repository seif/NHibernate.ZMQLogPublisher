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

task :default  => ["assemblyinfo"]