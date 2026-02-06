import type { ReactNode } from 'react'
import { Header } from '@/components/layout/Header'
import { Footer } from '@/components/layout/Footer'

interface MainLayoutProps {
  children: ReactNode
}

export function MainLayout({ children }: MainLayoutProps) {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      
      {/* Main content area with top padding to account for fixed header */}
      <main className="flex-1 pt-16">
        <div className="mx-auto max-w-full px-4 py-8 sm:px-6 lg:px-8 2xl:max-w-[1920px]">
          {children}
        </div>
      </main>
      
      <Footer />
    </div>
  )
}
