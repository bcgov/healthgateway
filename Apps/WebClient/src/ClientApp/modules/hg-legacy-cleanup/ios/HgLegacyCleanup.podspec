Pod::Spec.new do |s|
  s.name           = 'HgLegacyCleanup'
  s.version        = '1.0.0'
  s.summary        = 'Removes the data the old Health Gateway apps left on the device'
  s.description    = s.summary
  s.author         = 'Province of British Columbia'
  s.homepage       = 'https://github.com/bcgov/healthgateway'
  s.license        = 'Apache-2.0'
  s.platforms      = { :ios => '15.1' }
  s.source         = { git: '' }
  s.static_framework = true
  s.dependency 'ExpoModulesCore'
  s.frameworks     = 'Security', 'UIKit'
  s.source_files   = '**/*.swift'
end
