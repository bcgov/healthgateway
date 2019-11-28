import Vue from "vue";

// Routes
import VueRouter, { Route } from "vue-router";
import store from "./store/store";
import ProfileComponent from "@/views/profile.vue";
import LandingComponent from "@/views/landing.vue";
import NotFoundComponent from "@/views/errors/notFound.vue";
import LoginComponent from "@/views/login.vue";
import LogoutComponent from "@/views/logout.vue";
import UnauthorizedComponent from "@/views/errors/unauthorized.vue";
import LoginCallback from "@/views/loginCallback.vue";
import RegistrationComponent from "@/views/registration.vue";
import RegistrationInfoComponent from "@/views/registrationInfo.vue";
import TimelineComponent from "@/views/timeline.vue";
import ValidateEmailComponent from "@/views/validateEmail.vue";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    component: LandingComponent,
    meta: { requiresAuth: false }
  },
  {
    path: "/registrationInfo",
    component: RegistrationInfoComponent,
    props: route => ({
      inviteKey: route.query.inviteKey,
      email: route.query.email
    }),
    meta: { requiresAuth: false }
  },
  {
    path: "/registration",
    component: RegistrationComponent,
    props: route => ({
      inviteKey: route.query.inviteKey,
      email: route.query.email
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
  console.log(to.path);
  if (to.meta.requiresAuth || to.meta.requiresRegistration) {
    store.dispatch("auth/oidcCheckAccess", to).then(hasAccess => {
      if (!hasAccess) {
        next({ path: "/login", query: { redirect: to.path } });
      } else {
        handleUserHasAccess(to, from, next);
      }
    });
  } else {
    next();
  }
});

function handleUserHasAccess(to: Route, from: Route, next: any) {
  // If the user is registerd and is attempting to go to the registration flow pages, re-route to the timeline.
  let userIsRegistered: boolean = store.getters["user/userIsRegistered"];
  if (userIsRegistered && to.path.startsWith("/registration")) {
    next({ path: "/timeline" });
  } else if (to.meta.requiresRegistration && !userIsRegistered) {
    next({ path: "/unauthorized" });
  } else {
    next();
  }
}

export default router;
