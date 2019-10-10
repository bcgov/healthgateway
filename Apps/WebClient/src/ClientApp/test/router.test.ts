import router from "../app/router";
import ProfileComponent from "../app/views/profile.vue";
import RegistrationComponent from "../app/views/registration.vue";
import LandingComponent from "../app/views/landing.vue";
import NotFoundComponent from "../app/views/errors/notFound.vue";
import LoginComponent from "../app/views/login.vue";
import LogoutComponent from "../app/views/logout.vue";
import UnauthorizedComponent from "../app/views/errors/unauthorized.vue";

describe("Router", () => {
  test("has landing route", () => {
    const actualComponent = router.getMatchedComponents("/")[0];
    expect(actualComponent.name).toBe(LandingComponent.name);
  });

  test("has profile route", () => {
    const result = router.resolve("/profile");
    expect(result.resolved.matched[0].components.default.name).toBe(
      ProfileComponent.name
    );
    expect(result.route.meta.requiresRegistration).toBe(true);
  });

  test("has registration route", () => {
    const actualComponent = router.getMatchedComponents("/registration")[0];
    expect(actualComponent.name).toBe(RegistrationComponent.name);
  });

  test("has logout route", () => {
    const actualComponent = router.getMatchedComponents("/logout")[0];
    expect(actualComponent.name).toBe(LogoutComponent.name);
  });

  test("has login route", () => {
    const actualComponent = router.getMatchedComponents("/login")[0];
    expect(actualComponent.name).toBe(LoginComponent.name);
  });

  test("has unauthorized route", () => {
    const actualComponent = router.getMatchedComponents("/unauthorized")[0];
    expect(actualComponent.name).toBe(UnauthorizedComponent.name);
  });

  test("handles unexisting route", () => {
    const actualComponent = router.getMatchedComponents(
      "/paththatdoesnotexist"
    )[0];
    expect(actualComponent.name).toBe(NotFoundComponent.name);
  });
});
