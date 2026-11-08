Pod::Spec.new do |s|
  s.name           = 'HgSecureVault'
  s.version        = '1.0.0'
  s.summary        = 'Device-security-protected encryption key for the Health Gateway record cache'
  s.description    = s.summary
  s.author         = 'Province of British Columbia'
  s.homepage       = 'https://github.com/bcgov/healthgateway'
  s.license        = 'Apache-2.0'
  s.platforms      = { :ios => '15.1' }
  s.source         = { git: '' }
  s.static_framework = true
  s.dependency 'ExpoModulesCore'
  s.frameworks     = 'CryptoKit', 'LocalAuthentication', 'Security'
  s.source_files   = '**/*.swift'
end
