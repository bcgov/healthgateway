import Vue from 'vue'
import VueRouter from 'vue-router'

Vue.use(VueRouter)

function load(component) {
    // '@' is aliased to src/components
    return () => import(`./components/${component}/${component}.vue`)
}

const routes = [
    { path: '/', component: load('landing') },
    { path: '/registration', component: load('registration') },
    {
        path: '/home',
        component: load('home'),
        meta: { requiresAuth: true, roles: ['user'] },
        children: [
            { path: 'immunizations', component: load('immunizations'), meta: { requiresAuth: true, roles: ['user'] } }
        ]
    },
    {
        path: '/logout',
        component: load('logout'),
        meta: { requiresAuth: true, roles: ['user'] }
    },
    { path: '*', component: load('Error404') }, // Not found
    { path: '/unauthorized', name: 'Unauthorized', component: load('Unauthorized') } // Unauthorized
]

const router = new VueRouter({
    mode: 'history',
    scrollBehavior: () => ({ y: 0 }),
    routes
})

router.beforeEach((to, from, next) => {
    if (to.meta.requiresAuth) {
        //const auth = store.state.security.auth
        const auth = '';//store.state.security.auth
        console.log('Requires auth!');
        if (!auth.authenticated) {
            //security.init(next, to.meta.roles)
            console.log('Not authenticated');
        }
        else {
            console.log('Authenticated');
            if (to.meta.roles) {
                if (security.roles(to.meta.roles[0])) {
                    next()
                }
                else {
                    next({ name: 'unauthorized' })
                }
            }
            else {
                next()
            }
        }
    }
    else {
        next()
    }
})

export default router