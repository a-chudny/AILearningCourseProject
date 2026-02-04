import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { Footer } from '@/components/layout/Footer'

function renderFooter() {
  return render(
    <BrowserRouter>
      <Footer />
    </BrowserRouter>
  )
}

describe('Footer', () => {
  it('renders footer with copyright', () => {
    renderFooter()
    
    const currentYear = new Date().getFullYear()
    expect(screen.getByText(new RegExp(`${currentYear} Volunteer Event Portal`))).toBeInTheDocument()
  })

  it('renders about section', () => {
    renderFooter()
    
    expect(screen.getByRole('heading', { name: 'About' })).toBeInTheDocument()
    expect(screen.getByText(/connects passionate volunteers/i)).toBeInTheDocument()
  })

  it('renders quick links section', () => {
    renderFooter()
    
    expect(screen.getByRole('heading', { name: 'Quick Links' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Browse Events' })).toHaveAttribute('href', '/events')
    expect(screen.getByRole('link', { name: 'About Us' })).toHaveAttribute('href', '/about')
    expect(screen.getByRole('link', { name: 'Privacy Policy' })).toHaveAttribute('href', '/privacy')
    expect(screen.getByRole('link', { name: 'Terms of Service' })).toHaveAttribute('href', '/terms')
  })

  it('renders contact section', () => {
    renderFooter()
    
    expect(screen.getByRole('heading', { name: 'Contact' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /contact@volunteerportal.org/i })).toHaveAttribute('href', 'mailto:contact@volunteerportal.org')
    expect(screen.getByRole('link', { name: /\+1 \(555\) 123-4567/i })).toHaveAttribute('href', 'tel:+15551234567')
    expect(screen.getByText(/123 Community Way/i)).toBeInTheDocument()
  })

  it('renders social media links', () => {
    renderFooter()
    
    const socialLinks = screen.getAllByRole('link', { name: /Facebook|Twitter|LinkedIn/i })
    expect(socialLinks).toHaveLength(3)
    
    socialLinks.forEach(link => {
      expect(link).toHaveAttribute('target', '_blank')
      expect(link).toHaveAttribute('rel', 'noopener noreferrer')
    })
  })

  it('applies correct styling classes', () => {
    const { container } = renderFooter()
    
    const footer = container.querySelector('footer')
    expect(footer).toHaveClass('bg-gray-50', 'border-t', 'border-gray-200')
  })
})
