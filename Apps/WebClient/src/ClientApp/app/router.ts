import Vue from "vue";

// Routes
import VueRouter from "vue-router";
import store from "./store/store";
import HomeComponent from "@/views/home.vue";
import RegistrationComponent from "@/views/registration.vue";
import LandingComponent from "@/views/landing.vue";
import ImmunizationsComponent from "@/views/immunizations.vue";
import NotFoundComponent from "@/views/errors/notFound.vue";
import LoginComponent from "@/views/login.vue";
import LogoutComponent from "@/views/logout.vue";
import UnauthorizedComponent from "@/views/errors/unauthorized.vue";

Vue.use(VueRouter);

console.log('testing something');

console.log('testing something else');
const routes = [
  {
    path: "/",
    component: LandingComponent,
    meta: { requiresAuth: false }
  },
  {
    path: "/registration",
    component: RegistrationComponent,
    meta: { requiresAuth: false }
  },
  {
    path: "/home",
    component: HomeComponent,
    meta: { requiresAuth: true, roles: ["user"] }
  },
  {
    path: "/immunizations",
    component: ImmunizationsComponent,
    meta: { requiresAuth: true, roles: ["user"] }
  },
  {
    path: "/login",
    component: LoginComponent,
    meta: { requiresAuth: false, roles: ["user"] }
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
  if (to.meta.requiresAuth) {
    let isAuthenticated = store.getters["auth/isAuthenticated"];
    if (!isAuthenticated) {
      next({ path: "/login", query: { redirect: to.path } });
    } else {
      /*if (to.meta.roles) {
                /*if (security.roles(to.meta.roles[0])) {
                    next()
                }
                else {
                    next({ name: 'unauthorized' })
                }
            }
            else {
                next()
            } */
      next();
    }
  } else {
    next();
  }
});

export default router;
