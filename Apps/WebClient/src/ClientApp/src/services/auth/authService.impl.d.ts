// Type façade for the platform-specific implementations (authService.impl.web.ts / .native.ts),
// which Metro picks by platform extension.
import type { OpenIdConnectConfiguration } from "@/models/configData";

import type { IAuthService } from "./authService";

export function createAuthService(
    oidcConfig: OpenIdConnectConfiguration
): IAuthService;
