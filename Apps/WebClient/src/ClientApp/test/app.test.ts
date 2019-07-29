import { shallowMount, createLocalVue } from '@vue/test-utils'
import AppComponent from '@/app.vue'

describe('Home view', () => {
  const localVue = createLocalVue();
  const wrapper = shallowMount(AppComponent, { localVue });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBeTruthy();
  })
});
