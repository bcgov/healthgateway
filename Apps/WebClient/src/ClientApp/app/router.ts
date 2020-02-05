import Vue from "vue";

// Routes
import VueRouter, { Route } from "vue-router";
import store from "./store/store";
import { SnowplowWindow } from "@/extensions";
declare let window: SnowplowWindow;

const ProfileComponent = () =>
  import(/* webpackChunkName: "profile" */ "@/views/profile.vue");
const LandingComponent = () =>
  import(/* webpackChunkName: "landing" */ "@/views/landing.vue");
const NotFoundComponent = () =>
  import(/* webpackChunkName: "notFound" */ "@/views/errors/notFound.vue");
const LoginComponent = () =>
  import(/* webpackChunkName: "login" */ "@/views/login.vue");
const LogoutComponent = () =>
  import(/* webpackChunkName: "logout" */ "@/views/logout.vue");
const UnauthorizedComponent = () =>
  import(
    /* webpackChunkName: "unauthorized" */ "@/views/errors/unauthorized.vue"
  );
const LoginCallback = () =>
  import(/* webpackChunkName: "loginCallback" */ "@/views/loginCallback.vue");
const RegistrationComponent = () =>
  import(/* webpackChunkName: "registration" */ "@/views/registration.vue");
const RegistrationInfoComponent = () =>
  import(
    /* webpackChunkName: "registrationInfo" */ "@/views/registrationInfo.vue"
  );
const TimelineComponent = () =>
  import(/* webpackChunkName: "timeline" */ "@/views/timeline.vue");
const ValidateEmailComponent = () =>
  import(/* webpackChunkName: "validateEmail" */ "@/views/validateEmail.vue");

Vue.use(VueRouter);

const REGISTRATION_PATH = "/registration";
const REGISTRATION_INFO_PATH = "/registrationInfo";

const routes = [
  {
    path: "/",
    component: LandingComponent,
    meta: { requiresAuth: false }
  },
  {
    path: REGISTRATION_INFO_PATH,
    component: RegistrationInfoComponent,
    props: (route: Route) => ({
      inviteKey: route.query.inviteKey,
      email: route.query.email
    }),
    meta: { requiresAuth: false }
  },
  {
    path: REGISTRATION_PATH,
    component: RegistrationComponent,
    props: (route: Route) => ({
      inviteKey: route.query.inviteKey,
      inviteEmail: route.query.email
    }),
    meta: { requiresAuth: true }
  },
  {
    path: "/validateEmail/:inviteKey",
    component: ValidateEmailComponent,
    props: true,
    meta: { requiresAuth: true }
  },
  {
    path: "/profile",
    component: ProfileComponent,
    meta: { requiresRegistration: true, roles: ["user"] }
  },
  {
    path: "/timeline",
    component: TimelineComponent,
    meta: { requiresRegistration: true, roles: ["user"] }
  },
  {
    path: "/login",
    component: LoginComponent,
    props: (route: Route) => ({
      isRetry: route.query.isRetry
    }),
    meta: { requiresAuth: false, roles: ["user"] }
  },
  {
    path: "/loginCallback",
    component: LoginCallback,
    meta: { requiresAuth: false, roles: ["user"], routeIsOidcCallback: true }
  },
  {
    path: "/logout",
    component: LogoutComponent,
    meta: { requiresAuth: false }
  },
  {
    path: "/unauthorized",
    component: UnauthorizedComponent,
    meta: { requiresAuth: false }
  }, // Unauthorized
  { path: "/*", component: NotFoundComponent } // Not found; Will catch all other paths not covered previously
];

const router = new VueRouter({
  mode: "history",
  routes
});

router.beforeEach(async (to, from, next) => {
  console.log(from.fullPath, to.fullPath);
  if (to.meta.requiresAuth || to.meta.requiresRegistration) {
    store.dispatch("auth/oidcCheckAccess", to).then(hasAccess => {
      if (!hasAccess) {
        next({ path: "/login", query: { redirect: to.fullPath } });
      } else {
        handleUserHasAccess(to, from, next);
      }
    });
  } else {
    let userIsAuthenticated: boolean =
      store.getters["auth/oidcIsAuthenticated"];
    let userIsRegistered: boolean = store.getters["user/userIsRegistered"];

    // If the user is authenticated but not registered, the registration must be completed
    let isRegistrationPath =
      to.path.startsWith(REGISTRATION_PATH) ||
      to.path.startsWith(REGISTRATION_INFO_PATH);
    if (
      userIsAuthenticated &&
      !userIsRegistered &&
      !to.meta.routeIsOidcCallback &&
      !to.path.startsWith("/logout") &&
      !isRegistrationPath
    ) {
      next({ path: REGISTRATION_PATH });
    } else {
      next();
    }
  }
});

router.afterEach((to, from) => {
  window.snowplow("trackPageView");
});

function handleUserHasAccess(to: Route, from: Route, next: any) {
  // If the user is registerd and is attempting to go to the registration flow pages, re-route to the timeline.
  let userIsRegistered: boolean = store.getters["user/userIsRegistered"];
  if (userIsRegistered && to.path.startsWith(REGISTRATION_PATH)) {
    next({ path: "/timeline" });
  } else if (to.meta.requiresRegistration && !userIsRegistered) {
    next({ path: REGISTRATION_PATH });
  } else {
    next();
  }
}

export default router;
