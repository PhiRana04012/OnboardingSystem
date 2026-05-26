import { describe, it, expect } from 'vitest'
import { shallowMount } from '@vue/test-utils'
import ThemeIcon from '../../../components/ThemeIcon.vue'

describe('ThemeIcon', () => {
  it('renders sun icon when theme is dark', () => {
    const wrapper = shallowMount(ThemeIcon, {
      props: {
        isDark: true,
        sizeClass: 'w-7 h-7'
      }
    })

    const svg = wrapper.find('svg')
    expect(wrapper.html()).toContain('M12 2v2.5')
    expect(svg.attributes('class')).toContain('w-7')
    expect(svg.attributes('class')).toContain('h-7')
  })

  it('renders moon icon when theme is light', () => {
    const wrapper = shallowMount(ThemeIcon, {
      props: {
        isDark: false,
        sizeClass: 'w-8 h-8'
      }
    })

    const svg = wrapper.find('svg')
    expect(wrapper.html()).toContain('M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z')
    expect(svg.attributes('class')).toContain('w-8')
    expect(svg.attributes('class')).toContain('h-8')
  })
})
