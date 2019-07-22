import { shallowMount, createLocalVue } from '@vue/test-utils'
import RegistrationComponent from '@/views/registration.vue'

describe('Registration view', () => {
  const localVue = createLocalVue();
  const wrapper = shallowMount(RegistrationComponent, { localVue });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBeTruthy();
  });

  const expectedH1Text = "Registration with BC Services Card";
  test(`has header element with "${expectedH1Text}" text`, () => {
    expect(wrapper.find('h1').text()).toBe(expectedH1Text);
  });
});
