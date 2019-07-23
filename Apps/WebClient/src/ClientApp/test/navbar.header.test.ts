import VueRouter from 'vue-router'
import { shallowMount, createLocalVue } from '@vue/test-utils'
import HeaderComponent from '@/components/navmenu/header.vue'
import boostrapVue from 'bootstrap-vue'

describe('NavBar Header Component', () => {
  const localVue = createLocalVue();
  localVue.use(VueRouter);
  localVue.use(boostrapVue);

  const wrapper = shallowMount(HeaderComponent, { localVue });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBeTruthy()
  })

  const languageWrapper = wrapper.find("#languageSelector");

  test('has default language set', () => {
    expect(languageWrapper.attributes('text')).toBe('English'); // Checks the dropdown element text
    expect(wrapper.vm.$data.currentLanguage.code).toBe("en"); // Checks scope variable
  })

  test('updates the language localization', () => {
    const frenchLink = languageWrapper.find('#fr');
    frenchLink.trigger('click');

    expect(languageWrapper.attributes('text')).toBe("French"); // Checks the dropdown element text
    expect(wrapper.vm.$data.currentLanguage.code).toBe("fr"); // Checks scope variable
  })  
})

