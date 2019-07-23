import Vue from 'vue'
import VueRouter from 'vue-router'
import { IAuthenticationService } from '@/services/interfaces'
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "./inversify.config";

Vue.use(VueRouter)

function load(componentPath: string) {
    // '@' is aliased to src/components
    return () => import(`./views/${componentPath}`)
}

const routes = [
    { path: '/', component: load('landing.vue') },
    { path: '/registration', component: load('registration.vue') },
    {
        path: '/home',
        component: load('home/home.vue'),
        meta: { requiresAuth: true, roles: ['user'] },
        children: [
            { path: 'immunizations', component: load('immunizations.vue'), meta: { requiresAuth: true, roles: ['user'] } }
        ]
    },
    {
        path: '/logout',
        component: load('logout.vue'),
        meta: { requiresAuth: true, roles: ['user'] }
    },
    { path: '/unauthorized', component: load('errors/unauthorized.vue') }, // Unauthorized
    { path: '/notFound', component: load('errors/notFound.vue') },
    { path: '/Auth/Login' },
    { path: '*', redirect: '/notFound' } // Not found; Will catch all other paths not covered previously
]

const router = new VueRouter({
    mode: 'history',
    routes
})

router.beforeEach((to, from, next) => {
    if (to.meta.requiresAuth) {
        //const auth = store.state.security.auth
        const auth = { authenticated: false };//store.state.security.auth
        console.log('Requires auth!');

        const authService: IAuthenticationService = container.get<IAuthenticationService>(SERVICE_IDENTIFIER.AuthenticationService);
        authService.getBearerToken().then(bearerToken => {
            console.log(bearerToken.token);

            if (!bearerToken.isAuthenticated) {
                //security.init(next, to.meta.roles)
                console.log('Not authenticated');
                authService.startLoginFlow('aHint', to.path);
            }
            else {
                console.log('Authenticated');
                if (to.meta.roles) {
                    /*if (security.roles(to.meta.roles[0])) {
                        next()
                    }
                    else*/ {
                        next({ name: 'unauthorized' })
                    }
                }
                else {
                    next()
                }
            }
        }).catch(something => { 
            console.log('ERROR HERE' + something);
     });
    }
    else {
        next()
    }
})

export default router