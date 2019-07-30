import router from '../app/router'
import HomeComponent from '../app/views/home.vue'
import RegistrationComponent from '../app/views/registration.vue'
import LandingComponent from '../app/views/landing.vue'
import ImmunizationsComponent from '../app/views/immunizations.vue'
import NotFoundComponent from '../app/views/errors/notFound.vue'
import LoginComponent from '../app/views/login.vue'
import LogoutComponent from '../app/views/logout.vue'
import UnauthorizedComponent from '../app/views/errors/unauthorized.vue'

describe('Router', () => {
  test('has landing route', () => {
    const actualComponent = router.getMatchedComponents("/")[0];
    expect(actualComponent.name).toBe(LandingComponent.name);
  });

  test('has home route', () => {
    const result = router.resolve("/home");
    expect(result.resolved.matched[0].components.default.name).toBe(HomeComponent.name);
    expect(result.route.meta.requiresAuth).toBe(true);
  });

  test('has registration route', () => {
    const actualComponent = router.getMatchedComponents("/registration")[0];
    expect(actualComponent.name).toBe(RegistrationComponent.name);
  });

  test('has immunizations route', () => {
    const actualComponent = router.getMatchedComponents("/immunizations")[0];
    expect(actualComponent.name).toBe(ImmunizationsComponent.name);
  });

  test('has logout route', () => {
    const actualComponent = router.getMatchedComponents("/logout")[0];
    expect(actualComponent.name).toBe(LogoutComponent.name);
  });

  test('has login route', () => {
    const actualComponent = router.getMatchedComponents("/login")[0];
    expect(actualComponent.name).toBe(LoginComponent.name);
  });

  test('has unauthorized route', () => {
    const actualComponent = router.getMatchedComponents("/unauthorized")[0];
    expect(actualComponent.name).toBe(UnauthorizedComponent.name);
  });
  
  test('handles unexisting route', () => {
    const actualComponent = router.getMatchedComponents("/paththatdoesnotexist")[0];
    expect(actualComponent.name).toBe(NotFoundComponent.name);
  });
});

