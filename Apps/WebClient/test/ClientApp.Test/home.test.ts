import { mount, shallowMount, createLocalVue } from '@vue/test-utils'
import HomeComponent from '../../src/ClientApp/components/home/home.vue'

describe('Component', () => {
  test('is a Vue instance', () => {
    const localVue = createLocalVue()
    const wrapper = shallowMount(HomeComponent, { localVue })
    expect(wrapper.isVueInstance()).toBeTruthy()
  })
})