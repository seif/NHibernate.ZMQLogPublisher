require 'semver'

namespace :version do
  task :set_values do
    # pad build number to 5 digits or put 2 digit year followed by day of year (0-365) if not a teamcity build
    build_number = "-%05d" % (ENV['BUILD_NUMBER'] || Time.now.strftime('%y%j'))	
	# version loaded from .semver file
	version = SemVer.find	
	# use special token(alpha, beta, rc) followed by the build number.
	version.special =  "-#{version.special}#{build_number}" unless version.special.empty?
	# BUILD_VERSION used for assembly info version and teamcity build reporting, SemVer doesn't provide for build numbers unless special token defined
    BUILD_VERSION = "#{version.major}.#{version.minor}.#{version.patch}#{(version.special.empty? ? build_number:version.special)}"	
	# Should stick to SemVer, so no point in assembly binding pain, just use the major version for .Net assembly loading.
    ASSEMBLY_VERSION = version.format("%M.0.0")	
	# Used for file version and nuget package version
	SEM_VER = version.format "%M.%m.%p%s"
	
	
	puts "Build Version: #{BUILD_VERSION}"
    puts "Assembly Version: #{ASSEMBLY_VERSION}"
	puts "Semantic Version: #{SEM_VER}"
    puts "##teamcity[buildNumber '#{BUILD_VERSION}']" # report build number to teamcity
  end
end

