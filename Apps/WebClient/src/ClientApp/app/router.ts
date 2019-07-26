import Vue from 'vue'
import VueRouter, { Route, RouteConfig, RawLocation } from 'vue-router'
import store from './store'
import HomeComponent from '../app/views/home.vue'
import RegistrationComponent from '../app/views/registration.vue'
import LandingComponent from '../app/views/landing.vue'
import ImmunizationsComponent from '../app/views/immunizations.vue'
import NotFoundComponent from '../app/views/errors/notFound.vue'
import LoginComponent from '../app/views/login.vue'
import LogoutComponent from '../app/views/logout.vue'
import UnauthorizedComponent from '../app/views/errors/unauthorized.vue'
import TempAuthComponent from '../app/views/auth.vue'
import { Function } from '@babel/types';

Vue.use(VueRouter)

function load(componentPath: string) {
    // '@' is aliased to src/components, so we can't use it with our structure
    return () => import(`./views/${componentPath}`)
}

const routes: RouteConfig[] = [
    {
        path: '/',
        component: LandingComponent,
        meta: { requiresAuth: false }
    },
    {
        path: '/registration',
        component: RegistrationComponent,
        meta: { requiresAuth: false }
    },
    {
        name: 'home',
        path: '/home',
        component: HomeComponent,
        meta: { requiresAuth: true, roles: ['user'] },
        children: [
            {
                path: '/immunizations',
                component: ImmunizationsComponent,
                meta: { requiresAuth: true, roles: ['user'] }
            }
        ]
    },
    {
        path: '/auth',
        component: TempAuthComponent,
        meta: { requiresAuth: false }
    },
    {
        path: '/login',
        component: LoginComponent,
        meta: { requiresAuth: false, roles: ['user'] },
    },
    {
        path: '/logout',
        component: LogoutComponent,
        meta: { requiresAuth: true, roles: ['user'] },
    },
    {
        path: '/unauthorized',
        component: UnauthorizedComponent,
        meta: { requiresAuth: false }
    }, // Unauthorized
    { path: '*', component: NotFoundComponent } // Not found; Will catch all other paths not covered previously
]


const router = new VueRouter({
    mode: 'history',
    routes
})

export async function checkRoute(to: Route, from: Route, next: (to?: RawLocation | false | ((vm: Vue) => any) | void) => void) {
    if (to.meta.requiresAuth) {
        let isAuthenticated = store.getters.isAuthenticated;
        console.log('Requires auth!');
        if (!isAuthenticated) {
            //security.init(next, to.meta.roles)
            next({ name: 'unauthenticated' })
            console.log('Not authenticated');
            console.log(to.path);
            // commit the to.path to vuex here
            store.commit('requestedRoute', to.path);
            next({ name: 'login' });
        }
        else {
            console.log('Authenticated');
            if (to.path === '/login') {
                // if the user went to login but is already logged in, go to home instead
                next({ name: 'home' })
            }
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
            next()
        }
    }
    else {
        next()
    }
}

router.beforeEach(checkRoute);

export default router