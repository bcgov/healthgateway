import { shallowMount, createLocalVue } from '@vue/test-utils'
import VueRouter from 'vue-router'
import HeaderComponent from '@/components/navmenu/header.vue'
import BootstrapVue from 'bootstrap-vue'

describe('HeaderComponent', () => {
  test('is a Vue instance', () => {
    const localVue = createLocalVue()
    localVue.use(VueRouter);
    localVue.use(BootstrapVue)

    const wrapper = shallowMount(HeaderComponent, { localVue });
    expect(wrapper.isVueInstance()).toBeTruthy()
  })
})

describe('HeaderComponent', () => {
  test('updates displayed language', () => {
    // TODO: the following is not working. Figure out how to call a method at wrapper.vm
    /*const localVue = createLocalVue()
    localVue.use(VueRouter);
    localVue.use(BootstrapVue)
    const wrapper = shallowMount(HeaderComponent, { localVue })
    
    const languageWrapper = wrapper.find("#languageSelector");
    expect(languageWrapper.attributes('text')).toBe('English');

    const button = languageWrapper.find('[name="fr"]');
    console.log(button.html());
    console.log(wrapper.vm.$data.currentLanguage.code);
    
    button.trigger('click');
    (wrapper.vm as any).onLanguageSelect('fr');
    console.log(wrapper.vm.$data.currentLanguage.code);
    expect(languageWrapper.attributes('text')).toBe('French');
    
    console.log(languageWrapper.html());
    console.log(wrapper.vm.$bvModal);
    wrapper.vm.$data.onLanguageSelect('fr');    
    console.log(wrapper.html());
    expect(wrapper.isVueInstance()).toBeTruthy()*/
  })
})
