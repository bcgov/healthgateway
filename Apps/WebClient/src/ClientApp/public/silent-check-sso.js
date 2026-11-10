// Keycloak's silent check-sso (src/services/auth/authService.impl.web.ts): tell the app the result. A file rather than
// an inline script, so the WebClient's Content-Security-Policy ('self') allows it without listing a hash.
parent.postMessage(location.href, location.origin);
